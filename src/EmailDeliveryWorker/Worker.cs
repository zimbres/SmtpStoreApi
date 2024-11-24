namespace EmailDeliveryWorker;

public class Worker : BackgroundService
{
    private readonly Settings _settings;
    private readonly ILogger<Worker> _logger;
    private readonly HttpClient _httpClient;

    public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _settings = configuration.GetSection("Settings").Get<Settings>()!;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(_settings.ApiBaseAddress);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Delivery Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var emailFiles = await _httpClient.GetFromJsonAsync<Response>("/emails", stoppingToken);
                if (emailFiles == null || emailFiles.Emails.Count == 0)
                {
                    await Task.Delay(_settings.Delay * 1000, stoppingToken);
                    continue;
                }

                if (emailFiles.DeliveryEnabled && emailFiles.Emails.Count != 0)
                {
                    foreach (var fileName in emailFiles.Emails)
                    {
                        await ProcessEmailAsync(fileName, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing emails.");
            }

            await Task.Delay(_settings.Delay * 1000, stoppingToken);
        }

        _logger.LogInformation("EmailDeliveryWorker stopped.");
    }

    private async Task ProcessEmailAsync(string fileName, CancellationToken stoppingToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/email/{fileName}", stoppingToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to fetch email {fileName}: {response.StatusCode}");
                return;
            }

            var emailBytes = await response.Content.ReadAsByteArrayAsync(stoppingToken);

            var email = await MimeMessage.LoadAsync(new MemoryStream(emailBytes), stoppingToken);

            await DeliverEmailAsync(email, fileName);

            _logger.LogInformation($"Successfully delivered email: {fileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to process email: {fileName}");
        }
    }

    private async Task DeliverEmailAsync(MimeMessage email, string fileName)
    {
        using var smtpClient = new SmtpClient();
        try
        {
            await smtpClient.ConnectAsync(_settings.Server, _settings.Port, _settings.EnableSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None);

            if (_settings.Username != "")
            {
                await smtpClient.AuthenticateAsync(_settings.Username, _settings.Password);
            }

            var result = await smtpClient.SendAsync(email);

            if (result.Contains(_settings.SuccessMessage))
            {
                await _httpClient.DeleteAsync($"/email/{fileName}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email.");
        }
        finally
        {
            await smtpClient.DisconnectAsync(true);
        }
    }
}
