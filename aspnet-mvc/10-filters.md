# Module 10 — Filters

**Filters** inject cross-cutting logic into the request pipeline — running code **before/after**
action execution. They're perfect for logging, authorization, caching, error handling, and more,
without cluttering your actions.

## Filter Types & Execution Order

| Filter | Interface (MVC 5) | Runs | Use case |
|--------|-------------------|------|----------|
| **Authorization** | `IAuthorizationFilter` | first | authentication/authorization |
| **Action** | `IActionFilter` | around the action | logging, modifying args/result |
| **Result** | `IResultFilter` | around the result | modify response |
| **Exception** | `IExceptionFilter` | on unhandled error | error handling/logging |

Execution order for a successful request:

```text
Authorization → Action (OnActionExecuting) → [ACTION] → Action (OnActionExecuted)
→ Result (OnResultExecuting) → [RESULT] → Result (OnResultExecuted)
(Exception filters run if an unhandled exception occurs)
```

## Built-in Filters

```csharp
[Authorize]                                  // require authentication
[Authorize(Roles = "Admin")]                 // require role
[AllowAnonymous]                             // opt out of [Authorize]
[HttpPost]                                   // verb constraint
[ValidateAntiForgeryToken]                   // CSRF protection
[OutputCache(Duration = 60)]                 // cache output (MVC 5)
[HandleError]                                // exception handling (MVC 5)
[RequireHttps]                               // force HTTPS
```

## Custom Action Filter (MVC 5)

```csharp
public class LogActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext ctx)
    {
        var action = ctx.ActionDescriptor.ActionName;
        Debug.WriteLine($"Executing: {action}");
        base.OnActionExecuting(ctx);
    }

    public override void OnActionExecuted(ActionExecutedContext ctx)
    {
        Debug.WriteLine($"Executed: {ctx.ActionDescriptor.ActionName}");
        base.OnActionExecuted(ctx);
    }
}
```

## Custom Exception Filter

```csharp
public class CustomErrorFilter : FilterAttribute, IExceptionFilter
{
    public void OnException(ExceptionContext ctx)
    {
        Logger.Log(ctx.Exception);
        ctx.Result = new ViewResult { ViewName = "Error" };
        ctx.ExceptionHandled = true;
    }
}
```

## Applying Filters at Three Levels

```csharp
// 1. Action level — one action
[LogActionFilter]
public ActionResult Index() => View();

// 2. Controller level — every action in the controller
[Authorize]
public class AdminController : Controller { }

// 3. Global level — every action in the app
// MVC 5: App_Start/FilterConfig.cs
public static void RegisterGlobalFilters(GlobalFilterCollection filters)
{
    filters.Add(new HandleErrorAttribute());
    filters.Add(new LogActionFilter());
}
```

## ASP.NET Core Filters

Core has the same filter concept plus `IAsyncActionFilter` and **Resource** filters. Register
globally in `Program.cs`:

```csharp
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<LogActionFilter>();           // global
});

public class LogActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }
    public void OnActionExecuted(ActionExecutedContext context) { }
}
```

### Async filter (Core)

```csharp
public class TimingFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var sw = Stopwatch.StartNew();
        await next();                 // run the action (and later filters)
        sw.Stop();
        Debug.WriteLine($"Took {sw.ElapsedMilliseconds}ms");
    }
}
```

### Filters with DI (`ServiceFilter` / `TypeFilter`)

```csharp
[ServiceFilter(typeof(LogActionFilter))]   // resolves from DI container
public IActionResult Index() => View();
```

## Filter Ordering

Control execution order with the `Order` property (lower runs first). By default scope order is
**Global → Controller → Action** for "executing" and reversed for "executed".

```csharp
[MyFilter(Order = 1)]
[AnotherFilter(Order = 2)]
public ActionResult Index() => View();
```

## Common Use Cases

- **Logging / auditing** — action filters.
- **Authentication / authorization** — `[Authorize]` / authorization filters.
- **Caching** — output/result filters.
- **Exception handling** — exception filters / global error handling.
- **Validation** — short-circuit invalid `ModelState` in an action filter.

```csharp
public class ValidateModelFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext ctx)
    {
        if (!ctx.ModelState.IsValid)
            ctx.Result = new BadRequestObjectResult(ctx.ModelState);
    }
    public void OnActionExecuted(ActionExecutedContext ctx) { }
}
```

## Key Takeaways

- Filters run cross-cutting logic around the pipeline: **Authorization → Action → Result → (Exception)**.
- Apply at **action**, **controller**, or **global** level; control order with `Order`.
- Built-ins cover auth (`[Authorize]`), CSRF (`[ValidateAntiForgeryToken]`), caching, and errors.
- In Core, use async filters and `[ServiceFilter]`/`[TypeFilter]` to inject dependencies.

➡️ Next: [Module 11 — Authentication & Authorization](11-authentication-authorization.md)
