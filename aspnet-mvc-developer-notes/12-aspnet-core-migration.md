# 12 — Migrating to ASP.NET Core / Modern .NET

If your 3 years were on **ASP.NET MVC 5 (.NET Framework 4.x)**, the single most
career-defining move is learning **ASP.NET Core** on **modern .NET** (.NET 8/9+).
The MVC concepts transfer; the plumbing is different and better.

---

## 1. Why migrate
- **Cross-platform** (Windows/Linux/macOS, Docker, cloud-native).
- **High performance** (Kestrel is one of the fastest web servers).
- **Unified** MVC + Web API + Razor Pages + minimal APIs + Blazor.
- **Built-in DI, configuration, logging, middleware.**
- **Active development & support** — .NET Framework is in maintenance; new
  features land in modern .NET. Long-term support (LTS) releases every other year.

---

## 2. .NET landscape (clear up the naming)
- **.NET Framework 4.8** — Windows-only, legacy (still supported, not evolving).
- **.NET Core 1–3.1** — the cross-platform rewrite (now superseded).
- **.NET 5, 6, 7, 8, 9 …** — the unified, modern platform (drop the "Core" in the
  runtime name). Even-numbered releases (6, 8) are **LTS**.
- **ASP.NET Core** — the web framework that runs on modern .NET.

---

## 3. Key differences: classic MVC 5 → ASP.NET Core MVC
| Area | ASP.NET MVC 5 | ASP.NET Core MVC |
|------|---------------|------------------|
| Startup | `Global.asax`, `App_Start/*` | `Program.cs` (minimal hosting) |
| Pipeline | `System.Web` HTTP modules/handlers | **Middleware** |
| DI | 3rd-party container | **Built-in** |
| Config | `web.config` (XML) | `appsettings.json` + `IConfiguration` |
| Base class | `Controller` (System.Web.Mvc) | `Controller`/`ControllerBase` (AspNetCore.Mvc) |
| Web API | Separate `ApiController` | Unified with `[ApiController]` |
| Views | HTML helpers | **Tag Helpers** + HTML helpers |
| Hosting | IIS + `System.Web` | Kestrel (+ reverse proxy) |
| Auth | OWIN / Forms / Identity | ASP.NET Core Identity / JWT / OIDC |
| Bundling | `System.Web.Optimization` | build tools / WebOptimizer / bundlers |

---

## 4. The new startup model (`Program.cs`)
```csharp
var builder = WebApplication.CreateBuilder(args);

// Services (DI container)
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Middleware pipeline (order matters!)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

### Middleware mental model
Each middleware can act on the request, call `next()`, then act on the response —
like nested wrappers. **Order matters** (e.g. auth before authorization, routing
before endpoints).

---

## 5. Configuration & options
```csharp
// appsettings.json → strongly-typed options
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

public class EmailSender(IOptions<SmtpOptions> opts) { /* opts.Value.Host ... */ }
```
- Sources merge in order: appsettings → env-specific → env vars → user secrets →
  command line. Later wins.

---

## 6. Logging & DI built in
```csharp
public class OrdersController(IOrderService orders, ILogger<OrdersController> logger)
    : Controller
{
    public async Task<IActionResult> Index()
    {
        logger.LogInformation("Listing orders");
        return View(await orders.GetAllAsync());
    }
}
```
(Primary constructors shown — C# 12.)

---

## 7. Migration strategy
1. **Inventory** dependencies; check which have Core-compatible packages
   (use the .NET Upgrade Assistant / `try-convert`, API Portability Analyzer).
2. **Target a current LTS** (.NET 8).
3. **Move shared logic** to a class library (`net8.0` or `netstandard2.0`) first.
4. **Re-platform the web project**: recreate startup, DI, config; port
   controllers/views; swap HTML helpers for tag helpers as you go.
5. **Replace framework-specific bits:** `System.Web` (HttpContext usage),
   Forms auth → Identity/cookie auth, `web.config` → `appsettings`,
   bundling → build tooling.
6. **Strangler-fig** for large apps: run old & new side by side behind a proxy,
   migrate route-by-route.
7. **Test continuously**; lean on integration tests to catch regressions.

> EF6 → **EF Core** is its own migration: APIs are similar but not identical
> (lazy loading, some LINQ translations, mapping config differ). Plan and test.

---

## 8. Things to learn next on modern .NET
- **Minimal APIs** for lightweight endpoints.
- **Razor Pages** (page-focused alternative to MVC for forms/CRUD).
- **Blazor** (C# UI: Server, WebAssembly, and "Auto").
- **gRPC** for high-performance service-to-service calls.
- **Native AOT**, performance improvements, and source generators.
- **Health checks, rate limiting, output caching** (built-in middleware).

---

## Common pitfalls
- Treating EF6 and EF Core as identical (behavior differs).
- Wrong middleware order (auth/routing).
- Direct `System.Web.HttpContext` dependencies that don't exist in Core.
- Big-bang rewrite instead of incremental/strangler migration.
- Targeting a non-LTS version for a long-lived app.

## Practice / interview questions
1. What is middleware and why does order matter?
2. How does DI differ between MVC 5 and ASP.NET Core?
3. Where did `Global.asax`/`App_Start` go in Core?
4. Tag helpers vs HTML helpers?
5. How does configuration work in ASP.NET Core?
6. Outline a strategy to migrate a large MVC 5 app to Core.
7. EF6 vs EF Core — key differences to watch for.
8. What are the service lifetimes and when do you use each?
