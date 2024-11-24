namespace SmtpStoreApi.Utils;

public class ApiTokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Settings _settings;

    public ApiTokenAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _settings = configuration.GetSection("Settings").Get<Settings>()!;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var excludedPaths = new[] { "/" };

        if (excludedPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authorization header missing.");
            return;
        }

        var token = authorizationHeader.ToString().Replace("Bearer ", "");
        if (token != _settings.ApiToken)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid API token.");
            return;
        }

        await _next(context);
    }
}
