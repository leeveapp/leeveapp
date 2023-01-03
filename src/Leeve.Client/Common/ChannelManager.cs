namespace Leeve.Client.Common;

public sealed class ChannelManager
{
    public GrpcChannel? Channel { get; private set; }

    public static DateTime DeadLine => DateTime.UtcNow.AddSeconds(10);

    public void SetChannel(string ipAddress, int connPort)
    {
        Channel = GrpcChannel.ForAddress($"http://{ipAddress}:{connPort}");
    }
}