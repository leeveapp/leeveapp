using System.Reactive.Linq;

namespace Leeve.Server.Common;

public abstract class NotifierBase<T>
{
    private event Action<T>? Changed;

    public void Change(T message)
    {
        Changed?.Invoke(message);
    }

    public IObservable<T> GetAsObservable()
    {
        return Observable.FromEvent<T>(x => Changed += x, x => Changed -= x);
    }
}