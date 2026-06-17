# Module 04 — Control Flow

Control-flow statements decide **which** code runs and **how many times**.

## Conditional Statements

### if / else if / else

```csharp
int score = 75;

if (score >= 90)
    Console.WriteLine("A");
else if (score >= 80)
    Console.WriteLine("B");
else if (score >= 70)
    Console.WriteLine("C");
else
    Console.WriteLine("F");
```

### switch statement

```csharp
string role = "admin";

switch (role)
{
    case "admin":
        Console.WriteLine("Full access");
        break;
    case "editor":
    case "author":            // fall-through for multiple labels
        Console.WriteLine("Can edit");
        break;
    default:
        Console.WriteLine("Read only");
        break;
}
```

> Unlike C/Java, C# does **not** allow implicit fall-through between non-empty cases — you must
> `break`, `return`, `throw`, or `goto`.

### switch expression (C# 8+)

```csharp
string Grade(int score) => score switch
{
    >= 90 => "A",
    >= 80 => "B",
    >= 70 => "C",
    _     => "F"     // _ is the default (discard) pattern
};
```

## Pattern Matching

```csharp
object value = 42;

string Describe(object o) => o switch
{
    null            => "null",
    int n when n < 0 => "negative int",
    int             => "non-negative int",
    string s        => $"string of length {s.Length}",
    _               => "something else"
};
```

Tuple and property patterns:

```csharp
static string Quadrant((int x, int y) p) => p switch
{
    (0, 0)          => "origin",
    ( > 0, > 0)     => "Q1",
    ( < 0, > 0)     => "Q2",
    _               => "other"
};
```

## Loops

### for

```csharp
for (int i = 0; i < 5; i++)
    Console.WriteLine(i);  // 0 1 2 3 4
```

### while

```csharp
int n = 0;
while (n < 5)
{
    Console.WriteLine(n);
    n++;
}
```

### do-while (runs at least once)

```csharp
int input;
do
{
    Console.Write("Enter a positive number: ");
} while (!int.TryParse(Console.ReadLine(), out input) || input <= 0);
```

### foreach (iterate collections)

```csharp
string[] fruits = { "apple", "banana", "cherry" };
foreach (string fruit in fruits)
    Console.WriteLine(fruit);
```

`foreach` works on anything implementing `IEnumerable` / `IEnumerable<T>`.

## Jump Statements

```csharp
for (int i = 0; i < 10; i++)
{
    if (i == 3) continue;  // skip to next iteration
    if (i == 7) break;     // exit the loop entirely
    Console.WriteLine(i);
}
```

- `continue` — skip the rest of the current iteration.
- `break` — exit the nearest loop or switch.
- `return` — exit the current method (optionally returning a value).
- `goto` — jump to a label (rarely used; avoid except in switch).
- `throw` — raise an exception (see Module 10).

## Common Pitfalls

```csharp
// Off-by-one: use < not <= when iterating to array length
for (int i = 0; i < arr.Length; i++) { }

// Infinite loop: forgetting to change the condition variable
while (true) { /* must break eventually */ }

// Modifying a collection while iterating throws InvalidOperationException
foreach (var item in list) { list.Remove(item); } // DON'T
// Instead: iterate a copy, or use a for loop backwards, or RemoveAll
```

## Key Takeaways

- Prefer `switch` **expressions** and pattern matching for concise, readable branching.
- Use `foreach` for readability; use `for` when you need the index or to modify by position.
- Don't modify a collection while `foreach`-ing over it.
- `break`/`continue`/`return` control loop and method flow.

➡️ Next: [Module 05 — Methods & Parameters](05-methods-and-parameters.md)
