using System.Reflection;
using Leeve.Server.Users;
using MongoDB.Bson.Serialization.Conventions;

namespace Leeve.Server.Common;

internal sealed class StartupService : IHostedService
{
    private readonly ILogger _log;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly Credentials? _credentials;

    public StartupService(ILogger log, IHostApplicationLifetime appLifetime)
    {
        _log = log;
        _appLifetime = appLifetime;
        _credentials = Credentials.Get();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(OnStarted);
        _appLifetime.ApplicationStopping.Register(OnStopping);
        _appLifetime.ApplicationStopped.Register(OnStopped);

        InitializeDb();
        return SeedAdminUserAsync();
    }

    private void InitializeDb()
    {
        var connection = "mongodb://localhost:20050";

        var ip = Environment.GetEnvironmentVariable("LEEVE_SERVICE_IP");
        var port = Environment.GetEnvironmentVariable("LEEVE_SERVICE_PORT");
        if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(port))
            connection = $"mongodb://{ip}:{port}";

        var database = "leeve-db";

        if (_credentials != null)
        {
            connection = $"mongodb://{_credentials.User}:{_credentials.Password}" +
                         $"@localhost:{_credentials.Port}/?authSource={_credentials.Auth}";
            database = _credentials.Name;
        }

        DbFactory.DefaultConnection = connection;
        DbFactory.DefaultDatabase = database;
        DbFactory.SetGlobalFilter<IDeleteOn>("{ deleted: false }", Assembly.GetAssembly(typeof(BaseDocument))!);
        DbFactory.AddConvention("IgnoreExtraElements", new ConventionPack { new IgnoreExtraElementsConvention(true) });

        _log.Information("Database has been initialized");
    }

    private Task SeedAdminUserAsync()
    {
        var context = DbFactory.Get();
        var adminRepo = new AdminRepository(context, _log);
        return adminRepo.SeedAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void OnStarted()
    {
        _log.Information("Leeve service started");
    }

    private void OnStopping()
    {
        _log.Information("Leeve service stopping");
    }

    private void OnStopped()
    {
        _log.Information("Leeve service stopped");
    }
}