# Module 09 — State Management

HTTP is **stateless** — each request is independent. State management techniques let you persist
data across requests. ASP.NET MVC offers several options, each with a different scope and lifetime.

## Overview

| Technique | Stored | Scope | Lifetime | Typical use |
|-----------|--------|-------|----------|-------------|
| **ViewData** | server (request) | one action→view | single request | pass data to a view |
| **ViewBag** | server (request) | one action→view | single request | same as ViewData (dynamic) |
| **TempData** | server (Session/cookie) | across redirect | next request only | PRG messages |
| **Session** | server | per user | configurable timeout | user-specific data |
| **Cookies** | client (browser) | per browser | until expiry | preferences, tokens |
| **Cache** | server | application-wide | configurable | shared, expensive data |
| **Application/Static** | server | application-wide | app lifetime | global counters/config |
| **Hidden fields/Query string** | client | per request | round trip | small values in forms/links |

## ViewData & ViewBag (single request)

```csharp
public ActionResult Index()
{
    ViewData["Message"] = "Hello";      // dictionary (object, needs casting)
    ViewBag.Count = 10;                 // dynamic wrapper over ViewData
    return View();
}
```

```html
<p>@ViewData["Message"] — @ViewBag.Count</p>
```

- Both live for the **current request only** and are gone after the view renders.
- `ViewBag` is a `dynamic` wrapper over `ViewData` — same storage, nicer syntax, no compile checks.

## TempData (survives one redirect)

Backed by Session (or cookies in Core). Perfect for messages after a **POST-Redirect-GET**.

```csharp
[HttpPost]
public ActionResult Save(Product p)
{
    _repo.Add(p);
    TempData["Success"] = "Product saved!";
    return RedirectToAction("Index");   // value survives the redirect
}

public ActionResult Index()
{
    var msg = TempData["Success"];      // read once; then removed
    return View();
}
```

- Values are removed after being **read**. Use `TempData.Keep()` or `TempData.Peek("key")` to retain.

## Session (per-user, server-side)

```csharp
// MVC 5
Session["CartItemCount"] = 3;
int count = (int)(Session["CartItemCount"] ?? 0);
Session.Remove("CartItemCount");
Session.Clear();
```

```csharp
// ASP.NET Core — enable in Program.cs
builder.Services.AddSession();
app.UseSession();

// Usage (string/int helpers; complex types need serialization)
HttpContext.Session.SetInt32("CartCount", 3);
int? count = HttpContext.Session.GetInt32("CartCount");
HttpContext.Session.SetString("User", "Alice");
```

- Session uses a cookie (the session ID) to associate the browser with server-side data.
- Drawbacks: consumes server memory, complicates load-balancing (use a distributed cache for
  scale-out, e.g. Redis/SQL Server session state). Avoid storing large objects.

## Cookies (client-side)

```csharp
// MVC 5
var cookie = new HttpCookie("Theme", "Dark") { Expires = DateTime.Now.AddDays(30) };
Response.Cookies.Add(cookie);
string theme = Request.Cookies["Theme"]?.Value;
```

```csharp
// ASP.NET Core
Response.Cookies.Append("Theme", "Dark", new CookieOptions
{
    Expires = DateTimeOffset.Now.AddDays(30),
    HttpOnly = true,        // not accessible to JS (XSS protection)
    Secure = true,          // HTTPS only
    SameSite = SameSiteMode.Strict
});
string? theme = Request.Cookies["Theme"];
```

- **Never store sensitive data** in cookies unencrypted. Use `HttpOnly` + `Secure` flags.

## Caching (server-side, shared)

```csharp
// MVC 5 — HttpContext.Cache / MemoryCache
HttpRuntime.Cache.Insert("products", products, null,
    DateTime.Now.AddMinutes(10), System.Web.Caching.Cache.NoSlidingExpiration);

// ASP.NET Core — IMemoryCache (inject it)
public class ProductController : Controller
{
    private readonly IMemoryCache _cache;
    public ProductController(IMemoryCache cache) => _cache = cache;

    public IActionResult Index()
    {
        var products = _cache.GetOrCreate("products", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return _repo.GetAll();
        });
        return View(products);
    }
}
```

### Output Caching

Cache the rendered output of an action:

```csharp
// MVC 5
[OutputCache(Duration = 60, VaryByParam = "id")]
public ActionResult Details(int id) => View();
```

In ASP.NET Core, use Response Caching / Output Caching middleware.

## Query Strings & Hidden Fields

```csharp
// Query string — visible, bookmarkable, limited size
return RedirectToAction("List", new { page = 2, sort = "name" });
```

```html
@Html.HiddenFor(m => m.Id)   <!-- round-trips a value through a form -->
```

## Choosing the Right Technique

- Need data **only in the current view**? → ViewBag/ViewData/Model.
- Need a **message after a redirect**? → TempData.
- Need **per-user data across requests**? → Session (sparingly).
- Need **client preferences**? → Cookies.
- Need **shared, expensive data**? → Cache.

## Key Takeaways

- HTTP is stateless; pick the technique matching the needed **scope** and **lifetime**.
- ViewData/ViewBag = one request; TempData = across one redirect; Session = per user.
- Cookies live on the client — secure them (`HttpOnly`, `Secure`) and never store secrets.
- Use caching for expensive shared data; mind memory and load-balancing implications of Session.

➡️ Next: [Module 10 — Filters](10-filters.md)
