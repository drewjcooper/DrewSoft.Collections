using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DrewSoft.Collections;

public class PriorityBucket<T>(Ordering<T>? bucketOrder = null) : ICollection<T>
    where T: notnull
{
    private readonly List<T> _items = [];
    private Ordering<T> _bucketOrder = bucketOrder ?? Ordering<T>.Fifo;
    private long _version;

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public Ordering<T> Ordering
    {
        get => _bucketOrder;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (ReferenceEquals(_bucketOrder, value)) { return; }

            _bucketOrder = value;
            _version++;
        }
    }

    internal long Version => _version;

    public void Add(T item)
    {
        _items.Add(item);
        _version++;
    }

    public T Peek()
        => TryPeek(out var item)
            ? item
            : throw new InvalidOperationException("Cannot peek at an item from an empty bucket.");

    public bool TryPeek([NotNullWhen(true)] out T? item)
    {
        if (_items.Count > 0)
        {
            item = _bucketOrder.Apply(_items).First();
            return true;
        }

        item = default;
        return false;
    }

    internal bool TryDequeue([NotNullWhen(true)] out T? item)
    {
        if (_items.Count > 0)
        {
            item = _bucketOrder.Apply(_items).First();
            RemoveDequeuedItem(item);
            return true;
        }

        item = default;
        return false;
    }

    public void Clear()
    {
        if (_items.Count == 0) { return; }

        _items.Clear();
        _version++;
    }

    public bool Contains(T item) => _items.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public bool Remove(T item)
    {
        if (!_items.Remove(item)) { return false; }

        _version++;
        return true;
    }

    public IEnumerator<T> GetEnumerator() => GetEnumerator(_version);

    internal IEnumerator<T> GetEnumerator(long expectedVersion)
        => Enumerate(expectedVersion, _bucketOrder).GetEnumerator();

    private IEnumerable<T> Enumerate(long expectedVersion, Ordering<T> ordering)
    {
        EnsureVersion(expectedVersion);
        foreach (var item in ordering.Apply(_items))
        {
            EnsureVersion(expectedVersion);
            yield return item;
        }
        EnsureVersion(expectedVersion);
    }

    private void EnsureVersion(long expectedVersion)
    {
        if (_version != expectedVersion)
        {
            throw new InvalidOperationException("The bucket was modified during enumeration.");
        }
    }

    private void RemoveDequeuedItem(T item)
    {
        var index = _items.FindIndex(candidate => ReferenceEquals(candidate, item));
        if (index < 0)
        {
            index = _items.FindIndex(candidate => EqualityComparer<T>.Default.Equals(candidate, item));
        }

        if (index < 0) { return; }

        _items.RemoveAt(index);
        _version++;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
