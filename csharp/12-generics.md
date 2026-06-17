# Module 12 — Generics

Generics let you write **type-parameterized** code that works with any type while keeping
**compile-time type safety** and **avoiding boxing**. `T` is a placeholder filled in at usage.

## The Problem Generics Solve

```csharp
// Without generics: lose type safety, box value types
object[] items = new object[2];
items[0] = 5;
int x = (int)items[0];   // cast required, error-prone

// With generics: type-safe, no casting, no boxing
var list = new List<int> { 5 };
int y = list[0];         // strongly typed
```

## Generic Classes

```csharp
public class Box<T>
{
    private T _value;
    public void Set(T value) => _value = value;
    public T Get() => _value;
}

var intBox = new Box<int>();
intBox.Set(42);
int n = intBox.Get();

var strBox = new Box<string>();
strBox.Set("hello");
```

### A Generic Repository (very common in real apps)

```csharp
public class Repository<T>
{
    private readonly List<T> _items = new();
    public void Add(T item) => _items.Add(item);
    public IEnumerable<T> GetAll() => _items;
    public T? Find(Func<T, bool> predicate) => _items.FirstOrDefault(predicate);
}
```

## Generic Methods

```csharp
public static void Swap<T>(ref T a, ref T b)
{
    (a, b) = (b, a);   // tuple swap
}

int p = 1, q = 2;
Swap(ref p, ref q);    // T inferred as int

public static T Max<T>(T a, T b) where T : IComparable<T>
    => a.CompareTo(b) >= 0 ? a : b;
```

## Constraints (`where`)

Constraints restrict the types allowed and unlock operations on `T`.

```csharp
where T : class            // T must be a reference type
where T : struct           // T must be a value type
where T : new()            // T must have a public parameterless constructor
where T : SomeBaseClass    // T must inherit SomeBaseClass
where T : ISomeInterface   // T must implement the interface
where T : notnull          // T must be non-nullable
where T : unmanaged        // T must be an unmanaged type
where T : U                // T must derive from another type param U
```

```csharp
public class Factory<T> where T : new()
{
    public T Create() => new T();   // allowed because of new() constraint
}

public T GetOrCreate<T>(T? existing) where T : class, new()
    => existing ?? new T();
```

Multiple constraints combine:

```csharp
public class Cache<TKey, TValue>
    where TKey : notnull
    where TValue : class, new()
{ }
```

## Multiple Type Parameters

```csharp
public class Pair<TFirst, TSecond>
{
    public TFirst First { get; set; }
    public TSecond Second { get; set; }
}

var pair = new Pair<string, int> { First = "Age", Second = 30 };
```

## Generic Interfaces

```csharp
public interface IRepository<T> where T : class
{
    void Add(T entity);
    T? GetById(int id);
    IEnumerable<T> GetAll();
}

public class UserRepository : IRepository<User> { /* ... */ }
```

## Variance: covariance & contravariance

Controls assignment compatibility of generic interfaces/delegates.

```csharp
// Covariance (out): IEnumerable<Derived> -> IEnumerable<Base>
IEnumerable<string> strings = new List<string> { "a" };
IEnumerable<object> objects = strings;   // allowed because IEnumerable<out T>

// Contravariance (in): Action<Base> -> Action<Derived>
Action<object> printObj = o => Console.WriteLine(o);
Action<string> printStr = printObj;      // allowed because Action<in T>
```

- `out T` (covariant): T appears in **output** positions (return values).
- `in T` (contravariant): T appears in **input** positions (parameters).

## default(T)

```csharp
public T GetDefault<T>() => default;   // 0 for numbers, null for refs
```

## Benefits Recap

- **Type safety** at compile time (no invalid casts at runtime).
- **Performance** (no boxing/unboxing of value types).
- **Reusability** (one implementation works for all types).
- **Cleaner code** (no casts everywhere).

## Key Takeaways

- Generics parameterize types/methods while preserving compile-time safety and avoiding boxing.
- Use `where` constraints to enable operations and restrict valid type arguments.
- The entire generic collections library (`List<T>`, `Dictionary<K,V>`) is built on generics.
- Covariance (`out`) and contravariance (`in`) control generic assignment compatibility.

➡️ Next: [Module 13 — LINQ](13-linq.md)
