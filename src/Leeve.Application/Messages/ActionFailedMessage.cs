namespace Leeve.Application.Messages;

public sealed class ActionFailedMessage : ValueChangedMessage<string>
{
    public ActionFailedMessage(string value) : base(value)
    {
    }
}