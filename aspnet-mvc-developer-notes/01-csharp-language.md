# 01 — C# Language (Fundamentals → Advanced)

C# is the foundation. Strong language skills make every framework easier. This
file covers what an ASP.NET MVC developer should master.

---

## 1. Type system

### Value types vs reference types
- **Value types** (`int`, `bool`, `double`, `struct`, `enum`): hold the data
  directly; copied on assignment; live on the **stack** (or inline in a
  containing object).
- **Reference types** (`class`, `string`, `object`, arrays, delegates): hold a
  reference to data on the **heap**; copying copies the reference, not the data.

```csharp
struct PointS { public int X; }      // value type
class PointC { public int X; }       // reference type

var a = new PointS { X = 1 };
var b = a;        // copy
b.X = 99;         // a.X is still 1

var c = new PointC { X = 1 };
var d = c;        // reference copy
d.X = 99;         // c.X is now 99 (same object)
```

### Boxing / unboxing
Converting a value type to `object` (boxing) allocates on the heap; converting
back (unboxing) costs a cast + copy. Avoid in hot paths; prefer generics.

```csharp
int i = 42;
object o = i;        // boxing (heap allocation)
int j = (int)o;      // unboxing
```

### Nullable
- `Nullable<T>` / `int?` for value types that may be absent.
- **Nullable reference types** (C# 8+): `string?` vs `string`, compiler warns on
  possible nulls. Enable with `<Nullable>enable</Nullable>`.

```csharp
int? maybe = null;
int value = maybe ?? -1;          // null-coalescing
string? name = GetName();
int len = name?.Length ?? 0;      // null-conditional
```

---

## 2. Strings
- Strings are **immutable**. Concatenating in a loop creates garbage — use
  `StringBuilder`.
- Use `string.IsNullOrEmpty` / `string.IsNullOrWhiteSpace`.
- Interpolation: `$"Hello {name}, you are {age} years old"`.
- Be aware of culture: `ToString()`, `Parse`, comparisons can be
  culture-sensitive. Use `StringComparison.Ordinal` for non-linguistic compares.

```csharp
var sb = new StringBuilder();
for (int i = 0; i < 1000; i++) sb.Append(i);
string result = sb.ToString();
```

---

## 3. Collections & LINQ

### Key interfaces
- `IEnumerable<T>` — forward iteration, **deferred** execution.
- `ICollection<T>` — adds `Count`, `Add`, `Remove`.
- `IList<T>` — indexed access.
- `IDictionary<TKey,TValue>` — key lookups.
- `IQueryable<T>` — builds an **expression tree** translated to SQL by EF.

### IEnumerable vs IQueryable (critical for EF)
```csharp
// IQueryable: filtering happens in SQL (good)
IQueryable<User> q = db.Users.Where(u => u.Age > 18);

// IEnumerable: ToList() pulls EVERYTHING, then filters in memory (bad)
IEnumerable<User> e = db.Users.ToList().Where(u => u.Age > 18);
```

### Deferred execution
LINQ queries don't run until enumerated (`foreach`, `ToList`, `Count`, etc.).
Re-enumerating runs the query again. Materialize with `ToList()` when needed.

### Common LINQ operators
`Where`, `Select`, `SelectMany`, `OrderBy/ThenBy`, `GroupBy`, `Join`,
`First/FirstOrDefault`, `Single/SingleOrDefault`, `Any/All`, `Sum/Count/Max`,
`Distinct`, `Skip/Take` (paging), `Aggregate`.

```csharp
var report = orders
    .Where(o => o.Total > 100)
    .GroupBy(o => o.CustomerId)
    .Select(g => new { CustomerId = g.Key, Revenue = g.Sum(o => o.Total) })
    .OrderByDescending(x => x.Revenue)
    .ToList();
```

---

## 4. Object-oriented programming
- **Encapsulation** — hide state behind methods/properties.
- **Inheritance** — `class Dog : Animal`. Prefer composition over deep trees.
- **Polymorphism** — `virtual`/`override`, interface implementation.
- **Abstraction** — `abstract class`, `interface`.

### Interfaces vs abstract classes
| | Interface | Abstract class |
|--|-----------|----------------|
| Multiple inheritance | Yes | No |
| Fields | No (props only) | Yes |
| Constructors | No | Yes |
| Default impl | Yes (C# 8+) | Yes |

### SOLID principles (see also file 09)
- **S**ingle responsibility, **O**pen/closed, **L**iskov substitution,
  **I**nterface segregation, **D**ependency inversion.

---

## 5. Modern C# features you should use
- `var`, target-typed `new()`
- Object & collection initializers
- Auto-properties, init-only setters (`init`)
- `record` types (immutable value-equality DTOs) — C# 9+
- Pattern matching & `switch` expressions
- Tuples and deconstruction
- Local functions
- Expression-bodied members `=> `
- `nameof`, `is`/`as`, null operators (`?.`, `??`, `??=`)
- Ranges and indices (`arr[^1]`, `arr[1..3]`) — C# 8+

```csharp
public record Money(decimal Amount, string Currency);

string Describe(object shape) => shape switch
{
    Circle c    => $"circle r={c.Radius}",
    Rectangle r => $"rect {r.W}x{r.H}",
    null        => "nothing",
    _           => "unknown"
};
```

---

## 6. Delegates, events, lambdas
- **Delegate** — a typed reference to a method. `Func<>`, `Action<>`, `Predicate<>`.
- **Lambda** — inline anonymous function: `x => x * 2`.
- **Event** — publisher/subscriber based on delegates.

```csharp
Func<int, int> square = x => x * x;
Action<string> log = msg => Console.WriteLine(msg);

public event EventHandler<OrderPlacedArgs> OrderPlaced;
OrderPlaced?.Invoke(this, new OrderPlacedArgs(order));
```

---

## 7. Asynchronous programming (must-know)
- `async`/`await` frees the thread while waiting on I/O (DB, HTTP, file).
- Return `Task`/`Task<T>` (or `ValueTask`); avoid `async void` except event handlers.
- **Never** block on async with `.Result` / `.Wait()` in ASP.NET — classic MVC
  has a synchronization context and this causes **deadlocks**.
- Use `ConfigureAwait(false)` in library code (not needed in ASP.NET Core).
- Parallelism (`Task.WhenAll`) ≠ async; CPU-bound work → `Task.Run`/PLINQ.

```csharp
public async Task<IActionResult> Details(int id)
{
    var product = await _repo.GetByIdAsync(id);   // non-blocking I/O
    if (product is null) return NotFound();
    return View(product);
}

// Parallel independent calls
var (a, b) = (GetAAsync(), GetBAsync());
await Task.WhenAll(a, b);
```

### Common async pitfalls
- Deadlock from `.Result` on a captured context.
- Forgetting to `await` (fire-and-forget swallows exceptions).
- `async` all the way — don't mix sync/async.

---

## 8. Memory & resource management
- **GC** is generational (gen 0/1/2 + large object heap). You rarely call it
  directly; write allocation-aware code instead.
- **`IDisposable`** — release unmanaged/expensive resources (DB connections,
  files, streams). Use `using` to guarantee disposal.
- Prefer the **`using` declaration** (C# 8): `using var conn = ...;`.
- Implement the dispose pattern when you own unmanaged resources.

```csharp
using (var conn = new SqlConnection(cs))
{
    conn.Open();
    // ...
} // Dispose() called even on exception
```

---

## 9. Generics
- Write reusable, type-safe code: `Repository<T>`, `Result<T>`.
- Constraints: `where T : class`, `new()`, `IComparable<T>`, etc.
- Covariance (`out`) / contravariance (`in`) for interfaces/delegates.

```csharp
public interface IRepository<T> where T : class, IEntity
{
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
}
```

---

## 10. Exception handling strategy
- Catch what you can **handle**; let the rest bubble to a global handler.
- Don't swallow exceptions silently. Log with context.
- Use custom exception types for domain errors.
- `finally` for cleanup; prefer `using` for disposables.
- Avoid exceptions for control flow (they're expensive).

```csharp
try
{
    await _payment.ChargeAsync(order);
}
catch (PaymentDeclinedException ex)
{
    _logger.LogWarning(ex, "Declined for order {OrderId}", order.Id);
    return View("Declined");
}
```

---

## Common pitfalls recap
- Using `IEnumerable` where `IQueryable` is needed (pulls whole table).
- Blocking on async (`.Result`) → deadlocks.
- String concatenation in loops.
- Mutable structs.
- Catch-all `catch {}` blocks.
- Forgetting to dispose connections/streams.

## Practice / interview questions
1. Explain the difference between `IEnumerable` and `IQueryable` with an EF example.
2. What is boxing and when does it hurt performance?
3. Why can `.Result` deadlock in ASP.NET MVC 5?
4. Difference between `Single`, `First`, and `FirstOrDefault`.
5. When would you use a `struct` over a `class`?
6. What does deferred execution mean? Show a bug it can cause.
7. How does the `using` statement relate to `IDisposable`?
8. What are `record` types and when are they useful?
