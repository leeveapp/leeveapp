using Leeve.Application.Users;

namespace Leeve.Application.Messages;

public sealed class AddNewPageMessage : ValueChangedMessage<PageItem>
{
    public AddNewPageMessage(PageItem value) : base(value)
    {
    }
}