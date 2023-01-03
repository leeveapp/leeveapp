namespace Leeve.Client.Users;

public class AdminAssist
{
    static AdminAssist()
    {
    }

    private AdminAssist()
    {
        _callerId = Guid.NewGuid().ToString();
    }

    private readonly string _callerId;
    public static string CallerId => Instance._callerId;

    private static AdminAssist Instance { get; } = new();

    private string _adminId = string.Empty;

    public static string AdminId
    {
        get => Instance._adminId;
        set => Instance._adminId = value;
    }
}