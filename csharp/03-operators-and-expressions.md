# Module 03 — Operators & Expressions

## Arithmetic Operators

```csharp
int a = 10, b = 3;
Console.WriteLine(a + b);  // 13
Console.WriteLine(a - b);  // 7
Console.WriteLine(a * b);  // 30
Console.WriteLine(a / b);  // 3   (integer division truncates!)
Console.WriteLine(a % b);  // 1   (remainder / modulo)
Console.WriteLine(10.0 / 3); // 3.333... (floating-point division)
```

## Assignment & Compound Assignment

```csharp
int x = 5;
x += 3;  // 8
x -= 2;  // 6
x *= 2;  // 12
x /= 4;  // 3
x %= 2;  // 1
```

## Increment / Decrement

```csharp
int i = 5;
Console.WriteLine(i++); // prints 5, then i becomes 6 (post-increment)
Console.WriteLine(++i); // i becomes 7, prints 7      (pre-increment)
```

## Comparison Operators

`==`, `!=`, `>`, `<`, `>=`, `<=` — all return a `bool`.

> For reference types, `==` compares **references** by default (except `string`, which compares
> values). Override `Equals`/`==` for value-based comparison.

## Logical Operators

```csharp
bool t = true, f = false;
Console.WriteLine(t && f); // AND (short-circuits)
Console.WriteLine(t || f); // OR  (short-circuits)
Console.WriteLine(!t);     // NOT
```

**Short-circuit** means the right operand is not evaluated if the result is already known:

```csharp
if (obj != null && obj.IsValid) { } // safe: IsValid only checked when obj != null
```

## Null-Handling Operators

```csharp
string? name = null;

// Null-coalescing: returns left if not null, else right
string display = name ?? "Anonymous";

// Null-coalescing assignment: assign only if currently null
name ??= "Default";

// Null-conditional (?.): returns null instead of throwing
int? length = name?.Length;

// Combined
int len = customer?.Address?.Street?.Length ?? 0;
```

## Ternary (Conditional) Operator

```csharp
int age = 20;
string status = age >= 18 ? "Adult" : "Minor";
```

## Bitwise Operators

```csharp
int p = 0b1100; // 12
int q = 0b1010; // 10
Console.WriteLine(p & q); // AND -> 1000 (8)
Console.WriteLine(p | q); // OR  -> 1110 (14)
Console.WriteLine(p ^ q); // XOR -> 0110 (6)
Console.WriteLine(~p);    // NOT
Console.WriteLine(p << 1);// left shift  -> 24
Console.WriteLine(p >> 1);// right shift -> 6
```

These are commonly used with `[Flags]` enums:

```csharp
[Flags]
enum Permissions { None = 0, Read = 1, Write = 2, Execute = 4 }

var perms = Permissions.Read | Permissions.Write;
bool canWrite = perms.HasFlag(Permissions.Write); // true
```

## Type-Testing Operators

```csharp
object o = "hello";

if (o is string s)              // pattern matching: tests AND casts
    Console.WriteLine(s.Length);

string? cast = o as string;     // returns null if cast fails (no exception)
Type type = o.GetType();        // runtime type
bool isString = o is string;    // simple test
```

## Operator Precedence (high → low, abridged)

1. Postfix `x++`, `x--`, member access `.`, `()`
2. Unary `!`, `~`, `+`, `-`, `++x`, `--x`, casts
3. Multiplicative `* / %`
4. Additive `+ -`
5. Shift `<< >>`
6. Relational `< > <= >= is as`
7. Equality `== !=`
8. Bitwise `&` then `^` then `|`
9. Conditional AND `&&`, then OR `||`
10. Null-coalescing `??`
11. Conditional ternary `?:`
12. Assignment `= += -= ...`

> When in doubt, use parentheses for clarity rather than relying on precedence.

## Key Takeaways

- Integer division truncates; cast to `double`/`decimal` for fractional results.
- `&&`/`||` short-circuit — useful for safe null checks.
- Learn the null operators (`??`, `?.`, `??=`) — they eliminate verbose null checks.
- `is` with pattern matching tests and casts in one step.

➡️ Next: [Module 04 — Control Flow](04-control-flow.md)
