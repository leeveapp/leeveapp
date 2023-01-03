namespace Leeve.Application.Messages;

public sealed class NotificationMessage : ValueChangedMessage<string>
{
    public NotificationMessage(string value) : base(value)
    {
    }
}