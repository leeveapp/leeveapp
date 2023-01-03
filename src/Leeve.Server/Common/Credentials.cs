using System.Text.Json;

namespace Leeve.Server.Common;

internal sealed class Credentials
{
    public required int Port { get; init; }
    public required int Server { get; init; }
    public required string Name { get; init; }
    public required string User { get; init; }
    public required string Password { get; init; }
    public required string Auth { get; init; }

    public static Credentials? Get()
    {
        var credentials = Environment.GetEnvironmentVariable("LEEVE_SERVICE");
        if (string.IsNullOrEmpty(credentials)) return null;

        credentials = "{" + credentials + "}";
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<Credentials>(credentials, options);
    }
}