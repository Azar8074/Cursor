# Module 06 — Routing

**Routing** maps incoming request URLs to controller actions (and builds URLs from route data).
It's what gives MVC apps clean, SEO-friendly URLs.

## Convention-Based Routing

Routes are registered at startup. The default route matches `/{controller}/{action}/{id}`.

### MVC 5 (`App_Start/RouteConfig.cs`)

```csharp
public class RouteConfig
{
    public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

        routes.MapRoute(
            name: "Default",
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
        );
    }
}
```

### ASP.NET Core (`Program.cs`)

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

### How matching works

| URL | Controller | Action | id |
|-----|-----------|--------|----|
| `/` | Home | Index | — |
| `/Product` | Product | Index | — |
| `/Product/Details` | Product | Details | — |
| `/Product/Details/5` | Product | Details | 5 |

## Custom Routes

Define specific routes **before** the default (order matters — first match wins).

```csharp
routes.MapRoute(
    name: "Blog",
    url: "blog/{year}/{month}/{slug}",
    defaults: new { controller = "Blog", action = "Post" }
);
// matches /blog/2026/06/hello-world → Blog.Post(year, month, slug)
```

## Route Constraints

Restrict what a segment can match.

```csharp
// MVC 5
routes.MapRoute(
    name: "ProductById",
    url: "product/{id}",
    defaults: new { controller = "Product", action = "Details" },
    constraints: new { id = @"\d+" }   // id must be numeric
);
```

```csharp
// Core inline constraints
[Route("product/{id:int}")]              // int only
[Route("user/{name:alpha}")]             // letters only
[Route("item/{id:int:min(1)}")]          // int >= 1
[Route("date/{d:datetime}")]
[Route("p/{slug:regex(^[a-z0-9-]+$)}")]
```

Common constraints: `int`, `bool`, `datetime`, `decimal`, `guid`, `alpha`, `min(n)`, `max(n)`,
`minlength(n)`, `maxlength(n)`, `length(n)`, `range(a,b)`, `regex(...)`.

## Attribute Routing

Define routes directly on controllers/actions — great for APIs and fine-grained control.

```csharp
// Enable in MVC 5: routes.MapMvcAttributeRoutes(); (Core is on by default)

[RoutePrefix("api/products")]            // MVC 5 (Core: [Route("api/products")])
public class ProductsController : Controller
{
    [HttpGet]
    [Route("")]                          // GET /api/products
    public ActionResult GetAll() => ...;

    [HttpGet]
    [Route("{id:int}")]                  // GET /api/products/5
    public ActionResult Get(int id) => ...;

    [HttpGet]
    [Route("category/{name}")]           // GET /api/products/category/books
    public ActionResult ByCategory(string name) => ...;
}
```

### Core attribute routing with tokens

```csharp
[Route("api/[controller]")]              // [controller] → "Products"
public class ProductsController : ControllerBase
{
    [HttpGet("{id:int}")]                // GET /api/products/5
    public IActionResult Get(int id) => Ok();

    [HttpPost]                           // POST /api/products
    public IActionResult Create(Product p) => Created("", p);
}
```

## Optional & Default Parameters

```csharp
[Route("products/{category}/{page:int?}")]   // page optional
public ActionResult List(string category, int page = 1) { ... }
```

## Generating URLs (don't hard-code them)

```csharp
// In controllers
string url = Url.Action("Details", "Product", new { id = 5 });
return RedirectToAction("Index", "Home");

// In Razor views
@Url.Action("Edit", "Product", new { id = Model.Id })
@Html.ActionLink("Edit", "Edit", "Product", new { id = Model.Id }, null)

// Core Tag Helper
<a asp-controller="Product" asp-action="Edit" asp-route-id="@Model.Id">Edit</a>

// Named route
[Route("products/{id}", Name = "ProductDetails")]
@Url.RouteUrl("ProductDetails", new { id = 5 })
```

> Using URL generation keeps links correct if routes change — never hard-code paths.

## Areas

Areas partition a large app into modules, each with its own controllers/views/routes (e.g., an
Admin area). See Module 13.

```csharp
[Area("Admin")]
public class DashboardController : Controller { }
// URL: /Admin/Dashboard
```

## Convention vs Attribute Routing — When?

- **Convention-based**: consistent, app-wide URL patterns; fewer attributes.
- **Attribute**: precise, per-action control; ideal for **Web APIs** and irregular URLs.
- You can mix both.

## Key Takeaways

- Routing maps URLs → actions; the default pattern is `{controller}/{action}/{id?}`.
- Specific routes must be registered before general ones (first match wins).
- Use **constraints** to validate segments and **attribute routing** for precise/API routes.
- Always **generate** URLs via `Url.Action`/Tag Helpers rather than hard-coding them.

➡️ Next: [Module 07 — Validation & Data Annotations](07-validation-and-data-annotations.md)
