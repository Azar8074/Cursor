# 09 — Design Patterns & Architecture

This is what moves you from "writes features" to "designs systems." Know the
principles, common patterns, and how to structure an application.

---

## 1. SOLID principles
- **S — Single Responsibility:** a class has one reason to change.
- **O — Open/Closed:** open for extension, closed for modification (extend via
  new types/strategies, not editing existing ones).
- **L — Liskov Substitution:** subtypes must be usable wherever the base is,
  without surprises.
- **I — Interface Segregation:** many small, focused interfaces > one fat one.
- **D — Dependency Inversion:** depend on abstractions, not concretions (enables DI).

```csharp
// DIP + SRP: controller depends on an abstraction, not a concrete service
public class OrdersController : Controller
{
    private readonly IOrderService _orders;        // abstraction
    public OrdersController(IOrderService orders) => _orders = orders;
}
```

Other guiding ideas: **DRY** (don't repeat yourself), **KISS** (keep it simple),
**YAGNI** (you aren't gonna need it), **separation of concerns**, **composition
over inheritance**, **law of Demeter**.

---

## 2. Dependency Injection & IoC
- **Inversion of Control:** the framework creates and wires your objects.
- **DI:** dependencies are passed in (constructor injection preferred).
- Benefits: testability, loose coupling, swappable implementations.
- Lifetimes (Core): Transient / Scoped / Singleton (file 02). Avoid the
  **captive dependency** trap (singleton holding a scoped service).

---

## 3. GoF patterns you'll actually use
### Creational
- **Factory / Factory Method** — create objects without `new` everywhere.
- **Abstract Factory** — families of related objects.
- **Builder** — step-by-step construction (fluent config).
- **Singleton** — one instance (prefer DI singleton over the classic static).

### Structural
- **Adapter** — make incompatible interfaces work together.
- **Decorator** — add behavior by wrapping (e.g. caching/logging a service).
- **Facade** — simple interface over a complex subsystem.
- **Proxy** — stand-in (EF lazy-loading proxies, caching proxy).

### Behavioral
- **Strategy** — swap algorithms behind an interface (e.g. pricing rules).
- **Observer** — events/pub-sub.
- **Command** — encapsulate a request as an object (CQRS handlers, MediatR).
- **Template Method** — base defines skeleton, subclass fills steps.
- **Mediator** — decouple senders/receivers (MediatR for in-process messaging).

```csharp
// Strategy
public interface IShippingCalculator { decimal Cost(Order o); }
public class StandardShipping : IShippingCalculator { public decimal Cost(Order o) => 5m; }
public class ExpressShipping  : IShippingCalculator { public decimal Cost(Order o) => 15m; }
```

```csharp
// Decorator: add caching without changing the original service
public class CachedProductService : IProductService
{
    private readonly IProductService _inner;
    private readonly IMemoryCache _cache;
    public CachedProductService(IProductService inner, IMemoryCache cache)
    { _inner = inner; _cache = cache; }

    public Task<ProductDto> GetAsync(int id) =>
        _cache.GetOrCreateAsync($"p:{id}", _ => _inner.GetAsync(id));
}
```

---

## 4. Application-level patterns
- **Repository** — abstract data access (note: EF already is one; add only if it
  earns its keep — file 03).
- **Unit of Work** — group operations into one transaction (DbContext is this).
- **Service / Application layer** — business use-cases; keep controllers thin.
- **DTO / ViewModel** — shape data for transport/views; decouple from entities.
- **Specification** — encapsulate query criteria as reusable objects.
- **CQRS** — separate read and write models/paths (often with MediatR).
- **Mapper** — AutoMapper or manual mapping between entities and DTOs.

---

## 5. Layered / Clean architecture
A common, maintainable structure:
```
┌─────────────────────────────────────────┐
│ Presentation (MVC controllers, views, API)│  ← depends inward only
├─────────────────────────────────────────┤
│ Application (use-cases, services, DTOs)   │
├─────────────────────────────────────────┤
│ Domain (entities, business rules)         │  ← no dependencies
├─────────────────────────────────────────┤
│ Infrastructure (EF, files, email, HTTP)   │  ← implements interfaces
└─────────────────────────────────────────┘
```
- **Dependency rule:** dependencies point *inward*; domain knows nothing about
  EF/ASP.NET. Infrastructure implements interfaces defined in inner layers.
- Variants: **Onion**, **Hexagonal (Ports & Adapters)**, **Clean Architecture**.
- **Domain-Driven Design (DDD):** entities, value objects, aggregates,
  repositories, domain events, bounded contexts — useful for complex domains.

---

## 6. Monolith vs microservices
- **Monolith** — one deployable. Simpler, easier to develop/debug; start here.
- **Modular monolith** — clean internal boundaries; great middle ground.
- **Microservices** — independently deployable services; scale teams &
  components, at the cost of distributed-system complexity (networking,
  consistency, observability, deployment).
- Don't adopt microservices for resume points — adopt them for real
  organizational/scaling needs.

### Distributed concerns (if you go there)
Messaging/queues (RabbitMQ, Azure Service Bus, Kafka), API gateway, service
discovery, resilience (retries, circuit breakers — Polly), eventual consistency,
the saga pattern, distributed tracing.

---

## 7. Cross-cutting concerns
Logging, caching, validation, auth, transactions, error handling — implement
once via **filters/middleware/decorators**, not copy-pasted in every action.

---

## 8. Picking the right amount of architecture
- Small app/CRUD: simple layered structure, services + EF. Don't over-engineer.
- Growing/complex domain: add application layer, CQRS where reads/writes diverge,
  DDD tactics for the complex core.
- Let pain (duplication, coupling, hard tests) drive structure — **YAGNI**.

---

## Common pitfalls
- Over-engineering simple apps (patterns for their own sake).
- Anemic layers that just pass calls through (needless indirection).
- Fat controllers / fat `DbContext` usage in the UI layer.
- Leaking EF entities to the UI/API.
- Microservices without the operational maturity to run them.
- Repository-over-EF that just re-wraps `DbSet`.

## Practice / interview questions
1. Explain each SOLID principle with an example.
2. What problem does DI solve? Constructor vs property injection?
3. Describe the Strategy and Decorator patterns with a use case each.
4. What is the dependency rule in Clean Architecture?
5. CQRS — what is it and when is it worth it?
6. Repository pattern over EF — pros, cons, do you need it?
7. Monolith vs microservices trade-offs?
8. How do you handle cross-cutting concerns in MVC?
