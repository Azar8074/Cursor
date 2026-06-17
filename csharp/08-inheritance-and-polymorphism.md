# Module 08 — Inheritance & Polymorphism

## Inheritance

A class (**derived/child**) can inherit fields, properties, and methods from another
(**base/parent**) using `:`. This models an **"is-a"** relationship and promotes reuse.

```csharp
public class Animal
{
    public string Name { get; set; }
    public void Eat() => Console.WriteLine($"{Name} is eating");
    public virtual void Speak() => Console.WriteLine("Some sound"); // overridable
}

public class Dog : Animal     // Dog "is an" Animal
{
    public void Fetch() => Console.WriteLine($"{Name} fetches the ball");
    public override void Speak() => Console.WriteLine("Woof!"); // overrides base
}

var dog = new Dog { Name = "Rex" };
dog.Eat();    // inherited
dog.Fetch();  // own method
dog.Speak();  // Woof!
```

> C# supports **single** class inheritance only (one base class), but a class can implement
> **multiple interfaces**.

## Calling the Base Class — `base`

```csharp
public class Cat : Animal
{
    public Cat(string name)
    {
        Name = name;
    }

    public override void Speak()
    {
        base.Speak();             // call the base implementation
        Console.WriteLine("Meow!");
    }
}
```

Constructors are not inherited but the base constructor runs first; pass arguments with `: base(...)`:

```csharp
public class Employee : Person
{
    public Employee(string name) : base(name) { }
}
```

## Polymorphism

"Many forms" — the ability to treat derived objects through a base reference and have the
**correct overridden method** invoked at runtime (**dynamic dispatch**).

```csharp
Animal[] animals = { new Dog { Name = "Rex" }, new Cat("Tom") };

foreach (Animal a in animals)
    a.Speak();   // calls Dog.Speak then Cat.Speak — resolved at runtime
```

### virtual / override / new

```csharp
public class Base
{
    public virtual void Show() => Console.WriteLine("Base");
}

public class Derived : Base
{
    public override void Show() => Console.WriteLine("Derived"); // runtime polymorphism
}

public class Hidden : Base
{
    public new void Show() => Console.WriteLine("Hidden");       // hides, not overrides
}
```

| Keyword | Effect |
|---------|--------|
| `virtual` | marks a member as overridable |
| `override` | provides a new implementation; uses dynamic dispatch |
| `new` | **hides** the base member (compile-time, based on the reference type) |
| `sealed` | prevents further overriding / inheritance |

```csharp
public sealed class FinalDog : Dog { }       // cannot be inherited
public override sealed void Speak() { }      // cannot be overridden further
```

## Abstract Classes

A class that cannot be instantiated and may contain **abstract members** (no body) that derived
classes **must** implement.

```csharp
public abstract class Shape
{
    public abstract double Area();              // must be implemented
    public virtual string Describe() => $"Area = {Area()}"; // optional override
}

public class Circle : Shape
{
    public double Radius { get; set; }
    public override double Area() => Math.PI * Radius * Radius;
}

public class Square : Shape
{
    public double Side { get; set; }
    public override double Area() => Side * Side;
}
```

## Method Hiding vs Overriding (the classic trap)

```csharp
Base b = new Derived();
b.Show();   // "Derived" — override uses the actual object's type

Base h = new Hidden();
h.Show();   // "Base"  — new hides; the reference type decides
```

## Casting Up and Down

```csharp
Animal a = new Dog { Name = "Rex" };  // upcast (implicit, always safe)

Dog d = (Dog)a;                       // downcast (explicit, may throw)
if (a is Dog dog2)                    // safe pattern-based downcast
    dog2.Fetch();
Dog? maybe = a as Dog;                // null if not a Dog
```

## Composition over Inheritance

Inheritance is powerful but can create rigid hierarchies. Often **composition** (containing
other objects) is more flexible:

```csharp
public class Engine { public void Start() { } }

public class Car
{
    private readonly Engine _engine = new(); // Car "has-a" Engine
    public void Start() => _engine.Start();
}
```

## Key Takeaways

- Inheritance models **is-a**; composition models **has-a** — prefer composition when in doubt.
- `virtual` + `override` give runtime polymorphism; `new` only hides at compile time.
- Abstract classes define a partial template that subclasses must complete.
- Use `is`/`as`/pattern matching for safe downcasting.

➡️ Next: [Module 09 — Interfaces & Abstraction](09-interfaces-and-abstraction.md)
