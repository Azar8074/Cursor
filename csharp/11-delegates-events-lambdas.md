# Module 11 — Delegates, Events & Lambdas

## Delegates

A **delegate** is a type-safe function pointer — a variable that holds a reference to a method
with a matching signature.

```csharp
// 1. Declare a delegate type
public delegate int MathOperation(int a, int b);

// 2. Methods matching the signature
int Add(int a, int b) => a + b;
int Multiply(int a, int b) => a * b;

// 3. Assign and invoke
MathOperation op = Add;
Console.WriteLine(op(2, 3));   // 5
op = Multiply;
Console.WriteLine(op(2, 3));   // 6
```

## Built-in Delegates: Func, Action, Predicate

You rarely need to declare custom delegate types — use these generic ones:

```csharp
// Func<...,TResult> — returns a value (last type param is the return type)
Func<int, int, int> add = (a, b) => a + b;
Func<int, bool> isEven = n => n % 2 == 0;

// Action<...> — returns void
Action<string> print = msg => Console.WriteLine(msg);
Action greet = () => Console.WriteLine("Hi");

// Predicate<T> — returns bool (Func<T,bool>)
Predicate<int> positive = n => n > 0;

add(2, 3);           // 5
print("Hello");      // Hello
isEven(4);           // true
```

## Lambda Expressions

Concise anonymous functions: `(parameters) => expression-or-block`.

```csharp
Func<int, int> square = x => x * x;
Func<int, int, int> sum = (a, b) => a + b;

Action<string> log = message =>
{
    var time = DateTime.Now;
    Console.WriteLine($"[{time}] {message}");   // statement-body lambda
};
```

Lambdas **capture** variables from their enclosing scope (closures):

```csharp
int factor = 10;
Func<int, int> multiply = x => x * factor; // captures 'factor'
Console.WriteLine(multiply(5)); // 50
```

## Anonymous Methods (older syntax)

```csharp
Func<int, int> doubler = delegate(int x) { return x * 2; };
```

## Multicast Delegates

Delegates can chain multiple methods with `+=` / `-=`. Invoking calls them in order.

```csharp
Action pipeline = () => Console.WriteLine("Step 1");
pipeline += () => Console.WriteLine("Step 2");
pipeline += () => Console.WriteLine("Step 3");
pipeline();   // runs all three
pipeline -= null; // remove handlers with -=
```

> For multicast `Func`, only the **last** return value is kept.

## Events

Events are a **publisher/subscriber** mechanism built on delegates. The `event` keyword restricts
subscribers to only `+=` / `-=` (they cannot invoke or overwrite the delegate).

```csharp
public class Button
{
    // Standard pattern: EventHandler<TEventArgs>
    public event EventHandler<string>? Clicked;

    public void Click()
    {
        // Raise the event (null-conditional protects against no subscribers)
        Clicked?.Invoke(this, "Button was clicked");
    }
}

// Subscribe
var button = new Button();
button.Clicked += (sender, message) => Console.WriteLine($"Handler 1: {message}");
button.Clicked += (sender, message) => Console.WriteLine($"Handler 2: {message}");
button.Click();
```

### The Standard Event Pattern

```csharp
public class Order
{
    public event EventHandler<OrderEventArgs>? OrderPlaced;

    protected virtual void OnOrderPlaced(OrderEventArgs e)
        => OrderPlaced?.Invoke(this, e);

    public void Place(decimal total)
        => OnOrderPlaced(new OrderEventArgs { Total = total });
}

public class OrderEventArgs : EventArgs
{
    public decimal Total { get; set; }
}
```

## Why Use Delegates & Events?

- **Callbacks** — pass behavior as data (e.g., LINQ, event handlers, sorting comparers).
- **Decoupling** — publishers don't know who's listening.
- **Extensibility** — plug in custom logic without modifying the source class.

```csharp
// Delegates power LINQ-style APIs:
var numbers = new List<int> { 1, 2, 3, 4, 5 };
var evens = numbers.FindAll(n => n % 2 == 0);  // Predicate<int>
numbers.ForEach(n => Console.WriteLine(n));     // Action<int>
```

## Key Takeaways

- A delegate is a type-safe reference to a method; `Func`/`Action`/`Predicate` cover most needs.
- Lambdas are concise anonymous functions and can capture surrounding variables (closures).
- Events wrap delegates to provide a safe publish/subscribe model; raise with `?.Invoke`.
- These features enable callbacks, decoupling, and the entire LINQ ecosystem.

➡️ Next: [Module 12 — Generics](12-generics.md)
