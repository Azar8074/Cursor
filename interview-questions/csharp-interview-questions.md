# C# Interview Questions & Answers

A comprehensive set of C# interview questions organized from fundamentals to advanced topics.
Use them to self-test after each module. Answers are concise but complete.

## Table of Contents

1. [Language Fundamentals](#language-fundamentals)
2. [Value vs Reference Types](#value-vs-reference-types)
3. [OOP](#oop)
4. [Methods & Properties](#methods--properties)
5. [Collections & Generics](#collections--generics)
6. [Exception Handling](#exception-handling)
7. [Delegates, Events & LINQ](#delegates-events--linq)
8. [Asynchronous Programming](#asynchronous-programming)
9. [Memory & Performance](#memory--performance)
10. [Advanced & Modern C#](#advanced--modern-c)

---

## Language Fundamentals

**1. What is C# and what is .NET?**
C# is a strongly typed, object-oriented language. .NET is the platform (runtime + libraries) it
runs on. C# compiles to **IL** (Intermediate Language), which the **CLR** JIT-compiles to native
code at runtime.

**2. What is the CLR?**
The Common Language Runtime — the virtual machine that executes managed code, providing JIT
compilation, garbage collection, type safety, exception handling, and security.

**3. What is the difference between managed and unmanaged code?**
Managed code runs under the CLR (memory and safety managed for you). Unmanaged code runs directly
on the OS (e.g., C/C++), managing its own memory.

**4. What is IL (MSIL/CIL)?**
CPU-independent Intermediate Language produced by the C# compiler. The JIT compiler converts it
to native machine code at runtime.

**5. What is the difference between `const` and `readonly`?**
`const` is a compile-time constant, implicitly static, must be initialized at declaration.
`readonly` is set at runtime, in the declaration or constructor, and can differ per instance.

**6. What is the difference between `var` and `dynamic`?**
`var` is **statically** typed (inferred at compile time). `dynamic` bypasses compile-time type
checking and resolves at **runtime** (can throw `RuntimeBinderException`).

**7. What are the access modifiers in C#?**
`public`, `private`, `protected`, `internal`, `protected internal`, `private protected`.

**8. What is the difference between `==` and `.Equals()`?**
For value types both compare values. For reference types, `==` compares references by default
(unless overloaded, like `string`), while `.Equals()` can be overridden for value comparison.

**9. What is boxing and unboxing?**
Boxing converts a value type to `object` (heap allocation). Unboxing extracts it back (with a
cast). Both have performance costs — generics avoid them.

**10. What is the difference between `string` and `StringBuilder`?**
`string` is **immutable** — each modification creates a new object. `StringBuilder` is mutable
and efficient for repeated concatenation (loops).

**11. What is string interning?**
The CLR keeps a pool of unique string literals so identical literals share one reference, saving
memory. Use `string.Intern()` to add to the pool.

**12. What is the difference between `i++` and `++i`?**
`i++` (post) returns the value then increments; `++i` (pre) increments then returns.

---

## Value vs Reference Types

**13. What is the difference between value types and reference types?**
Value types hold the data directly (stack/inline) and copy by value. Reference types hold a
reference to heap data and copy the reference. Examples: value — `int`, `struct`, `enum`;
reference — `class`, `string`, arrays, delegates.

**14. What is the difference between a `class` and a `struct`?**
`class` = reference type (heap, nullable, inheritance). `struct` = value type (stack/inline,
copy semantics, no inheritance besides interfaces, cannot be null unless `Nullable`). Use structs
for small, immutable values.

**15. What is a nullable value type?**
`int?` (`Nullable<int>`) allows a value type to be `null`. Has `.HasValue` and `.Value`.

**16. What are nullable reference types?**
A C# 8 feature (`<Nullable>enable</Nullable>`) giving compile-time warnings about possible null
dereferences. `string?` can be null; `string` should not.

**17. What is the difference between `default` and `null`?**
`default` gives a type's default value (0 for numbers, `false` for bool, `null` for reference
types). `null` is the absence of a reference (only valid for nullable types).

**18. Are strings value or reference types?**
Reference types, but **immutable** and with value-based equality for `==`, so they often *behave*
like value types.

**19. What is the stack vs the heap?**
The stack stores method frames and value-type locals (fast, auto-managed). The heap stores
reference-type objects (managed by the GC).

---

## OOP

**20. What are the four pillars of OOP?**
**Encapsulation** (hiding state), **Inheritance** (reuse via is-a), **Polymorphism** (many forms),
**Abstraction** (exposing essentials, hiding detail).

**21. What is encapsulation?**
Bundling data with the methods that operate on it and restricting direct access via access
modifiers — exposing a controlled public interface.

**22. What is inheritance? Does C# support multiple inheritance?**
A derived class inherits members from a base class (`:`). C# supports **single** class
inheritance but **multiple interface** implementation.

**23. What is polymorphism? Types?**
The ability to treat objects of different types through a common interface. **Compile-time**
(method overloading) and **runtime** (method overriding via `virtual`/`override`).

**24. What is the difference between `virtual`, `override`, and `new`?**
`virtual` marks an overridable member; `override` provides a new implementation with dynamic
dispatch; `new` **hides** the base member (resolved by the reference type, not the object type).

**25. What is an abstract class?**
A class that can't be instantiated and may have abstract members (no body) that derived classes
must implement, plus optional concrete members and state.

**26. What is the difference between an abstract class and an interface?**
Abstract class: single inheritance, can have fields/constructors/implementation/state. Interface:
multiple implementation, traditionally no state (default methods since C# 8). Use interface for
"can-do" contracts, abstract class for shared base + state.

**27. Can you instantiate an abstract class?**
No — only concrete derived classes.

**28. What is method overloading vs overriding?**
Overloading: same name, different parameter signatures, resolved at compile time. Overriding:
redefining a `virtual`/`abstract` base method in a derived class, resolved at runtime.

**29. What is a sealed class/method?**
`sealed` class can't be inherited; `sealed` override can't be further overridden.

**30. What is the `base` keyword?**
Accesses base-class members/constructors from a derived class.

**31. What is the `this` keyword?**
Refers to the current instance; disambiguates members and enables fluent chaining and indexers.

**32. What is a static class?**
A class that can't be instantiated and contains only static members (e.g., `Math`).

**33. What is a constructor? Types?**
A special method to initialize objects. Types: default, parameterized, static, copy, private,
and primary constructors (C# 12).

**34. What is a static constructor?**
Runs once, automatically, before the type is first used — to initialize static data. No access
modifiers or parameters.

**35. What is composition over inheritance?**
Favoring "has-a" relationships (containing objects) over deep inheritance hierarchies for
flexibility and reduced coupling.

**36. What is the difference between `is` and `as`?**
`is` tests type (returns bool, supports pattern matching). `as` attempts a cast, returning `null`
on failure instead of throwing.

---

## Methods & Properties

**37. What is the difference between `ref`, `out`, and `in`?**
`ref` passes by reference (must be initialized before). `out` passes by reference for output
(must be assigned inside). `in` passes by read-only reference (perf for large structs).

**38. What is the `params` keyword?**
Lets a method accept a variable number of arguments as an array: `Sum(params int[] nums)`.

**39. What are optional and named arguments?**
Optional: parameters with default values. Named: specifying arguments by parameter name, allowing
reordering and skipping optional ones.

**40. What is a property? Auto-implemented property?**
A member with `get`/`set` accessors wrapping a field. Auto-implemented properties (`{ get; set; }`)
let the compiler create the backing field.

**41. What is the difference between a field and a property?**
A field is a variable; a property exposes a field through accessors, allowing validation,
computation, and encapsulation. Prefer properties for public data.

**42. What are `init`-only setters?**
C# 9 setters that allow assignment only during object initialization, enabling immutable objects.

**43. What is an extension method?**
A static method (in a static class) with a `this` first parameter that adds behavior to an
existing type without modifying it. LINQ is built on them.

**44. What is a local function?**
A method declared inside another method; can access the enclosing scope's variables.

**45. What is recursion? Risk?**
A method calling itself; risk of `StackOverflowException` without a proper base case.

---

## Collections & Generics

**46. What is the difference between an array and a `List<T>`?**
Arrays are fixed-size; `List<T>` is dynamically resizable with rich methods (`Add`, `Remove`, etc.).

**47. What is the difference between `IEnumerable`, `ICollection`, and `IList`?**
`IEnumerable<T>`: iteration only. `ICollection<T>`: adds Count/Add/Remove. `IList<T>`: adds
index-based access.

**48. When would you use a `Dictionary`?**
For fast O(1) key→value lookups by a unique key.

**49. What is the difference between `Dictionary` and `Hashtable`?**
`Dictionary<K,V>` is generic and type-safe (no boxing). `Hashtable` is non-generic (boxes value
types, requires casting). Prefer `Dictionary`.

**50. What is a `HashSet`?**
A collection of unique elements with O(1) membership tests and set operations (union, intersect).

**51. What are generics? Benefits?**
Type-parameterized classes/methods. Benefits: compile-time type safety, no boxing (performance),
and code reuse.

**52. What are generic constraints?**
`where` clauses limiting the type argument: `class`, `struct`, `new()`, base class, interface,
`notnull`, `unmanaged`.

**53. What is covariance and contravariance?**
Covariance (`out`) allows using a more-derived type (e.g., `IEnumerable<string>` →
`IEnumerable<object>`). Contravariance (`in`) allows a less-derived type (e.g., `Action<object>`
→ `Action<string>`).

**54. What is the difference between `Queue` and `Stack`?**
`Queue<T>` is FIFO (Enqueue/Dequeue); `Stack<T>` is LIFO (Push/Pop).

**55. What are concurrent collections?**
Thread-safe collections in `System.Collections.Concurrent` (e.g., `ConcurrentDictionary`,
`ConcurrentQueue`, `BlockingCollection`).

---

## Exception Handling

**56. What is an exception?**
An object representing a runtime error, derived from `System.Exception`.

**57. Explain `try`, `catch`, `finally`.**
`try` wraps risky code; `catch` handles specific exceptions; `finally` always runs (cleanup).

**58. What is the difference between `throw` and `throw ex`?**
`throw;` re-throws preserving the original stack trace; `throw ex;` resets it (loses the origin).

**59. What is the exception hierarchy?**
`Exception` → `SystemException`/`ApplicationException` → specific types like
`NullReferenceException`, `ArgumentException`, `InvalidOperationException`.

**60. What are exception filters?**
The `when` clause that conditionally catches: `catch (Ex e) when (e.Code == 404)`.

**61. How do you create a custom exception?**
Derive from `Exception`, add constructors and useful properties/context.

**62. What is `IDisposable` and the `using` statement?**
`IDisposable.Dispose()` releases unmanaged resources deterministically; `using` guarantees
`Dispose` is called even on exceptions.

**63. When should you NOT use exceptions?**
For normal control flow (they're expensive). Use `TryParse`/`TryGetValue` patterns for expected
failures.

**64. What is the difference between `finally` and a finalizer?**
`finally` is a code block that always runs. A finalizer (`~Class()`) is called by the GC before
reclaiming an object (non-deterministic) — rarely needed.

---

## Delegates, Events & LINQ

**65. What is a delegate?**
A type-safe reference to a method (a function pointer with a signature).

**66. What is the difference between `Func`, `Action`, and `Predicate`?**
`Func<...,TResult>` returns a value; `Action<...>` returns void; `Predicate<T>` is `Func<T,bool>`.

**67. What is a multicast delegate?**
A delegate referencing multiple methods (`+=`/`-=`), invoked in order.

**68. What is an event?**
A delegate-based publish/subscribe mechanism; the `event` keyword restricts subscribers to
`+=`/`-=` only.

**69. What is a lambda expression?**
A concise anonymous function: `x => x * x`. Can capture enclosing variables (closures).

**70. What is a closure?**
A lambda/anonymous method that captures variables from its enclosing scope.

**71. What is LINQ?**
Language Integrated Query — a unified syntax to query collections, databases, XML, etc., via
query or method syntax.

**72. What is deferred (lazy) execution in LINQ?**
LINQ queries don't run until enumerated. `ToList()`/`Count()`/`First()` force immediate execution.

**73. What is the difference between `First` and `FirstOrDefault`?**
`First` throws if no element matches; `FirstOrDefault` returns the type's default (e.g., null/0).

**74. What is the difference between `Select` and `SelectMany`?**
`Select` projects each element; `SelectMany` flattens nested collections into one sequence.

**75. What is the difference between `IEnumerable` and `IQueryable`?**
`IEnumerable<T>` executes in memory (LINQ to Objects). `IQueryable<T>` builds an expression tree
translated to the data source (e.g., SQL by EF) — filtering happens at the database.

---

## Asynchronous Programming

**76. What is the difference between asynchronous and parallel programming?**
Async is about not blocking threads while waiting (mainly I/O). Parallelism is about doing
multiple computations simultaneously (CPU-bound, multiple cores).

**77. What are `async` and `await`?**
`async` marks a method that uses `await`; `await` suspends the method until a task completes
without blocking the calling thread, then resumes.

**78. What is a `Task`?**
An object representing an asynchronous operation; `Task<T>` returns a result.

**79. What return types can async methods have?**
`Task`, `Task<T>`, `ValueTask<T>`, and `void` (only for event handlers).

**80. Why is `async void` discouraged?**
You can't await it or catch its exceptions; unhandled exceptions can crash the process. Use only
for event handlers.

**81. What is the difference between `Task.WhenAll` and `Task.WhenAny`?**
`WhenAll` completes when all tasks finish; `WhenAny` completes when the first one finishes.

**82. What is a deadlock with async, and how do you avoid it?**
Blocking on async with `.Result`/`.Wait()` in a context that captures a sync context can deadlock.
Avoid by awaiting all the way up (and/or `ConfigureAwait(false)` in libraries).

**83. What is `ConfigureAwait(false)`?**
Tells `await` not to resume on the captured synchronization context — improves performance and
avoids deadlocks in library code.

**84. How do you cancel an async operation?**
Pass a `CancellationToken` (from a `CancellationTokenSource`) and check it / propagate it.

**85. What is `Task.Run` used for?**
Offloading CPU-bound work to a thread-pool thread. Don't use it to wrap I/O — use native async APIs.

---

## Memory & Performance

**86. What is garbage collection?**
Automatic reclamation of unreachable managed objects on the heap by the CLR's generational GC.

**87. What are GC generations?**
Gen 0 (short-lived), Gen 1 (intermediate), Gen 2 (long-lived). Most objects die young in Gen 0,
making collection efficient.

**88. What is the difference between `Dispose` and a finalizer?**
`Dispose` is deterministic cleanup you call (via `using`). A finalizer runs non-deterministically
during GC. Implement the dispose pattern with `GC.SuppressFinalize`.

**89. What is a memory leak in managed code?**
Objects that stay reachable unintentionally (e.g., event handler references, static caches),
preventing GC from reclaiming them.

**90. What is `Span<T>`?**
A stack-only, allocation-free view over contiguous memory (arrays, stack, strings) for
high-performance slicing without copying.

**91. What is the difference between `StringBuilder` and string concatenation performance-wise?**
Repeated `+` creates many temporary strings (O(n²) in loops). `StringBuilder` mutates a buffer
(O(n)).

**92. What is the large object heap (LOH)?**
A separate heap for objects ≥ 85,000 bytes; collected with Gen 2 and not compacted by default.

---

## Advanced & Modern C#

**93. What is a record?**
A reference (or value, `record struct`) type with value-based equality, concise syntax, immutability
support, and `with` expressions — ideal for DTOs.

**94. What is the difference between a record and a class?**
Records add value equality, `ToString`, deconstruction, and non-destructive copy (`with`) by
default; classes have reference equality unless overridden.

**95. What are tuples and deconstruction?**
Tuples group multiple values: `(int, string)`. Deconstruction splits them: `var (a, b) = tuple;`.

**96. What is pattern matching?**
Testing a value's shape/type and extracting data: type, relational, logical, property, and tuple
patterns, often with `switch` expressions.

**97. What is reflection?**
Inspecting and manipulating types/members at runtime via `System.Reflection`. Powerful but slow.

**98. What are attributes?**
Declarative metadata attached to code elements, read via reflection (e.g., `[Obsolete]`,
`[Serializable]`, custom attributes).

**99. What is an indexer?**
A member that lets an object be indexed like an array: `this[int index]`.

**100. What is operator overloading?**
Defining custom behavior for operators (`+`, `==`, etc.) on your types via `public static operator`.

**101. What is the difference between `Func` and an expression tree (`Expression<Func<...>>`)?**
`Func` is compiled executable code. `Expression<Func<...>>` is a data structure representing the
code, which can be analyzed/translated (e.g., EF translates it to SQL).

**102. What is the null-coalescing operator and null-conditional operator?**
`??` returns the right operand if the left is null. `?.` short-circuits member access to null
instead of throwing. `??=` assigns only if null.

**103. What is the `nameof` operator?**
Returns the string name of a variable/type/member at compile time — refactor-safe for argument
names and logging.

**104. What are `yield return` and iterators?**
`yield return` produces a sequence lazily, creating a state machine that returns elements one at
a time without building the whole collection.

**105. What is the difference between `Task` and `Thread`?**
A `Thread` is an OS-level thread you manage. A `Task` is a higher-level abstraction over the
thread pool representing work; preferred for async and parallelism.

---

## Quick-Fire Round

- **Difference between `String` and `string`?** `string` is a C# alias for the `System.String` type. Identical.
- **Is C# pass-by-value or reference?** By value by default (references are copied for reference types); use `ref`/`out` for true by-reference.
- **Can a `struct` inherit from a class?** No (only from interfaces); all structs derive from `System.ValueType`.
- **Can you override a non-virtual method?** No — use `new` to hide it (not true overriding).
- **What is the default access modifier for a class?** `internal`. For class members it's `private`.
- **What is the entry point of a C# app?** The `Main` method (or top-level statements).
- **Can constructors be inherited?** No, but base constructors are invoked via `: base(...)`.
- **Can an interface have a constructor?** No.
- **What is the difference between `readonly` and `const` again?** `const` = compile-time, static; `readonly` = runtime, set in ctor.
- **What is a jagged vs multidimensional array?** Jagged = array of arrays (`int[][]`); multidimensional = rectangular (`int[,]`).

---

➡️ Continue with the [ASP.NET MVC Interview Questions](aspnet-mvc-interview-questions.md) and the
[Coding Challenges](coding-challenges.md).
