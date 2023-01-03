namespace Leeve.Core.Common;

public interface IThreadWrapper
{
    void Invoke(Action action);
    Task InvokeAsync(Func<Task> action);
}