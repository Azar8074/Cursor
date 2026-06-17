# Module 02 — Variables & Data Types

## Declaring Variables

```csharp
int age = 30;            // explicit type
string name = "Alice";   // explicit type
var city = "London";     // implicit type — compiler infers 'string'
const double Pi = 3.14159; // compile-time constant (cannot change)
readonly int id;         // can only be set in constructor (used on fields)
```

> `var` is **not** dynamic typing. The type is still resolved at compile time; it's just inferred.

## The Two Type Categories

C# types fall into **value types** and **reference types** — understanding the difference is essential.

### Value Types

Stored **directly** on the stack (or inline within their containing object). Assigning copies the value.

| Type | Description | Range / Notes |
|------|-------------|---------------|
| `byte` / `sbyte` | 8-bit integer | 0–255 / −128–127 |
| `short` / `ushort` | 16-bit integer | |
| `int` / `uint` | 32-bit integer | most common integer |
| `long` / `ulong` | 64-bit integer | |
| `float` | 32-bit floating point | suffix `f`: `3.14f` |
| `double` | 64-bit floating point | default for decimals |
| `decimal` | 128-bit precise decimal | money/finance, suffix `m`: `9.99m` |
| `char` | single Unicode character | `'A'` |
| `bool` | true/false | |
| `struct` | user-defined value type | |
| `enum` | named integer constants | |

### Reference Types

Store a **reference (pointer)** to data on the **managed heap**. Assigning copies the reference, not the object.

- `string`, `object`, arrays, `class`, `interface`, `delegate`.

```csharp
int a = 5;
int b = a;     // b is a copy; changing b does not affect a
b = 10;        // a is still 5

int[] x = { 1, 2, 3 };
int[] y = x;   // y references the SAME array
y[0] = 99;     // x[0] is now also 99
```

## Default Values

```csharp
int i = default;     // 0
bool flag = default; // false
string s = default;  // null
```

## Type Conversion

```csharp
// Implicit (widening, safe) — no data loss
int n = 100;
long big = n;

// Explicit (narrowing, may lose data) — requires a cast
double d = 9.78;
int truncated = (int)d;   // 9

// Convert class / parsing
string text = "123";
int parsed = int.Parse(text);          // throws if invalid
bool ok = int.TryParse("abc", out int result); // safe; ok = false, result = 0
int viaConvert = Convert.ToInt32("456");
```

## Nullable Types

By default value types cannot be `null`. Add `?` to allow it:

```csharp
int? maybe = null;
if (maybe.HasValue)
    Console.WriteLine(maybe.Value);

int safe = maybe ?? -1;  // null-coalescing: use -1 if null
```

**Nullable reference types** (C# 8+, enabled via `<Nullable>enable</Nullable>`) help the compiler
warn you about possible `null` dereferences:

```csharp
string? canBeNull = null;   // allowed
string notNull = "value";   // compiler warns if you assign null
```

## Boxing and Unboxing

- **Boxing**: converting a value type to `object` (wraps it on the heap).
- **Unboxing**: extracting the value type back out (with a cast).

```csharp
int value = 42;
object boxed = value;        // boxing  (heap allocation — has a cost)
int unboxed = (int)boxed;    // unboxing (must match the original type)
```

> Boxing/unboxing hurts performance in hot paths. Prefer generics (`List<int>`) over non-generic
> collections (`ArrayList`) to avoid it.

## Strings

```csharp
string greeting = "Hello";
string interpolated = $"{greeting}, World! Length={greeting.Length}";
string verbatim = @"C:\temp\file.txt";   // no escaping needed
string multiline = """
    Raw string literal (C# 11)
    keeps   formatting.
    """;
```

Strings are **immutable** — every "modification" creates a new string. For heavy concatenation
use `StringBuilder`:

```csharp
var sb = new System.Text.StringBuilder();
for (int k = 0; k < 1000; k++) sb.Append(k);
string finalText = sb.ToString();
```

## Key Takeaways

- **Value types** copy by value; **reference types** copy the reference.
- Use `decimal` for money, `double` for general math, `int`/`long` for whole numbers.
- Prefer `var` when the type is obvious; prefer `TryParse` over `Parse` for untrusted input.
- Avoid boxing; strings are immutable — use `StringBuilder` for loops.

➡️ Next: [Module 03 — Operators & Expressions](03-operators-and-expressions.md)
