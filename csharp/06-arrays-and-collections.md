# Module 06 — Arrays & Collections

## Arrays

Fixed-size, same-type sequences. Indices are **zero-based**.

```csharp
int[] numbers = new int[5];          // {0,0,0,0,0}
int[] primes = { 2, 3, 5, 7, 11 };   // initialized
string[] names = new[] { "A", "B" }; // type inferred

Console.WriteLine(primes[0]);   // 2
Console.WriteLine(primes.Length); // 5
primes[0] = 1;                  // mutate by index
```

### Multidimensional & Jagged Arrays

```csharp
int[,] matrix = new int[2, 3];        // rectangular 2x3
matrix[0, 1] = 9;

int[][] jagged = new int[2][];        // array of arrays (rows can differ in length)
jagged[0] = new[] { 1, 2 };
jagged[1] = new[] { 3, 4, 5 };
```

### Iterating

```csharp
foreach (int p in primes) Console.WriteLine(p);
Array.Sort(primes);
Array.Reverse(primes);
int idx = Array.IndexOf(primes, 5);
```

## The Generic Collections (`System.Collections.Generic`)

Prefer these over arrays when the size changes, and over legacy non-generic collections
(`ArrayList`, `Hashtable`) which box value types.

### List&lt;T&gt; — resizable array

```csharp
var list = new List<string> { "apple", "banana" };
list.Add("cherry");
list.Insert(0, "apricot");
list.Remove("banana");
list.RemoveAt(0);
bool has = list.Contains("cherry");
int count = list.Count;
list.Sort();
list.ForEach(Console.WriteLine);
```

### Dictionary&lt;TKey, TValue&gt; — key/value lookup (hash table)

```csharp
var ages = new Dictionary<string, int>
{
    ["Alice"] = 30,
    ["Bob"]   = 25
};

ages["Carol"] = 28;                 // add or update
if (ages.TryGetValue("Alice", out int a))
    Console.WriteLine(a);           // 30 — safe lookup
bool exists = ages.ContainsKey("Bob");
ages.Remove("Bob");

foreach (var kvp in ages)
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
```

> Dictionary lookups are O(1) on average. Accessing a missing key with `[]` throws — use
> `TryGetValue` or `ContainsKey`.

### HashSet&lt;T&gt; — unique elements, fast membership

```csharp
var set = new HashSet<int> { 1, 2, 3 };
set.Add(2);            // ignored — already present
bool added = set.Add(4); // true
set.UnionWith(new[] { 5, 6 });
set.IntersectWith(new[] { 1, 5 });
```

### Queue&lt;T&gt; — FIFO (first in, first out)

```csharp
var queue = new Queue<string>();
queue.Enqueue("first");
queue.Enqueue("second");
string next = queue.Dequeue(); // "first"
string peek = queue.Peek();    // "second" (without removing)
```

### Stack&lt;T&gt; — LIFO (last in, first out)

```csharp
var stack = new Stack<int>();
stack.Push(1);
stack.Push(2);
int top = stack.Pop();   // 2
int look = stack.Peek(); // 1
```

### LinkedList&lt;T&gt;, SortedList, SortedDictionary

- `LinkedList<T>` — doubly linked list; fast inserts/removes in the middle.
- `SortedDictionary<K,V>` / `SortedList<K,V>` — keep keys sorted automatically.

## Choosing the Right Collection

| Need | Use |
|------|-----|
| Indexed, growable list | `List<T>` |
| Key → value lookup | `Dictionary<K,V>` |
| Unique items / set math | `HashSet<T>` |
| Process in arrival order | `Queue<T>` |
| Process most-recent first | `Stack<T>` |
| Always-sorted keys | `SortedDictionary<K,V>` |
| Thread-safe concurrent access | `ConcurrentDictionary<K,V>`, `ConcurrentQueue<T>` |
| Read-only / immutable | `IReadOnlyList<T>`, `ImmutableList<T>` |

## Collection Initializers & Spread

```csharp
var nums = new List<int> { 1, 2, 3 };

// Collection expressions (C# 12)
int[] combined = [1, 2, 3, ..nums, 4]; // spread operator
```

## Big-O Cheat Sheet

| Operation | `List<T>` | `Dictionary` | `HashSet` |
|-----------|-----------|--------------|-----------|
| Access by index | O(1) | — | — |
| Search by value | O(n) | O(1) avg | O(1) avg |
| Add (end) | O(1) amortized | O(1) avg | O(1) avg |
| Insert/Remove (middle) | O(n) | O(1) avg | O(1) avg |

## Key Takeaways

- Use **generic** collections; avoid boxing from legacy non-generic ones.
- `List<T>` for ordered data, `Dictionary` for lookups, `HashSet` for uniqueness.
- Use `TryGetValue` to avoid exceptions and double lookups.
- Pick the collection that matches your access pattern — it dictates performance.

➡️ Next: [Module 07 — OOP: Classes & Objects](07-oop-classes-and-objects.md)
