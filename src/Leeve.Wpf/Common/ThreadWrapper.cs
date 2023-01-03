using System;
using System.Threading.Tasks;
using Leeve.Core.Common;

namespace Leeve.Wpf.Common;

public sealed class ThreadWrapper : IThreadWrapper
{
    public void Invoke(Action action)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(action);
    }

    public async Task InvokeAsync(Func<Task> action)
    {
        await System.Windows.Application.Current.Dispatcher.InvokeAsync(action);
    }
}