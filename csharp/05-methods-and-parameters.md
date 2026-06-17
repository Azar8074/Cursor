# Module 05 — Methods & Parameters

A **method** is a named block of code that performs a task and optionally returns a value.

## Anatomy of a Method

```csharp
public int Add(int a, int b)   // access modifier, return type, name, parameters
{
    return a + b;              // method body
}
```

- **Access modifier**: `public`, `private`, `protected`, `internal` (controls visibility).
- **Return type**: the type returned, or `void` if nothing is returned.
- **Parameters**: typed inputs the caller supplies.

## Expression-Bodied Methods

For one-liners:

```csharp
public int Square(int x) => x * x;
public string Greeting => "Hi";   // expression-bodied property
```

## Parameter Passing

### By value (default)

A **copy** is passed. Changes inside the method don't affect the caller's variable.

```csharp
void Increment(int n) { n++; }   // caller's value unchanged
```

### ref — pass by reference (read/write)

```csharp
void Double(ref int n) { n *= 2; }
int x = 5;
Double(ref x);   // x is now 10
```

### out — output parameter (must be assigned inside)

```csharp
bool TryDivide(int a, int b, out int result)
{
    if (b == 0) { result = 0; return false; }
    result = a / b;
    return true;
}

if (TryDivide(10, 2, out int q))
    Console.WriteLine(q); // 5
```

### in — pass by reference, read-only (performance for large structs)

```csharp
double Distance(in Point p1, in Point p2) { /* cannot modify p1/p2 */ }
```

## Optional & Named Arguments

```csharp
void Log(string message, string level = "INFO", bool timestamp = true) { }

Log("Started");                          // uses defaults
Log("Error!", "ERROR");                  // override level
Log("Done", timestamp: false);           // named argument skips 'level'
```

## params — Variable Number of Arguments

```csharp
int Sum(params int[] numbers)
{
    int total = 0;
    foreach (int n in numbers) total += n;
    return total;
}

Sum(1, 2, 3);          // 6
Sum();                 // 0
Sum(new[] { 4, 5 });   // 9
```

## Method Overloading

Same name, **different parameter signatures**. The compiler picks the best match.

```csharp
int  Multiply(int a, int b)       => a * b;
double Multiply(double a, double b) => a * b;
int  Multiply(int a, int b, int c) => a * b * c;
```

> Overloads must differ by parameter **types or count** — not just the return type.

## Local Functions

A method declared **inside** another method. Great for helpers that aren't needed elsewhere.

```csharp
int Factorial(int n)
{
    return Compute(n);

    // local function can access enclosing variables
    int Compute(int x) => x <= 1 ? 1 : x * Compute(x - 1);
}
```

## Static vs Instance Methods

```csharp
class Calculator
{
    public static int Add(int a, int b) => a + b; // call as Calculator.Add(...)
    public int Total { get; private set; }
    public void AddToTotal(int n) => Total += n;   // needs an instance
}

Calculator.Add(2, 3);            // static — no object needed
var c = new Calculator();
c.AddToTotal(5);                 // instance
```

## Recursion

A method that calls itself. Always define a **base case** to avoid stack overflow.

```csharp
int Fibonacci(int n) => n < 2 ? n : Fibonacci(n - 1) + Fibonacci(n - 2);
```

## Extension Methods

Add methods to existing types without modifying them. Defined as `static` in a `static` class
with `this` on the first parameter.

```csharp
public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value)
        => string.IsNullOrEmpty(value);
}

// Usage — looks like an instance method
bool empty = "hello".IsNullOrEmpty();
```

LINQ is built entirely on extension methods.

## Key Takeaways

- Default passing is **by value**; use `ref`/`out`/`in` to pass references.
- Use `out` + `TryXxx` pattern for operations that can fail without throwing.
- Overloading varies by parameter signature, not return type.
- Extension methods let you "add" behavior to types you don't own — the foundation of LINQ.

➡️ Next: [Module 06 — Arrays & Collections](06-arrays-and-collections.md)
