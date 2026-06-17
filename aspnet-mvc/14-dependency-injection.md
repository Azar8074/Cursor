# Module 14 — Dependency Injection

**Dependency Injection (DI)** is a technique where a class receives its dependencies from the
outside (via constructor, usually) instead of creating them itself. It implements the
**Inversion of Control (IoC)** principle and is fundamental to testable, maintainable MVC apps.

## The Problem DI Solves

```csharp
// ❌ Tight coupling — hard to test, hard to change
public class OrderController : Controller
{
    private readonly OrderService _service = new OrderService(new SqlOrderRepository());

    public ActionResult Index() => View(_service.GetAll());
}
```

The controller is welded to concrete classes. You can't swap the repository or unit-test without
a database.

```csharp
// ✅ Loose coupling — depend on an abstraction, injected in
public class OrderController : Controller
{
    private readonly IOrderService _service;
    public OrderController(IOrderService service) => _service = service; // injected

    public ActionResult Index() => View(_service.GetAll());
}
```

Now any `IOrderService` can be supplied (real, fake, mock).

## Benefits

- **Testability** — inject mocks/stubs in unit tests.
- **Loose coupling** — depend on interfaces, not implementations.
- **Flexibility** — swap implementations via configuration.
- **Maintainability** — single place to wire up dependencies.
- **Single Responsibility** — classes focus on their job, not on building collaborators.

## Built-in DI Container (ASP.NET Core)

ASP.NET Core has DI built in. Register services in `Program.cs`; they're injected automatically
into controllers, filters, views, and other services.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Register your services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

var app = builder.Build();
```

## Service Lifetimes

| Lifetime | Method | New instance… | Use for |
|----------|--------|---------------|---------|
| **Transient** | `AddTransient` | every time it's requested | lightweight, stateless services |
| **Scoped** | `AddScoped` | once per HTTP request | `DbContext`, per-request services |
| **Singleton** | `AddSingleton` | once for the app lifetime | caches, config, stateless shared services |

```csharp
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();  // per resolution
builder.Services.AddScoped<AppDbContext>();                     // per request
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();     // app-wide
```

> ⚠️ **Captive dependency pitfall:** never inject a **scoped** service into a **singleton** — the
> scoped service would be captured for the app's lifetime. The container can detect some of these.

## Constructor Injection (the standard)

```csharp
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;
    private readonly IEmailSender _email;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository repo, IEmailSender email,
                        ILogger<OrderService> logger)
    {
        _repo = repo;
        _email = email;
        _logger = logger;
    }
}
```

## Injecting into Views & Actions

```html
@inject IConfiguration Config
<p>Version: @Config["App:Version"]</p>
```

```csharp
// Action-level injection with [FromServices]
public IActionResult Index([FromServices] IClock clock)
    => Content(clock.Now.ToString());
```

## DI in MVC 5 (not built-in)

MVC 5 has no built-in container, so you use a third-party one and a custom
`IDependencyResolver` / `IControllerActivator`. Popular containers: **Autofac**, **Ninject**,
**Unity**, **StructureMap**, **Castle Windsor**.

```csharp
// Example: Autofac in Global.asax / App_Start
var builder = new ContainerBuilder();
builder.RegisterControllers(typeof(MvcApplication).Assembly);
builder.RegisterType<OrderService>().As<IOrderService>().InstancePerRequest();
var container = builder.Build();
DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
```

## IoC Container Responsibilities

An IoC/DI container:

1. **Registers** mappings (interface → implementation) with lifetimes.
2. **Resolves** a requested type, constructing it and all its dependencies recursively.
3. **Manages lifetime** (dispose scoped/transient `IDisposable`s appropriately).

## Registering Multiple Implementations

```csharp
builder.Services.AddScoped<INotification, EmailNotification>();
builder.Services.AddScoped<INotification, SmsNotification>();

// Inject all of them
public class Notifier
{
    public Notifier(IEnumerable<INotification> channels) { /* loop & send */ }
}
```

## Testing with DI

```csharp
[Fact]
public void Index_ReturnsAllOrders()
{
    var mockService = new Mock<IOrderService>();
    mockService.Setup(s => s.GetAll()).Returns(new List<Order> { new() });

    var controller = new OrderController(mockService.Object);  // inject the mock
    var result = controller.Index() as ViewResult;

    Assert.NotNull(result);
}
```

## Key Takeaways

- DI supplies dependencies from outside (usually via constructor), implementing IoC.
- It yields loose coupling, testability, and flexibility — depend on **interfaces**.
- ASP.NET Core has a **built-in container**; register with `AddTransient`/`AddScoped`/`AddSingleton`.
- Match lifetimes carefully (`DbContext` = scoped); never capture scoped services in singletons.

➡️ Next: [Module 15 — Deployment & Best Practices](15-deployment-and-best-practices.md)
