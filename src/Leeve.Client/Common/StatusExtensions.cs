namespace Leeve.Client.Common;

public static partial class StatusExtensions
{
    public static string ExtractMessage(this Status status)
    {
        var message = status.Detail;
        var match = DetailMessageRegex().Match(message);

        return match.Success ? match.Groups[1].Value : message;
    }

    [GeneratedRegex("Detail=\"([^\"]+)\"")]
    private static partial Regex DetailMessageRegex();
}