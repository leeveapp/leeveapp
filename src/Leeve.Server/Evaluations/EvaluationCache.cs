using Leeve.Core.Common;

namespace Leeve.Server.Evaluations;

public class EvaluationCache
{
    static EvaluationCache()
    {
    }

    private EvaluationCache()
    {
        _currentEvaluations = new Dictionary<string, string>();
    }

    private readonly Dictionary<string, string> _currentEvaluations;

    private static EvaluationCache Instance { get; } = new();

    public static bool IsActive(string id, out string code) => Instance.IsActiveInternal(id, out code);

    private bool IsActiveInternal(string id, out string code)
    {
        var added = _currentEvaluations.TryGetValue(id, out var result);

        code = result ?? string.Empty;
        return added;
    }

    public static string Add(string id) => Instance.AddInternal(id);

    private int _latestCount;

    private string AddInternal(string id)
    {
        var added = _currentEvaluations.TryGetValue(id, out var result);
        if (added) return result!;

        var newCode = GenerateCode();
        _currentEvaluations.TryAdd(id, newCode);

        return newCode;
    }

    private string GenerateCode()
    {
        var str = $"{Guid.NewGuid().ToString("N").Substring(0, 5)}{_latestCount++}";
        return str.ToUpper();
    }

    public static void Remove(string id) => Instance._currentEvaluations.Remove(id);

    public static Result<string> Get(string requestCode)
    {
        var key = Instance._currentEvaluations.FirstOrDefault(x => x.Value == requestCode).Key;
        return string.IsNullOrEmpty(key)
            ? new Result<string>(new ArgumentException("Invalid request code"))
            : new Result<string>(key);
    }
}