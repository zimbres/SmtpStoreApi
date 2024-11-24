namespace SmtpStoreApi.Services;

public class SmtpService : BackgroundService
{
    private readonly Settings _settings;
    private readonly SmtpServer.SmtpServer _smtpServer;
    private readonly MessageStoreService _messageStoreService;

    public SmtpService(IConfiguration configuration, MessageStoreService messageStoreService)
    {
        _settings = configuration.GetSection("Settings").Get<Settings>()!;
        var options = new SmtpServerOptionsBuilder().ServerName(_settings.ServerName).Port(_settings.Port).Build();

        _messageStoreService = messageStoreService;
        var serviceProvider = new SmtpServer.ComponentModel.ServiceProvider();
        serviceProvider.Add(_messageStoreService);

        _smtpServer = new SmtpServer.SmtpServer(options, serviceProvider);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _smtpServer.StartAsync(stoppingToken);
    }
}