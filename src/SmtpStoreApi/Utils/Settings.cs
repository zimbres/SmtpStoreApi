namespace SmtpStoreApi.Utils;

public class Settings
{
    public required string ApiToken { get; set; }
    public bool ApiAuthEnabled { get; set; }
    public required string ServerName { get; set; }
    public int Port { get; set; }
    public required string StorePath { get; set; }
}