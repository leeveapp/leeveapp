namespace Leeve.Application.Common;

public static class LoadingWrapper
{
    public static async Task<T> LoadAsync<T>(this IDialog dialog, string identifier, Func<Task<T>> task)
    {
        var loading = await dialog.ShowLoadingAsync(identifier);
        try
        {
            return await task.Invoke();
        }
        finally
        {
            await dialog.CloseLoadingAsync(loading);
        }
    }

    public static async Task LoadAsync(this IDialog dialog, string identifier, Func<Task> task)
    {
        var loading = await dialog.ShowLoadingAsync(identifier);
        try
        {
            await task.Invoke();
        }
        finally
        {
            await dialog.CloseLoadingAsync(loading);
        }
    }
}