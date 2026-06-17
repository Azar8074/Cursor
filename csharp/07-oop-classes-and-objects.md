# Module 07 — OOP: Classes & Objects

Object-Oriented Programming organizes code around **objects** that bundle **data** (fields/properties)
and **behavior** (methods). C# is built on four OOP pillars: **Encapsulation, Inheritance,
Polymorphism, Abstraction** (this module covers the first; later modules cover the rest).

## Class vs Object

- A **class** is a blueprint/template.
- An **object** is an instance of a class created with `new`.

```csharp
public class Car
{
    public string Make { get; set; }
    public string Model { get; set; }
    public void Drive() => Console.WriteLine($"{Make} {Model} is driving");
}

var myCar = new Car { Make = "Toyota", Model = "Corolla" };
myCar.Drive();
```

## Fields

Variables that hold an object's state.

```csharp
public class Account
{
    private decimal _balance;     // private field (encapsulated)
    public const string Bank = "Acme";          // compile-time constant
    public static int Count;      // shared across all instances
    private readonly DateTime _created = DateTime.Now; // set once
}
```

## Properties

Smart fields with `get`/`set` accessors. Prefer properties over public fields.

```csharp
public class Person
{
    // Auto-implemented property (compiler creates the backing field)
    public string Name { get; set; }

    // Read-only (init via constructor or initializer)
    public int Id { get; }

    // Init-only setter (C# 9): settable only during initialization
    public string Email { get; init; }

    // Full property with validation
    private int _age;
    public int Age
    {
        get => _age;
        set
        {
            if (value < 0) throw new ArgumentException("Age cannot be negative");
            _age = value;
        }
    }

    // Computed (read-only) property
    public bool IsAdult => Age >= 18;
}
```

## Encapsulation & Access Modifiers

Hide internal state; expose a controlled public surface.

| Modifier | Accessible from |
|----------|-----------------|
| `public` | anywhere |
| `private` | only within the same class (default for members) |
| `protected` | the class and derived classes |
| `internal` | the same assembly |
| `protected internal` | same assembly **or** derived classes |
| `private protected` | derived classes within the same assembly |

```csharp
public class BankAccount
{
    private decimal _balance;                  // hidden state
    public decimal Balance => _balance;        // read-only view

    public void Deposit(decimal amount)        // controlled mutation
    {
        if (amount <= 0) throw new ArgumentException("Must be positive");
        _balance += amount;
    }
}
```

## Constructors

Special methods that initialize new objects.

```csharp
public class Rectangle
{
    public double Width { get; }
    public double Height { get; }

    // Default constructor
    public Rectangle() : this(1, 1) { } // constructor chaining with 'this'

    // Parameterized constructor
    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }

    // Static constructor — runs once before first use
    static Rectangle() => Console.WriteLine("Rectangle type initialized");

    public double Area => Width * Height;
}
```

### Primary Constructors (C# 12)

```csharp
public class Point(int x, int y)
{
    public int X { get; } = x;
    public int Y { get; } = y;
}
```

## Object Initializers

```csharp
var p = new Person
{
    Name = "Alice",
    Email = "alice@example.com",
    Age = 30
};
```

## The `this` Keyword

Refers to the current instance — used to disambiguate or chain.

```csharp
public class Builder
{
    private string _name;
    public Builder SetName(string name)
    {
        this._name = name;
        return this;     // enables fluent chaining
    }
}
```

## Static Members & Classes

```csharp
public static class MathHelper          // cannot be instantiated
{
    public static double Pi = 3.14159;
    public static double CircleArea(double r) => Pi * r * r;
}

double area = MathHelper.CircleArea(2);
```

## The `object` Class

Every type ultimately derives from `System.Object`, inheriting:

- `ToString()` — string representation (override for meaningful output).
- `Equals(object)` — equality comparison.
- `GetHashCode()` — hash for dictionaries/sets.
- `GetType()` — runtime type.

```csharp
public override string ToString() => $"{Name} ({Age})";
```

## Key Takeaways

- A class is a blueprint; objects are instances created with `new`.
- **Encapsulate** state with private fields and expose it through properties/methods.
- Constructors initialize objects; chain with `this(...)` and use object initializers for clarity.
- `static` members belong to the type; instance members belong to objects.

➡️ Next: [Module 08 — Inheritance & Polymorphism](08-inheritance-and-polymorphism.md)
