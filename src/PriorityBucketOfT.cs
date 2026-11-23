using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DrewSoft.Collections;

public class PriorityBucket<T>(Ordering<T>? bucketOrder = null) : ICollection<T>
    where T: notnull
{
    private readonly List<T> _items = [];
    private Ordering<T> _bucketOrder = bucketOrder ?? Ordering<T>.Fifo;

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public void Add(T item) => _items.Add(item);

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
            _items.Remove(item);
            return true;
        }

        item = default;
        return false;
    }

    public void Clear() => _items.Clear();

    public bool Contains(T item) => _items.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public bool Remove(T item) => _items.Remove(item);

    public IEnumerator<T> GetEnumerator() => _bucketOrder.Apply(_items).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
