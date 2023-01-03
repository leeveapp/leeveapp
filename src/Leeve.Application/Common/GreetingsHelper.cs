namespace Leeve.Application.Common;

public interface IGreetingsHelper : IDisposable
{
    Task StartAsync();
    event EventHandler<string>? GreetingsChanged;
}

public sealed class GreetingsHelper : IGreetingsHelper
{
    private string _greetings = string.Empty;
    private readonly PeriodicTimer _timer;
    private bool _started;

    public GreetingsHelper()
    {
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
    }

    public async Task StartAsync()
    {
        if (_started) return;

        _started = true;
        while (await _timer.WaitForNextTickAsync())
        {
            var greetings = GenerateGreetings();
            if (string.Equals(greetings, _greetings)) continue;

            GreetingsChanged?.Invoke(this, greetings);
            _greetings = greetings;
        }
    }

    private static string GenerateGreetings()
    {
        var now = DateTime.Now;
        var hour = now.Hour;
        var minute = now.Minute;
        if (hour < 12)
            return "Good morning";
        if (hour == 12 && minute == 0)
            return "Good noon";
        return hour < 18
            ? "Good afternoon"
            : "Good evening";
    }

    public event EventHandler<string>? GreetingsChanged;

    public void Dispose()
    {
        _timer.Dispose();
    }
}