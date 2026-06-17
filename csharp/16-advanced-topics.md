# Module 16 — Advanced C#

A tour of modern and advanced features that round out your C# expertise.

## Records (C# 9+)

Immutable reference types with **value-based equality**, concise syntax, and built-in
`ToString`, `Equals`, `GetHashCode`, and deconstruction. Ideal for DTOs and domain models.

```csharp
public record Person(string FirstName, string LastName);

var a = new Person("Ada", "Lovelace");
var b = new Person("Ada", "Lovelace");
Console.WriteLine(a == b);          // True — value equality (not reference)

// Non-destructive mutation with 'with'
var c = a with { LastName = "Byron" };

// Deconstruction
var (first, last) = a;

public record struct Point(int X, int Y); // value-type record (C# 10)
```

## Tuples & Deconstruction

```csharp
(string name, int age) person = ("Alice", 30);
Console.WriteLine(person.name);

// Returning multiple values
(int min, int max) GetRange(int[] nums) => (nums.Min(), nums.Max());
var (lo, hi) = GetRange(new[] { 3, 1, 4 });
```

## Pattern Matching (advanced)

```csharp
object shape = new { Width = 10, Height = 5 };

string result = shape switch
{
    int n and > 0           => "positive int",
    string { Length: > 5 }  => "long string",
    null                    => "null",
    _                       => "other"
};

// Relational and logical patterns
static string Classify(int temp) => temp switch
{
    < 0          => "freezing",
    >= 0 and < 20 => "cold",
    >= 20 and < 30 => "warm",
    _            => "hot"
};
```

## Nullable Reference Types

Enable in the project (`<Nullable>enable</Nullable>`) to get compile-time null-safety warnings.

```csharp
string nonNull = "value";   // cannot be null
string? nullable = null;    // may be null
int len = nullable!.Length; // null-forgiving operator (you assert it's not null)
```

## Extension Members & LINQ recap

Extension methods (Module 05) underpin LINQ. You can extend any type, including interfaces.

## Indexers

Let objects be indexed like arrays.

```csharp
public class Week
{
    private readonly string[] _days =
        { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
    public string this[int index] => _days[index];
}

var w = new Week();
Console.WriteLine(w[0]); // Mon
```

## Operator Overloading

```csharp
public readonly struct Money
{
    public decimal Amount { get; }
    public Money(decimal amount) => Amount = amount;
    public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount);
    public override string ToString() => $"{Amount:C}";
}

var total = new Money(10) + new Money(5); // $15.00
```

## Reflection

Inspect and manipulate types at runtime.

```csharp
using System.Reflection;

Type type = typeof(Person);
foreach (PropertyInfo prop in type.GetProperties())
    Console.WriteLine(prop.Name);

object? instance = Activator.CreateInstance(typeof(Person), "Ada", "Lovelace");
MethodInfo? method = type.GetMethod("ToString");
method?.Invoke(instance, null);
```

> Reflection is powerful but slow and bypasses compile-time safety. Use it for frameworks,
> serializers, and DI containers — not hot paths.

## Attributes

Metadata you attach to code, read via reflection.

```csharp
[AttributeUsage(AttributeTargets.Method)]
public class AuthorAttribute : Attribute
{
    public string Name { get; }
    public AuthorAttribute(string name) => Name = name;
}

public class Service
{
    [Author("Alice")]
    public void DoWork() { }
}
```

## Span&lt;T&gt; & Memory&lt;T&gt;

High-performance, allocation-free slices of contiguous memory (arrays, stack, strings).

```csharp
Span<int> numbers = stackalloc int[] { 1, 2, 3, 4, 5 };
Span<int> slice = numbers.Slice(1, 3);   // {2,3,4} — no copy

ReadOnlySpan<char> text = "Hello World".AsSpan();
ReadOnlySpan<char> word = text.Slice(0, 5); // "Hello" without allocating a new string
```

## Garbage Collection (GC) Essentials

- The CLR automatically frees unreachable managed objects via a **generational** GC (Gen 0/1/2).
- You rarely call `GC` directly. For **unmanaged** resources, implement `IDisposable` and use
  `using` for deterministic cleanup.

```csharp
public class ResourceHolder : IDisposable
{
    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        // release unmanaged resources here
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
```

## `dynamic` and `var` (don't confuse them)

```csharp
var s = "hello";    // statically typed as string at compile time
dynamic d = "hello"; // type resolved at RUNTIME; no compile-time checking
d = 42;              // allowed
```

## Local Functions, `nameof`, String Interpolation Recap

```csharp
void Validate(string input)
{
    if (input is null) throw new ArgumentNullException(nameof(input));
    Console.WriteLine($"Validating: {input}");
}
```

## Key Takeaways

- **Records** give you concise, immutable, value-equal types — perfect for DTOs.
- Advanced **pattern matching**, tuples, and deconstruction make code expressive and safe.
- **Reflection** and **attributes** power frameworks; **Span<T>** powers high-performance code.
- The GC manages memory automatically; use `IDisposable`/`using` for unmanaged resources.

🎉 You've completed the C# track! Continue with the
➡️ [ASP.NET MVC Track](../aspnet-mvc/01-introduction.md) or review the
[C# Interview Questions](../interview-questions/csharp-interview-questions.md).
