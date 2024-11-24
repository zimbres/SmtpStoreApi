namespace SmtpStoreApi.Services;

public class MessageStoreService : MessageStore
{
    private readonly Settings _settings;
    public MessageStoreService(IConfiguration configuration)
    {
        _settings = configuration.GetSection("Settings").Get<Settings>()!;
    }
    public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();

        var position = buffer.GetPosition(0);
        while (buffer.TryGet(ref position, out var memory))
        {
            await stream.WriteAsync(memory, cancellationToken);
        }

        stream.Position = 0;

        var message = await MimeMessage.LoadAsync(stream, cancellationToken);

        var emailPath = Path.Combine(_settings.StorePath, $"{Guid.NewGuid()}.eml");

        Directory.CreateDirectory(Path.GetDirectoryName(emailPath) ?? string.Empty);

        await using var fileStream = new FileStream(emailPath, FileMode.Create, FileAccess.Write);
        await message.WriteToAsync(fileStream, cancellationToken);

        return SmtpResponse.Ok;
    }
}
