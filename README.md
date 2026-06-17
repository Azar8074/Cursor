# Complete Course: C# and ASP.NET MVC Framework

A comprehensive, hands-on course that takes you from C# language fundamentals all the way
through building real web applications with the ASP.NET MVC framework — finished off with a
large bank of interview questions and coding challenges to help you land the job.

> Each module is a self-contained Markdown lesson with explanations, runnable code examples,
> best practices, and "Key Takeaways". Work through them in order, or jump to a topic you need.

---

## How to Use This Course

1. **Start with the C# track** if you are new to the language or want a refresher.
2. **Move to the ASP.NET MVC track** once you are comfortable with classes, interfaces,
   collections, LINQ, and async/await.
3. **Practice with the Interview Questions** section throughout — don't wait until the end.
4. Type out the code samples yourself. Reading is not the same as doing.

### Prerequisites & Tooling

- **.NET SDK** (the modern cross-platform `dotnet` CLI) and/or **Visual Studio**.
- For the classic ASP.NET MVC 5 examples you will need **Visual Studio on Windows** with the
  *.NET Framework*. For cross-platform work, the same MVC concepts apply to **ASP.NET Core MVC**.
- A code editor: **Visual Studio**, **VS Code**, or **Rider**.

```bash
# Verify your installation
dotnet --version

# Create and run a quick console app to test C# samples
dotnet new console -o HelloCSharp
cd HelloCSharp
dotnet run
```

---

## Curriculum

### Part 1 — C# Programming Language

| # | Module | Topics |
|---|--------|--------|
| 01 | [Introduction to C# and .NET](csharp/01-introduction.md) | CLR, CTS, CLS, compilation, first program |
| 02 | [Variables & Data Types](csharp/02-variables-and-data-types.md) | Value vs reference types, nullable, boxing |
| 03 | [Operators & Expressions](csharp/03-operators-and-expressions.md) | Arithmetic, logical, null-coalescing |
| 04 | [Control Flow](csharp/04-control-flow.md) | if/switch, loops, pattern matching |
| 05 | [Methods & Parameters](csharp/05-methods-and-parameters.md) | ref/out/in, params, overloading, local functions |
| 06 | [Arrays & Collections](csharp/06-arrays-and-collections.md) | Arrays, List, Dictionary, HashSet, Queue, Stack |
| 07 | [OOP: Classes & Objects](csharp/07-oop-classes-and-objects.md) | Fields, properties, constructors, encapsulation |
| 08 | [OOP: Inheritance & Polymorphism](csharp/08-inheritance-and-polymorphism.md) | virtual/override, abstract, sealed |
| 09 | [Interfaces & Abstraction](csharp/09-interfaces-and-abstraction.md) | Interfaces vs abstract classes, default members |
| 10 | [Exception Handling](csharp/10-exception-handling.md) | try/catch/finally, custom exceptions |
| 11 | [Delegates, Events & Lambdas](csharp/11-delegates-events-lambdas.md) | Func/Action, events, anonymous methods |
| 12 | [Generics](csharp/12-generics.md) | Generic classes/methods, constraints, variance |
| 13 | [LINQ](csharp/13-linq.md) | Query/method syntax, deferred execution |
| 14 | [Asynchronous Programming](csharp/14-async-await.md) | Task, async/await, cancellation |
| 15 | [Files, Streams & Serialization](csharp/15-files-and-serialization.md) | File I/O, JSON, streams |
| 16 | [Advanced C#](csharp/16-advanced-topics.md) | Records, tuples, spans, reflection, GC |

### Part 2 — ASP.NET MVC Framework

| # | Module | Topics |
|---|--------|--------|
| 01 | [Introduction to ASP.NET MVC](aspnet-mvc/01-introduction.md) | What is MVC, history, project structure |
| 02 | [The MVC Architecture](aspnet-mvc/02-mvc-architecture.md) | Request lifecycle, separation of concerns |
| 03 | [Controllers & Actions](aspnet-mvc/03-controllers-and-actions.md) | Action results, parameters |
| 04 | [Views & Razor](aspnet-mvc/04-views-and-razor.md) | Razor syntax, layouts, partials |
| 05 | [Models & ViewModels](aspnet-mvc/05-models-and-viewmodels.md) | Model binding, ViewModels |
| 06 | [Routing](aspnet-mvc/06-routing.md) | Convention & attribute routing |
| 07 | [Validation & Data Annotations](aspnet-mvc/07-validation-and-data-annotations.md) | Server/client validation |
| 08 | [Entity Framework & Data Access](aspnet-mvc/08-entity-framework.md) | Code First, migrations, CRUD |
| 09 | [State Management](aspnet-mvc/09-state-management.md) | TempData, Session, cookies, cache |
| 10 | [Filters](aspnet-mvc/10-filters.md) | Action/Authorization/Exception/Result filters |
| 11 | [Authentication & Authorization](aspnet-mvc/11-authentication-authorization.md) | Identity, roles, claims |
| 12 | [Web API & AJAX](aspnet-mvc/12-web-api-and-ajax.md) | REST APIs, JSON, calling from JS |
| 13 | [Bundling, Minification & Areas](aspnet-mvc/13-bundling-and-areas.md) | Performance, modular apps |
| 14 | [Dependency Injection](aspnet-mvc/14-dependency-injection.md) | IoC containers, lifetimes |
| 15 | [Deployment & Best Practices](aspnet-mvc/15-deployment-and-best-practices.md) | Hosting, config, security |

### Part 3 — Interview Preparation

| Topic | Link |
|-------|------|
| C# Interview Questions (150+) | [csharp-interview-questions.md](interview-questions/csharp-interview-questions.md) |
| ASP.NET MVC Interview Questions (100+) | [aspnet-mvc-interview-questions.md](interview-questions/aspnet-mvc-interview-questions.md) |
| Coding Challenges & Solutions | [coding-challenges.md](interview-questions/coding-challenges.md) |

---

## License

This course material is released under the [MIT License](LICENSE). Feel free to use it for
self-study, teaching, and sharing.

Happy coding! 🚀
