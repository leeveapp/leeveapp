using Leeve.Core.Common;

namespace Leeve.Client.Common;

public interface IClientService
{
    Task<bool> StartAsync();
    Task<bool> SaveConnectionAsync(Connection conn);
    Task<Connection> GetConnectionAsync();
}

public sealed class ClientServiceBase : IClientService
{
    private readonly ChannelManager _manager;
    private readonly string _configFile;
    private readonly List<Action> _services = new();

    public ClientServiceBase(ChannelManager manager, string configFile)
    {
        _manager = manager;
        _configFile = configFile;
    }

    public async Task<bool> StartAsync()
    {
        try
        {
            var conn = await GetConnectionAsync().ConfigureAwait(false);
            var ipAddress = await Task.Run(() => GetServerIp(conn.ServerName)).ConfigureAwait(false);

            _manager.SetChannel(ipAddress, conn.Port);
            foreach (var service in _services) service.Invoke();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<Connection> GetConnectionAsync()
    {
        var defaultConn = new Connection { ServerName = Dns.GetHostName(), Port = 2701 };

        try
        {
            if (!File.Exists(_configFile)) return defaultConn;

            var json = await File.ReadAllTextAsync(_configFile).ConfigureAwait(false);
            var conn = JsonSerializer.Deserialize<Connection>(json);

            if (conn == null) return defaultConn;

            if (string.IsNullOrEmpty(conn.ServerName)) conn.ServerName = defaultConn.ServerName;
            if (conn.Port == 0) conn.Port = defaultConn.Port;
            return conn;
        }
        catch (Exception)
        {
            return defaultConn;
        }
    }

    private string GetServerIp(string server)
    {
        try
        {
            var ipaddress = Dns.GetHostAddresses(server);
            foreach (var ip in ipaddress)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();

            return "127.0.0.1";
        }
        catch (SocketException)
        {
            return "127.0.0.1";
        }
    }

    public Task<bool> SaveConnectionAsync(Connection conn) => conn.SaveAsync(_configFile);

    /// <summary>
    ///     Register a service that run at start up
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public ClientServiceBase Register(Action action)
    {
        _services.Add(action);
        return this;
    }
}