# ASP.NET MVC Developer — Complete Knowledge Notes

> A structured roadmap and reference for a developer with ~3 years of ASP.NET MVC
> experience who wants to consolidate fundamentals and grow toward senior level.

These notes are organized by topic. Each file is self-contained and goes from
fundamentals to advanced topics, with code samples, "why it matters", common
pitfalls, and interview-style questions.

## How to use these notes

1. **Audit yourself** — read the checklist below and mark what you already know.
2. **Fill the gaps** — open the topic file for anything you're unsure about.
3. **Practice** — every file ends with practice tasks / interview questions.
4. **Revisit** — these are living notes; add your own learnings as you go.

## Table of Contents

| # | Topic | File | Why it matters |
|---|-------|------|----------------|
| 01 | C# Language (fundamentals → advanced) | [01-csharp-language.md](./01-csharp-language.md) | The language everything is built on |
| 02 | ASP.NET MVC Core Concepts | [02-aspnet-mvc-core-concepts.md](./02-aspnet-mvc-core-concepts.md) | Your daily framework |
| 03 | Entity Framework & Data Access | [03-entity-framework-data-access.md](./03-entity-framework-data-access.md) | How you talk to the database |
| 04 | Web Fundamentals, HTTP, REST & Web API | [04-web-http-rest-api.md](./04-web-http-rest-api.md) | The platform under the framework |
| 05 | Frontend: Razor, JS, CSS | [05-frontend-razor-js-css.md](./05-frontend-razor-js-css.md) | The part users actually see |
| 06 | SQL & Databases | [06-sql-and-databases.md](./06-sql-and-databases.md) | Performance lives here |
| 07 | Security Best Practices | [07-security.md](./07-security.md) | Don't get breached |
| 08 | Testing | [08-testing.md](./08-testing.md) | Confidence to change code |
| 09 | Design Patterns & Architecture | [09-design-patterns-architecture.md](./09-design-patterns-architecture.md) | Senior-level thinking |
| 10 | Performance, Caching & Optimization | [10-performance-caching.md](./10-performance-caching.md) | Fast, scalable apps |
| 11 | DevOps, Deployment & Tooling | [11-devops-deployment-tooling.md](./11-devops-deployment-tooling.md) | Ship reliably |
| 12 | Migrating to ASP.NET Core / Modern .NET | [12-aspnet-core-migration.md](./12-aspnet-core-migration.md) | The future of your career |
| 13 | Career Growth, Soft Skills & Interview Prep | [13-career-and-interview.md](./13-career-and-interview.md) | Level up beyond code |

## Quick self-assessment checklist

Mark each item: ✅ confident · 🟡 shaky · ❌ don't know.

### Language & runtime
- [ ] Value vs reference types, boxing/unboxing, stack vs heap
- [ ] `IEnumerable` vs `IQueryable` vs `IList` and deferred execution
- [ ] `async`/`await`, `Task`, deadlocks, `ConfigureAwait`
- [ ] Garbage collection, `IDisposable`, `using`, finalizers
- [ ] Delegates, events, lambdas, LINQ internals
- [ ] Generics, covariance/contravariance
- [ ] Exception handling strategy (not just try/catch)

### Framework
- [ ] Full MVC request lifecycle (routing → model binding → action → result)
- [ ] Filters (auth, action, result, exception) and order of execution
- [ ] Model binding & validation (`ModelState`, data annotations)
- [ ] State management: TempData, ViewData, ViewBag, Session, cookies
- [ ] Dependency Injection (even in classic MVC via a container)
- [ ] Areas, partial views, view components, layouts, bundling

### Data
- [ ] EF change tracking, lazy/eager/explicit loading
- [ ] N+1 query problem and how to detect/fix it
- [ ] Transactions, concurrency (optimistic vs pessimistic)
- [ ] Writing efficient SQL, reading an execution plan, indexing

### Cross-cutting
- [ ] OWASP Top 10 and how MVC defends against each
- [ ] Authentication vs authorization; cookies, JWT, OAuth/OIDC
- [ ] Unit vs integration tests; mocking; test-first habits
- [ ] Caching layers (output, data, distributed)
- [ ] CI/CD, IIS configuration, logging & monitoring

> If most items are ✅, focus on architecture (09), performance (10), and
> migrating to modern .NET (12). If many are 🟡/❌, start at 01 and work down.

## A note on "ASP.NET MVC" vs "ASP.NET Core MVC"

If you've been doing **ASP.NET MVC 5** (the classic `System.Web`-based,
.NET Framework 4.x stack), the single most valuable next step is learning
**ASP.NET Core MVC** (cross-platform, `Microsoft.AspNetCore.*`). Concepts
transfer, but DI, configuration, middleware, and hosting are different.
See [12-aspnet-core-migration.md](./12-aspnet-core-migration.md). Topic files
note where classic and Core differ.
