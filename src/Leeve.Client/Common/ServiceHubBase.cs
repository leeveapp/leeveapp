namespace Leeve.Client.Common;

public class ServiceHubBase<T> where T : new()
{
    #region singleton

    static ServiceHubBase()
    {
    }

    protected ServiceHubBase()
    {
    }

    private static readonly ServiceHubBase<T> Instance = new();

    #endregion

    private readonly ConcurrentDictionary<string, T> _collection = new();

    private bool _lookedUp;

    public static int Count
    {
        get
        {
            Instance._lookedUp = true;
            return Instance._collection.Count;
        }
    }

    public static IEnumerable<T> GetAll() => Instance._collection.Values;

    public static bool TryGet(string key, out T? result) => Instance._collection.TryGetValue(key, out result);

    public static IEnumerable<T> Search(Func<KeyValuePair<string, T>, bool> predicate)
    {
        return Instance._collection.Where(predicate).Select(i => i.Value);
    }

    public static void AddOrUpdate(string key, T value)
    {
        if (Instance._lookedUp) //to add or update, the collection must be looked up first
            Instance._collection.AddOrUpdate(key, _ => value, (_, _) => value);
    }

    public static void Remove(string key)
    {
        Instance._collection.TryRemove(key, out _);
    }

    public static void Clear()
    {
        Instance._lookedUp = false;
        Instance._collection.Clear();
    }
}