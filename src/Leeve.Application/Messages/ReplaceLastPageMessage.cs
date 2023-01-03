using Leeve.Application.Users;

namespace Leeve.Application.Messages;

public sealed class ReplaceLastPageMessage : ValueChangedMessage<PageItem>
{
    public ReplaceLastPageMessage(PageItem value) : base(value)
    {
    }
}