# Module 10 — Exception Handling

Exceptions represent **runtime errors**. Proper handling keeps your app robust and gives useful
diagnostics instead of crashing.

## try / catch / finally

```csharp
try
{
    int[] data = { 1, 2, 3 };
    Console.WriteLine(data[10]);   // throws IndexOutOfRangeException
}
catch (IndexOutOfRangeException ex)
{
    Console.WriteLine($"Bad index: {ex.Message}");
}
catch (Exception ex)               // catch-all (most general goes LAST)
{
    Console.WriteLine($"Unexpected: {ex.Message}");
}
finally
{
    Console.WriteLine("Always runs — cleanup goes here");
}
```

- `try` — wraps risky code.
- `catch` — handles a specific exception type. Order from most-specific to most-general.
- `finally` — always executes (even on return/throw) — ideal for releasing resources.

## The Exception Hierarchy

All exceptions derive from `System.Exception`.

```text
Exception
├── SystemException
│   ├── NullReferenceException
│   ├── IndexOutOfRangeException
│   ├── InvalidOperationException
│   ├── ArgumentException
│   │   ├── ArgumentNullException
│   │   └── ArgumentOutOfRangeException
│   ├── FormatException
│   ├── ArithmeticException → DivideByZeroException, OverflowException
│   └── IOException → FileNotFoundException
└── ApplicationException (legacy base for custom exceptions)
```

### Common Exception Properties

```csharp
catch (Exception ex)
{
    Console.WriteLine(ex.Message);     // human-readable description
    Console.WriteLine(ex.StackTrace);  // call stack where it occurred
    Console.WriteLine(ex.InnerException?.Message); // wrapped cause
    Console.WriteLine(ex.Source);      // app/assembly that caused it
}
```

## Throwing Exceptions

```csharp
public void SetAge(int age)
{
    if (age < 0)
        throw new ArgumentOutOfRangeException(nameof(age), "Age cannot be negative");
}
```

### Re-throwing Correctly

```csharp
try { /* ... */ }
catch (Exception ex)
{
    Log(ex);
    throw;            // ✅ preserves the original stack trace
    // throw ex;      // ❌ resets the stack trace — avoid
}
```

### Wrapping (preserve the cause via InnerException)

```csharp
catch (SqlException ex)
{
    throw new DataAccessException("Failed to load orders", ex);
}
```

## Exception Filters (`when`)

Catch conditionally without unwinding the stack:

```csharp
try { /* ... */ }
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    Console.WriteLine("Resource not found");
}
```

## Custom Exceptions

```csharp
public class InsufficientFundsException : Exception
{
    public decimal Requested { get; }
    public decimal Available { get; }

    public InsufficientFundsException(decimal requested, decimal available)
        : base($"Requested {requested:C} but only {available:C} available")
    {
        Requested = requested;
        Available = available;
    }
}
```

## Resource Cleanup: using

`IDisposable` resources should be released deterministically. The `using` statement guarantees
`Dispose()` runs even if an exception is thrown.

```csharp
using (var reader = new StreamReader("file.txt"))
{
    string content = reader.ReadToEnd();
} // reader.Dispose() called here

// using declaration (C# 8) — disposed at end of enclosing scope
using var writer = new StreamWriter("out.txt");
writer.WriteLine("done");
```

## Best Practices

- **Catch only what you can handle.** Don't swallow exceptions silently.
- **Don't use exceptions for normal control flow** — they're expensive.
- Prefer **`TryParse`/`TryGetValue`** patterns over try/catch for expected failures.
- Throw the **most specific** exception type; include helpful messages and `nameof(param)`.
- Use `throw;` (not `throw ex;`) to preserve the stack trace.
- Validate arguments early (guard clauses) and throw `ArgumentException` family.
- Always release resources with `using` or `finally`.

```csharp
// Guard-clause example
public Order Create(string customer)
{
    ArgumentException.ThrowIfNullOrEmpty(customer); // .NET 7+ helper
    return new Order(customer);
}
```

## Key Takeaways

- `try`/`catch`/`finally` structure error handling; order catches specific → general.
- Use `throw;` to re-throw and preserve the stack trace; wrap with `InnerException`.
- Create custom exceptions for domain-specific errors with useful context.
- Use `using` for deterministic cleanup; don't use exceptions for ordinary control flow.

➡️ Next: [Module 11 — Delegates, Events & Lambdas](11-delegates-events-lambdas.md)
