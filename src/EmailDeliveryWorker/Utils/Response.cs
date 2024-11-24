namespace EmailDeliveryWorker.Utils;

internal class Response
{
    public bool DeliveryEnabled { get; set; }
    public List<string> Emails { get; set; } = [];
}
