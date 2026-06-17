# 08 — MediatR & CQRS

**MediatR** is a small in-process messaging library. It lets a controller (or any
caller) **send a request** and have a separate **handler** process it — without
the caller knowing the handler. It's the most common way teams implement **CQRS**
in .NET and keep controllers thin.

> Note: MediatR moved to a commercial license for newer versions; many projects
> pin an older free version or use alternatives (e.g. **Wolverine**, or plain
> handler classes). The *pattern* is what matters and transfers regardless.

---

## 1. CQRS in one minute
**Command Query Responsibility Segregation** = separate the **write** model
(Commands that change state) from the **read** model (Queries that return data).
- **Command** — does something (CreateOrder, CancelOrder). Returns little/nothing.
- **Query** — returns data, no side effects (GetOrderById, ListOrders).

Benefits: clear intent, single-responsibility handlers, independent optimization
of reads vs writes. You do **not** need separate databases — CQRS is first a
code-organization pattern.

---

## 2. Why MediatR
- **Thin controllers** — they just build a request and `Send` it.
- **One handler per use-case** — easy to find, test, and reason about (SRP).
- **Decoupling** — caller depends on the request type, not the handler.
- **Pipeline behaviors** — cross-cutting concerns (validation, logging,
  transactions, caching) wrap every handler in one place.

**When *not* to:** small CRUD apps where it just adds indirection. Don't add it
for its own sake.

---

## 3. Install
```bash
dotnet add package MediatR
```
```csharp
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

---

## 4. A Query
```csharp
public record GetProductById(int Id) : IRequest<ProductDto?>;

public class GetProductByIdHandler : IRequestHandler<GetProductById, ProductDto?>
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    public GetProductByIdHandler(AppDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<ProductDto?> Handle(GetProductById req, CancellationToken ct)
    {
        return await _db.Products
            .Where(p => p.Id == req.Id)
            .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }
}
```

## 5. A Command
```csharp
public record CreateProduct(string Name, decimal Price) : IRequest<int>;

public class CreateProductHandler : IRequestHandler<CreateProduct, int>
{
    private readonly AppDbContext _db;
    public CreateProductHandler(AppDbContext db) => _db = db;

    public async Task<int> Handle(CreateProduct req, CancellationToken ct)
    {
        var product = new Product { Name = req.Name, Price = req.Price };
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return product.Id;
    }
}
```

## 6. The controller becomes thin
```csharp
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var dto = await mediator.Send(new GetProductById(id));
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProduct command)
    {
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id }, null);
    }
}
```

---

## 7. Pipeline behaviors (the killer feature)
Wrap **every** request with cross-cutting logic, defined once:
```csharp
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        foreach (var v in _validators)
        {
            var result = await v.ValidateAsync(request, ct);
            if (!result.IsValid) throw new ValidationException(result.Errors);
        }
        return await next();
    }
}

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```
Common behaviors: **validation** (with FluentValidation, file 07), **logging**,
**performance timing**, **transactions**, **caching**, **retry**.

---

## 8. Notifications (one-to-many, in-process events)
```csharp
public record OrderPlaced(int OrderId) : INotification;

public class SendConfirmationEmail : INotificationHandler<OrderPlaced>
{
    public Task Handle(OrderPlaced n, CancellationToken ct) { /* ... */ return Task.CompletedTask; }
}
public class UpdateInventory : INotificationHandler<OrderPlaced> { /* ... */ }

// Publish: both handlers run
await mediator.Publish(new OrderPlaced(order.Id));
```
Great for **domain events** within a process. For cross-service events, use a
**message queue** (file 05).

---

## 9. Typical project structure with CQRS
```
Features/
  Products/
    CreateProduct.cs        (command + handler + validator)
    GetProductById.cs       (query + handler)
    ProductDto.cs
```
"Vertical slice" organization: everything for one use-case lives together.

---

## Pitfalls & gotchas
- Adding MediatR/CQRS to a simple CRUD app (needless ceremony).
- Putting business logic in controllers anyway — keep it in handlers/domain.
- Fat handlers that do too much (one use-case per handler).
- Confusing in-process notifications with durable cross-service messaging.
- Forgetting to register behaviors/validators.
- License consideration for newer MediatR versions.

## Interview questions
1. What is CQRS and what problem does it address?
2. Do you need two databases for CQRS? (No — explain.)
3. How does MediatR keep controllers thin?
4. What are pipeline behaviors and what do you use them for?
5. Command vs Query vs Notification?
6. In-process notifications vs a message broker — when each?
7. When is MediatR overkill?
8. How would you add validation to every request in one place?
