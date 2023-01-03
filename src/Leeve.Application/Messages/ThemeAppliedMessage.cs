namespace Leeve.Application.Messages;

public sealed class ThemeAppliedMessage : ValueChangedMessage<bool>
{
    public ThemeAppliedMessage(bool value) : base(value)
    {
    }
}