namespace Leeve.Application.Common;

public interface IInitializable
{
    Task<bool> InitializeAsync();
}