# Module 09 — Interfaces & Abstraction

## What is an Interface?

An **interface** is a contract: it declares **what** members a type must provide, without saying
**how**. A class or struct that implements the interface promises to supply those members.

```csharp
public interface IPaymentProcessor
{
    bool ProcessPayment(decimal amount);
    string Name { get; }
}

public class CreditCardProcessor : IPaymentProcessor
{
    public string Name => "Credit Card";
    public bool ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Charging {amount:C} to credit card");
        return true;
    }
}

public class PayPalProcessor : IPaymentProcessor
{
    public string Name => "PayPal";
    public bool ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Sending {amount:C} via PayPal");
        return true;
    }
}
```

By convention, interface names start with **`I`**.

## Programming to an Interface

Code depends on the abstraction, not the concrete type — enabling swappable implementations
(the key to testability and dependency injection).

```csharp
void Checkout(IPaymentProcessor processor, decimal total)
{
    if (processor.ProcessPayment(total))
        Console.WriteLine($"Paid with {processor.Name}");
}

Checkout(new CreditCardProcessor(), 99.99m);
Checkout(new PayPalProcessor(), 49.50m);
```

## Multiple Interface Implementation

A class can implement many interfaces (C# has no multiple **class** inheritance).

```csharp
public interface IReadable { string Read(); }
public interface IWritable { void Write(string data); }

public class File : IReadable, IWritable
{
    private string _content = "";
    public string Read() => _content;
    public void Write(string data) => _content += data;
}
```

## Explicit Interface Implementation

Used to resolve member name clashes or hide members from the public surface.

```csharp
public class Logger : IReadable, IWritable
{
    string IReadable.Read() => "log data";          // accessible only via IReadable
    void IWritable.Write(string data) { }

    // Usage requires casting to the interface:
    // ((IReadable)logger).Read();
}
```

## Default Interface Methods (C# 8+)

Interfaces can provide default implementations, so adding members doesn't break existing implementers.

```csharp
public interface ILogger
{
    void Log(string message);
    void LogError(string message) => Log($"ERROR: {message}"); // default body
}
```

## Interface vs Abstract Class

| Aspect | Interface | Abstract Class |
|--------|-----------|----------------|
| Instantiable | No | No |
| Multiple inheritance | Yes (implement many) | No (single base) |
| Fields | No (only constants since C# 8 via static) | Yes |
| Constructors | No | Yes |
| Access modifiers on members | public by default | any |
| State (instance fields) | No | Yes |
| Use when… | defining a capability/contract many unrelated types can share | sharing common code/state among closely related types |

**Rule of thumb:** "**can-do**" capability → interface (`IDisposable`, `IComparable`).
"**is-a**" with shared implementation → abstract class.

## Common BCL Interfaces Worth Knowing

```csharp
// IComparable<T> — define natural ordering for sorting
public class Product : IComparable<Product>
{
    public decimal Price { get; set; }
    public int CompareTo(Product? other) => Price.CompareTo(other?.Price);
}

// IEquatable<T> — value-based equality
// IEnumerable<T> — make a type foreach-able
// IDisposable — deterministic cleanup with 'using'
public class Connection : IDisposable
{
    public void Dispose() => Console.WriteLine("Connection closed");
}

using (var conn = new Connection())
{
    // ... use conn ...
} // Dispose() called automatically
```

## Abstraction (the Concept)

Abstraction means exposing **essential features** while hiding implementation detail. Both
interfaces and abstract classes are tools to achieve it. The goal: callers work with simple,
stable contracts and don't depend on volatile internals.

## Key Takeaways

- An interface is a contract specifying **what**, not **how**; prefix names with `I`.
- Program to interfaces to get loose coupling, testability, and DI.
- A type can implement many interfaces but inherit one class.
- Choose interface for capabilities/contracts, abstract class for shared base behavior + state.

➡️ Next: [Module 10 — Exception Handling](10-exception-handling.md)
