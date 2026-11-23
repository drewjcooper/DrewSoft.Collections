using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DrewSoft.Collections;

public class BucketedPriorityQueue<T> : IEnumerable<T>
    where T : notnull
{
    private readonly List<PriorityBucket<T>> _buckets;
    private long _version;

    public BucketedPriorityQueue() => _buckets = [];

    public BucketedPriorityQueue(IEnumerable<PriorityBucket<T>> buckets) => _buckets = [..buckets];

    public IEnumerable<PriorityBucket<T>> Buckets => _buckets.AsReadOnly();
    public int Count => _buckets.Sum(bucket => bucket.Count);
    public bool IsEmpty => Count == 0;

    public void Add(PriorityBucket<T> bucket)
    {
        _buckets.Add(bucket);
        _version++;
    }

    public void Insert(int index, PriorityBucket<T> bucket)
    {
        if (index < _buckets.Count) 
        {
            _buckets.Insert(index, bucket);
        }
        else
        {
            _buckets.Add(bucket);
        }

        _version++;
    }

    public bool RemoveBucket(PriorityBucket<T> bucket)
    {
        if (!_buckets.Remove(bucket)) { return false; }

        _version++;
        return true;
    }

    public bool MoveBucket(PriorityBucket<T> bucket, int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        if (index >= _buckets.Count) { throw new ArgumentOutOfRangeException(nameof(index)); }

        var currentIndex = _buckets.IndexOf(bucket);
        if (currentIndex < 0) { return false; }
        if (currentIndex == index) { return true; }

        _buckets.RemoveAt(currentIndex);
        _buckets.Insert(index, bucket);
        _version++;
        return true;
    }

    public bool Remove(T item)
    {
        foreach (var bucket in _buckets)
        {
            if (bucket.Remove(item)) { return true; }
        }

        return false;
    }

    public void Clear()
    {
        foreach (var bucket in _buckets)
        {
            bucket.Clear();
        }
    }

    public T Peek()
        => TryPeek(out var item)
            ? item
            : throw new InvalidOperationException("Cannot peek at an item from an empty queue.");

    public bool TryPeek([NotNullWhen(true)] out T? item)
    {
        foreach (var bucket in _buckets)
        {
            if (bucket.TryPeek(out item)) { return true; }
        }

        item = default;
        return false;
    }

    public T Dequeue()
        => TryDequeue(out var item)
            ? item
            : throw new InvalidOperationException("Cannot dequeue an item from an empty queue.");

    public bool TryDequeue([NotNullWhen(true)] out T? item)
    {
        foreach (var bucket in _buckets)
        {
            if (bucket.TryDequeue(out item)) { return true; }
        }

        item = default;
        return false;
    }

    public IEnumerator<T> GetEnumerator()
    {
        var expectedVersion = _version;
        var buckets = _buckets.Select(bucket => (Bucket: bucket, Version: bucket.Version)).ToArray();
        return Enumerate(expectedVersion, buckets).GetEnumerator();
    }

    private IEnumerable<T> Enumerate(long expectedVersion, (PriorityBucket<T> Bucket, long Version)[] buckets)
    {
        EnsureVersion(expectedVersion);
        foreach (var (bucket, bucketVersion) in buckets)
        {
            EnsureVersion(expectedVersion);
            using var enumerator = bucket.GetEnumerator(bucketVersion);
            while (enumerator.MoveNext())
            {
                EnsureVersion(expectedVersion);
                yield return enumerator.Current;
            }
        }
        EnsureVersion(expectedVersion);
    }

    private void EnsureVersion(long expectedVersion)
    {
        if (_version != expectedVersion)
        {
            throw new InvalidOperationException("The queue was modified during enumeration.");
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
