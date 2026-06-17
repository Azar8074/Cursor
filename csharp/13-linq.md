# Module 13 — LINQ (Language Integrated Query)

**LINQ** provides a unified, declarative way to query collections, databases, XML, and more —
using the same syntax regardless of the data source.

## Two Syntaxes

```csharp
var numbers = new[] { 5, 2, 8, 1, 9, 3 };

// Query syntax (SQL-like)
var query = from n in numbers
            where n > 3
            orderby n
            select n;

// Method syntax (extension methods + lambdas) — more common
var method = numbers.Where(n => n > 3).OrderBy(n => n);
```

Both compile to the same thing. Method syntax is more powerful (some operators have no query keyword).

## Filtering & Projection

```csharp
var people = new[]
{
    new { Name = "Alice", Age = 30, City = "NYC" },
    new { Name = "Bob",   Age = 25, City = "LA" },
    new { Name = "Carol", Age = 35, City = "NYC" }
};

// Where — filter
var adults = people.Where(p => p.Age >= 30);

// Select — project/transform
var names = people.Select(p => p.Name);
var upper = people.Select(p => p.Name.ToUpper());

// SelectMany — flatten nested collections
var words = new[] { "hello world", "foo bar" };
var allWords = words.SelectMany(s => s.Split(' ')); // hello, world, foo, bar
```

## Ordering

```csharp
var sorted = people.OrderBy(p => p.Age);
var desc   = people.OrderByDescending(p => p.Age);
var multi  = people.OrderBy(p => p.City).ThenByDescending(p => p.Age);
var rev    = people.Reverse();
```

## Aggregation

```csharp
var nums = new[] { 1, 2, 3, 4, 5 };
int count   = nums.Count();
int sum     = nums.Sum();
double avg  = nums.Average();
int min     = nums.Min();
int max     = nums.Max();
int product = nums.Aggregate((acc, n) => acc * n); // 120

int adultsCount = people.Count(p => p.Age >= 30);
int oldest      = people.Max(p => p.Age);
```

## Element Operators

```csharp
var first   = nums.First();                 // throws if empty
var firstOr = nums.FirstOrDefault(n => n > 10); // 0 (default) if none
var single  = nums.Single(n => n == 3);     // exactly one, else throws
var last    = nums.Last();
var elem    = nums.ElementAt(2);
```

## Quantifiers & Set Operations

```csharp
bool anyAdult = people.Any(p => p.Age >= 30); // true
bool allAdult = people.All(p => p.Age >= 18); // true
bool has5     = nums.Contains(5);

var a = new[] { 1, 2, 3 };
var b = new[] { 2, 3, 4 };
var union     = a.Union(b);     // 1,2,3,4
var intersect = a.Intersect(b); // 2,3
var except    = a.Except(b);    // 1
var distinct  = new[] { 1, 1, 2 }.Distinct(); // 1,2
```

## Grouping

```csharp
var byCity = people.GroupBy(p => p.City);
foreach (var group in byCity)
{
    Console.WriteLine($"{group.Key}: {group.Count()} people");
    foreach (var p in group)
        Console.WriteLine($"  {p.Name}");
}

// GroupBy with projection
var counts = people
    .GroupBy(p => p.City)
    .Select(g => new { City = g.Key, Count = g.Count() });
```

## Joining

```csharp
var customers = new[] { new { Id = 1, Name = "Alice" }, new { Id = 2, Name = "Bob" } };
var orders    = new[] { new { CustomerId = 1, Product = "Book" },
                        new { CustomerId = 1, Product = "Pen" } };

var joined = customers.Join(
    orders,
    c => c.Id,             // outer key
    o => o.CustomerId,     // inner key
    (c, o) => new { c.Name, o.Product });
```

## Partitioning & Conversion

```csharp
var firstThree = nums.Take(3);
var skipTwo    = nums.Skip(2);
var page       = nums.Skip(10).Take(10);    // pagination
var chunked    = nums.Chunk(2);             // batches of 2 (.NET 6+)

List<int> list   = nums.ToList();
int[] array      = nums.ToArray();
var dict         = people.ToDictionary(p => p.Name, p => p.Age);
var lookup       = people.ToLookup(p => p.City);
```

## Deferred vs Immediate Execution

LINQ queries are **lazy** — they don't run until enumerated. Operators like `ToList()`, `Count()`,
`First()` force immediate execution.

```csharp
var q = numbers.Where(n => n > 3); // NOT executed yet
numbers[0] = 100;                  // change source
foreach (var n in q) { }           // executes NOW, sees the change

var snapshot = numbers.Where(n => n > 3).ToList(); // executes immediately
```

> **Pitfall:** re-enumerating a deferred query runs it again. Materialize with `ToList()` if you
> need a stable snapshot or will iterate multiple times.

## Chaining & Readability

```csharp
var result = people
    .Where(p => p.Age >= 25)
    .OrderBy(p => p.Name)
    .Select(p => new { p.Name, p.City })
    .ToList();
```

## Key Takeaways

- LINQ unifies querying across in-memory collections, databases (EF), XML, and more.
- Prefer **method syntax** with lambdas for full power; query syntax for complex joins/groupings.
- Most queries are **deferred** — call `ToList()`/`ToArray()` to materialize results.
- Master `Where`, `Select`, `OrderBy`, `GroupBy`, `Join`, and the aggregates — they cover 90% of needs.

➡️ Next: [Module 14 — Asynchronous Programming](14-async-await.md)
