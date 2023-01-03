namespace Leeve.Application.Common;

public interface IInitializableForEdit
{
    Task<bool> InitializeAsync(object entity);
}