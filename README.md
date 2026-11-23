# DrewSoft.Collections

A .NET collection library that provides a bucketed priority queue implementation.

## Features

- Prioritised buckets with independent ordering strategies
- FIFO and LIFO bucket ordering support
- Stable key-based and custom ordering via `Ordering<T>`
- Queue-style dequeue operations with `TryDequeue` and `Dequeue`

## Installation

```bash
dotnet add package DrewSoft.Collections
```

## Example

```csharp
using DrewSoft.Collections;

var queue = new BucketedPriorityQueue<int>
{
    new PriorityBucket<int>(Ordering<int>.Fifo) { 1, 2, 3 },
    new PriorityBucket<int>(Ordering<int>.Lifo) { 10, 20 }
};

while (queue.TryDequeue(out var item))
{
    Console.WriteLine(item);
}
```

## Custom ordering

Use `Ordering<T>.By` for stable key ordering. Items with equal keys retain their enqueue order:

```csharp
var bucket = new PriorityBucket<string>(
    Ordering<string>.By(value => value.Length))
{
    "three", "six", "nine"
};
```

For other policies, provide an ordering function:

```csharp
var queue = new BucketedPriorityQueue<string>
{
    new PriorityBucket<string>(new Ordering<string>(source => source.OrderBy(x => x.Length)))
    {
        "three", "six", "nine"
    }
};
```

## Collection behavior

- `Contains` and `Remove` use `EqualityComparer<T>.Default`.
- Queue-level `Remove` checks buckets in priority order and removes the first matching item.
- `Dequeue` removes the exact reference selected by the bucket ordering when possible.
- Enumerators are fail-fast: a queue or bucket mutation after `GetEnumerator()` causes the next `MoveNext()` to throw `InvalidOperationException`.

## License

MIT
