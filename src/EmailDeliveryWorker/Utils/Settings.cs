namespace EmailDeliveryWorker.Utils;

public class Settings
{
    public required string ApiBaseAddress { get; set; }
    public required string ApiToken { get; set; }
    public required string Server { get; set; }
    public int Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public bool EnableSsl { get; set; }
    public required string SuccessMessage { get; set; }
    public int Delay { get; set; }
}