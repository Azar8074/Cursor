# Module 01 — Introduction to C# and .NET

## What is C#?

C# (pronounced "C-sharp") is a modern, **object-oriented**, **type-safe** programming language
developed by Microsoft and led by Anders Hejlsberg. It runs on the **.NET platform** and is one
of the most popular languages for building:

- Web applications (ASP.NET / ASP.NET Core)
- Desktop apps (WPF, WinForms, MAUI)
- Cloud services and microservices
- Games (Unity)
- Mobile apps (.NET MAUI / Xamarin)

## What is .NET?

**.NET** is the development platform (runtime + libraries + tooling) on which C# runs.

| Term | Meaning |
|------|---------|
| **.NET Framework** | The original Windows-only implementation (versions 1.0–4.8). Used by classic ASP.NET MVC 5. |
| **.NET Core / .NET 5+** | The modern, cross-platform, open-source implementation. ".NET" now means this. |
| **CLR (Common Language Runtime)** | The virtual machine that executes managed code: JIT compilation, garbage collection, type safety, exception handling. |
| **BCL (Base Class Library)** | The huge set of built-in types (`System.*`) such as `string`, `List<T>`, `File`, etc. |
| **CTS (Common Type System)** | Defines how types are declared and used so all .NET languages interoperate. |
| **CLS (Common Language Specification)** | A subset of CTS that guarantees cross-language interoperability. |

## How C# Code Runs (Compilation Pipeline)

```text
   C# source (.cs)
        │  C# compiler (Roslyn / csc)
        ▼
   IL (Intermediate Language) + metadata  →  packaged into an assembly (.dll / .exe)
        │  CLR loads the assembly
        ▼
   JIT (Just-In-Time) compiler converts IL → native machine code at runtime
        ▼
   CPU executes native code
```

- **Managed code**: code that runs under the control of the CLR (memory, security, types managed for you).
- **Assembly**: the unit of deployment — a `.dll` or `.exe` containing IL + metadata + a manifest.

## Your First C# Program

Create a console project with the CLI:

```bash
dotnet new console -o HelloWorld
cd HelloWorld
dotnet run
```

### Classic (explicit) style

```csharp
using System;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
```

### Modern top-level statements (C# 9+)

```csharp
// Program.cs — no class or Main required
Console.WriteLine("Hello, World!");
```

Both compile to the same thing. `Main` is the **entry point** of an application.

## Anatomy of the Program

- `using System;` — imports a **namespace** so you can use its types without full qualification.
- `namespace` — a logical container that organizes types and prevents name clashes.
- `class Program` — a class; the basic building block of OOP in C#.
- `static void Main` — the application entry point. `static` means it belongs to the type, not an instance.
- `Console.WriteLine(...)` — calls a method on the `Console` class to print a line.

## Comments

```csharp
// Single-line comment

/* Multi-line
   comment */

/// <summary>XML documentation comment — used by IntelliSense and doc generators.</summary>
```

## Key Takeaways

- C# is a strongly typed, object-oriented language that compiles to **IL**, which the **CLR**
  JIT-compiles to native code.
- ".NET" today means the cross-platform .NET 5+; ".NET Framework" is the legacy Windows-only stack.
- Every executable needs an entry point (`Main` or top-level statements).
- The **BCL** gives you thousands of ready-made types — learn to lean on it.

➡️ Next: [Module 02 — Variables & Data Types](02-variables-and-data-types.md)
