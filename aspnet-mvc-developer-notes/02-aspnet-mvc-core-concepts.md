# 02 — ASP.NET MVC Core Concepts

This is the framework you use every day. Knowing the **request lifecycle** and
each extension point is what separates a junior from a senior MVC developer.

> Notes apply to **ASP.NET MVC 5** (classic) with **Core differences** called out.

---

## 1. The MVC pattern
- **Model** — data + business logic / domain.
- **View** — presentation (Razor `.cshtml`).
- **Controller** — handles requests, coordinates model & view, returns a result.

Goal: separation of concerns, testability, parallel work.

> In practice you'll also use **ViewModels** (a shape tailored to a specific
> view) instead of passing domain/EF entities straight to views. This avoids
> over-posting and leaks of data.

---

## 2. Request lifecycle (know this cold)

```
Incoming HTTP request
  → Routing (match URL to a route → controller + action)
  → Controller instantiation (DI / DefaultControllerFactory)
  → Authentication / Authorization filters
  → Action filters (OnActionExecuting)
  → Model binding + validation (build action params, populate ModelState)
  → Action method executes
  → Action filters (OnActionExecuted)
  → Action returns an ActionResult
  → Result filters (OnResultExecuting)
  → Result execution (e.g. ViewResult renders Razor → HTML)
  → Result filters (OnResultExecuted)
  → Response written to client
(Exception filters run if an exception is thrown along the way)
```

**Classic vs Core:** Core replaces the `System.Web` pipeline with
**middleware**. Routing, auth, static files, etc. are middleware components
registered in `Program.cs`/`Startup.cs`. The MVC filter pipeline still exists
inside the MVC middleware.

---

## 3. Routing
### Convention-based routing (classic, `RouteConfig.cs`)
```csharp
routes.MapRoute(
    name: "Default",
    url: "{controller}/{action}/{id}",
    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });
```

### Attribute routing
```csharp
[RoutePrefix("products")]
public class ProductsController : Controller
{
    [Route("{id:int}")]
    public ActionResult Details(int id) { ... }
}
```

**Core:** `app.MapControllerRoute(...)` and `[Route]`/`[HttpGet("...")]`
attributes; endpoint routing is the default.

Know: route constraints (`{id:int}`), defaults, optional params, ordering, and
how ambiguous routes are resolved.

---

## 4. Controllers & action results
- Controllers derive from `Controller` (or `ControllerBase` in Core APIs).
- Actions return an `ActionResult` (`IActionResult` in Core).

Common results:
| Result | Helper | Use |
|--------|--------|-----|
| `ViewResult` | `View()` | Render a Razor view |
| `PartialViewResult` | `PartialView()` | Render a partial |
| `RedirectResult` | `Redirect()` | 302 redirect |
| `RedirectToRouteResult` | `RedirectToAction()` | Redirect by route |
| `JsonResult` | `Json()` | Return JSON |
| `ContentResult` | `Content()` | Raw text |
| `FileResult` | `File()` | Download/stream a file |
| `HttpStatusCodeResult` | `StatusCode()` / `NotFound()` | Status codes |

### PRG pattern (Post-Redirect-Get)
After a successful POST, **redirect** to a GET to avoid duplicate form
submissions on refresh.

```csharp
[HttpPost, ValidateAntiForgeryToken]
public ActionResult Create(OrderVm vm)
{
    if (!ModelState.IsValid) return View(vm);
    var id = _service.Create(vm);
    return RedirectToAction(nameof(Details), new { id });   // PRG
}
```

---

## 5. Model binding & validation
- **Model binding** maps request data (route, query string, form, body) to
  action parameters / model properties by name.
- **Validation** via data annotations populates `ModelState`.

```csharp
public class RegisterVm
{
    [Required, StringLength(50)]
    public string UserName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required, DataType(DataType.Password), MinLength(8)]
    public string Password { get; set; }
}

[HttpPost, ValidateAntiForgeryToken]
public ActionResult Register(RegisterVm vm)
{
    if (!ModelState.IsValid) return View(vm);
    // ...
}
```

### Over-posting / mass assignment
Never bind directly to EF entities for create/update forms — an attacker can set
fields you didn't intend (e.g. `IsAdmin`). Use **ViewModels** or `[Bind]` /
`TryUpdateModel` include lists.

Custom validation: `IValidatableObject`, custom `ValidationAttribute`, remote
validation.

---

## 6. Views & Razor (see file 05 for depth)
- Razor syntax: `@Model`, `@foreach`, `@if`, `@{ }`, `@: `.
- Strongly-typed views: `@model RegisterVm`.
- Layouts (`_Layout.cshtml`), sections (`@RenderSection`), partials
  (`@Html.Partial` / `@await Html.PartialAsync`), and **view components**
  (Core) / **child actions** (classic) for reusable, logic-backed widgets.
- HTML helpers (`@Html.TextBoxFor`) vs **Tag Helpers** (Core, `asp-for`).
- `_ViewStart.cshtml` and `_ViewImports.cshtml` (Core).

---

## 7. Filters (a senior topic)
Cross-cutting concerns without cluttering actions. Types & order:
1. **Authorization filters** — `[Authorize]`, run first.
2. **Action filters** — `OnActionExecuting/Executed`. Logging, timing.
3. **Result filters** — around result execution.
4. **Exception filters** — `[HandleError]` / `IExceptionFilter`.

```csharp
public class LogActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext ctx)
        => Debug.WriteLine($"Entering {ctx.ActionDescriptor.ActionName}");
}
```

Register globally (`GlobalFilters.Filters.Add(...)` classic; `options.Filters`
in Core), per-controller, or per-action.

---

## 8. State management
| Mechanism | Scope | Notes |
|-----------|-------|-------|
| `ViewData` / `ViewBag` | single request, controller→view | `ViewBag` is dynamic wrapper over `ViewData` |
| `TempData` | next request only | Backed by session; great for PRG messages |
| `Session` | per user, server-side | Avoid heavy use; scaling needs distributed session |
| Cookies | client | Sign/encrypt sensitive data; size limits |
| Hidden fields / query string | per request | Visible/editable by client |
| Cache | app-wide | See file 10 |

> HTTP is stateless. Choose the **smallest** scope that solves the problem.

---

## 9. Dependency Injection
- **Core** has built-in DI: register services in `Program.cs`
  (`builder.Services.AddScoped<IFoo, Foo>()`), inject via constructors.
- **Classic MVC** needs a container (Autofac, Unity, Ninject, SimpleInjector)
  wired to `DependencyResolver` / a custom controller factory.

Lifetimes (Core): **Transient**, **Scoped** (per request — typical for
`DbContext`/repos), **Singleton**.

```csharp
// Core
builder.Services.AddScoped<IOrderService, OrderService>();

public class OrdersController : Controller
{
    private readonly IOrderService _orders;
    public OrdersController(IOrderService orders) => _orders = orders;
}
```

---

## 10. Areas, bundling, configuration
- **Areas** — partition large apps (e.g. `Admin`, `Shop`) into modules.
- **Bundling & minification** — classic uses `System.Web.Optimization`
  (`BundleConfig`); Core uses build-time tooling / `LibMan` / `WebOptimizer` or
  a JS bundler.
- **Configuration** — classic `web.config` (`appSettings`, `connectionStrings`,
  transforms per environment); Core uses `appsettings.json` +
  `IConfiguration`/Options pattern + environment variables + user secrets.

---

## 11. Error handling
- `customErrors`/`httpErrors` in `web.config` (classic).
- Global exception filter / `Application_Error` in `Global.asax` (classic).
- Core: `UseExceptionHandler`, `UseDeveloperExceptionPage`, status code pages.
- Always log; show friendly pages to users, details only in dev.

---

## Common pitfalls
- Passing EF entities directly to views (over-posting, lazy-load in view).
- Doing heavy work / DB calls in the view.
- Forgetting `[ValidateAntiForgeryToken]` on POSTs.
- Not checking `ModelState.IsValid`.
- Using `Session` as a dumping ground.
- Fat controllers — push logic into services.

## Practice / interview questions
1. Walk through the full MVC request lifecycle.
2. Difference between `ViewData`, `ViewBag`, and `TempData`?
3. What order do the filter types run in?
4. What is over-posting and how do you prevent it?
5. Why use the PRG pattern?
6. How does model binding decide where to read a value from?
7. How would you add DI to a classic ASP.NET MVC 5 app?
8. Convention-based vs attribute routing — pros/cons?
