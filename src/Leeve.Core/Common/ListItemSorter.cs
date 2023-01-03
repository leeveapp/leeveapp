namespace Leeve.Core.Common;

public sealed class ListItemSorter<T>
{
    private readonly T _item;
    private readonly IList<T> _collection;
    private IOrderedEnumerable<T>? _orderedEnumerable;

    /// <summary>
    ///     Helps to position an element in a collection without affecting other elements.
    /// </summary>
    /// <param name="collection">The current collection where the <see cref="item" /> is to be position.</param>
    /// <param name="item">The element to be position based on the ordering set up.</param>
    public ListItemSorter(IList<T> collection, T item)
    {
        _item = item;
        _collection = collection;

        if (!collection.Contains(item))
            _collection.Add(item);
    }

    /// <summary>
    ///     Set the ordering of the elements in the current collection in ascending order according to a key.
    /// </summary>
    /// <param name="keySelector">The type of the elements of source.</param>
    /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
    /// <returns></returns>
    public ListItemSorter<T> OrderBy<TKey>(Func<T, TKey> keySelector)
    {
        _orderedEnumerable = _collection.OrderBy(keySelector);
        return this;
    }

    /// <summary>
    ///     Set the ordering of the elements in the current collection in descending order according to a key.
    /// </summary>
    /// <param name="keySelector">The type of the elements of source.</param>
    /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
    /// <returns></returns>
    public ListItemSorter<T> OrderByDescending<TKey>(Func<T, TKey> keySelector)
    {
        _orderedEnumerable = _collection.OrderByDescending(keySelector);
        return this;
    }

    /// <summary>
    ///     Set the subsequent ordering of the elements in the current collection in ascending order according to a key.
    /// </summary>
    /// <param name="keySelector">The type of the elements of source.</param>
    /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
    /// <returns></returns>
    public ListItemSorter<T> ThenBy<TKey>(Func<T, TKey> keySelector)
    {
        _orderedEnumerable = _orderedEnumerable?.ThenBy(keySelector);
        return this;
    }

    /// <summary>
    ///     Position the current item in the collection according to the current ordering.
    /// </summary>
    public async Task SortAsync()
    {
        if (_orderedEnumerable == null) return;

        var index = await Task.Run(() => IndexOf(_orderedEnumerable, _item));

        var currentIndex = _collection.IndexOf(_item);
        if (currentIndex == index) return;

        _collection.Remove(_item);
        _collection.Insert(index, _item);
    }

    private int IndexOf(IEnumerable<T> collection, T value)
    {
        var comparer = EqualityComparer<T>.Default;
        var found = collection.Select((item, index) => new { item, index }).FirstOrDefault(x => comparer.Equals(x.item, value));
        return found?.index ?? -1;
    }
}