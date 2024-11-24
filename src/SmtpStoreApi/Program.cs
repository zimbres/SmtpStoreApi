var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var settings = builder.Configuration.GetSection("Settings").Get<Settings>()!;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHostedService<SmtpService>();
builder.Services.AddSingleton<MessageStoreService>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (settings.ApiAuthEnabled)
{
    app.UseMiddleware<ApiTokenAuthenticationMiddleware>();
}

app.MapGet("/", () =>
{
    return Results.Ok();
});

var deliveryEnabled = true;

app.MapPost("/delivery", (bool status) =>
{
    deliveryEnabled = status;
});

app.MapGet("/emails", () =>
{
    var emailDirectory = Path.Combine(Directory.GetCurrentDirectory(), settings.StorePath);

    var emails = Directory.GetFiles(emailDirectory, "*.eml").Select(Path.GetFileName).ToArray();

    return Results.Ok(new
    {
        deliveryEnabled,
        emails
    });
});

app.MapGet("/email/{fileName}", (string fileName) =>
{
    var emailPath = Path.Combine(Directory.GetCurrentDirectory(), settings.StorePath, fileName);
    if (!File.Exists(emailPath))
    {
        return Results.NotFound("Email file not found.");
    }

    var fileBytes = File.ReadAllBytes(emailPath);
    return Results.File(fileBytes, "message/rfc822", fileName);
});

app.MapDelete("/email/{fileName}", (string fileName) =>
{
    var emailPath = Path.Combine(Directory.GetCurrentDirectory(), settings.StorePath, fileName);
    if (!File.Exists(emailPath))
    {
        return Results.NotFound("Email file not found.");
    }

    try
    {
        File.Delete(emailPath);
        return Results.Ok($"File {fileName} deleted successfully.");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error deleting file {fileName}: {ex.Message}");
    }
});

app.Run();
