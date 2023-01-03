namespace Leeve.Client.Common;

public abstract class NotificationService<TResponse>
{
    private CancellationTokenSource _tokenSource;

    protected NotificationService()
    {
        _tokenSource = new CancellationTokenSource();
    }

    private CancellationToken InitializeToken()
    {
        _tokenSource.Cancel();
        _tokenSource = new CancellationTokenSource();
        return _tokenSource.Token;
    }

    private IAsyncEnumerable<TResponse> GetRequests(CancellationToken token)
    {
        var call = GetServerCall(token);
        var moveNext = () => call.ResponseStream.MoveNext(token);
        return moveNext.ToAsyncEnumerable(() => call.ResponseStream.Current).Finally(() => call.Dispose());
    }

    public async Task Subscribe()
    {
        var token = InitializeToken();
        await GetRequests(token).ForEachAsync(Respond, token).ConfigureAwait(false);
    }

    protected abstract AsyncServerStreamingCall<TResponse> GetServerCall(CancellationToken token);
    protected abstract void Respond(TResponse response);
}