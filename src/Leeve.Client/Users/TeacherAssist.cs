using Leeve.Core.Users;

namespace Leeve.Client.Users;

public sealed class TeacherAssist
{
    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static TeacherAssist()
    {
    }

    private readonly string _callerId;

    private TeacherAssist()
    {
        _callerId = Guid.NewGuid().ToString();
    }

    private static TeacherAssist Instance { get; } = new();

    public static string CallerId => Instance._callerId;

    public static string Id => Instance._teacher?.Id ?? string.Empty;

    private static string _fullName = string.Empty;

    public static string FullName
    {
        get => _fullName;
        private set
        {
            _fullName = value;
            NotifyStaticPropertyChanged();
        }
    }

    private static string _department = string.Empty;

    public static string Department
    {
        get => _department;
        private set
        {
            _department = value;
            NotifyStaticPropertyChanged();
        }
    }

    private static byte[]? _image;

    public static byte[]? Image
    {
        get => _image;
        private set
        {
            _image = value;
            NotifyStaticPropertyChanged();
        }
    }

    private Teacher? _teacher;

    public static Teacher? Teacher
    {
        get => Instance._teacher;
        set
        {
            Instance._teacher = value;
            FullName = Instance._teacher?.FullName ?? string.Empty;
            Department = Instance._teacher?.Department ?? string.Empty;
            Image = Instance._teacher?.Image;
        }
    }

    public static void UpdateImage(byte[] image)
    {
        if (Instance._teacher != null)
            Instance._teacher.Image = image;

        Image = image;
    }

    // this is required to notify the UI that a property has changed
    public static event PropertyChangedEventHandler? StaticPropertyChanged;

    private static void NotifyStaticPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }
}