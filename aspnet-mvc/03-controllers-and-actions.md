# Module 03 — Controllers & Actions

## What is a Controller?

A controller is a class that handles incoming requests. By convention it:

- Lives in the `Controllers` folder.
- Is named with the suffix **`Controller`** (e.g., `ProductController`).
- Inherits from `Controller` (MVC 5) / `Controller` or `ControllerBase` (Core).

```csharp
public class ProductController : Controller
{
    public ActionResult Index()
    {
        return View();
    }
}
```

## Action Methods

**Public methods** on a controller are **actions** — they respond to requests. The URL
`/Product/Details/5` maps to `ProductController.Details(5)` by default routing.

```csharp
public class ProductController : Controller
{
    public ActionResult Index()          => View();        // /Product or /Product/Index
    public ActionResult Details(int id)  => View();        // /Product/Details/5
    public ActionResult About()          => View();        // /Product/About
}
```

To prevent a public method from being an action, mark it `[NonAction]`.

## Action Results

Actions return an `ActionResult` (or `IActionResult` in Core). The framework executes it.

| Result | Helper | Purpose |
|--------|--------|---------|
| `ViewResult` | `View()` | render a Razor view |
| `PartialViewResult` | `PartialView()` | render a partial view |
| `RedirectResult` | `Redirect(url)` | 302 redirect to URL |
| `RedirectToRouteResult` | `RedirectToAction()` | redirect to another action |
| `JsonResult` | `Json(obj)` | return JSON |
| `ContentResult` | `Content("text")` | return raw text |
| `FileResult` | `File(...)` | return a file/stream |
| `HttpStatusCodeResult` | `StatusCode()` / `HttpNotFound()` | status codes |
| `EmptyResult` | — | nothing |

```csharp
public ActionResult Examples(int id)
{
    return View();                                  // ViewResult
    return View("CustomViewName", model);           // specific view + model
    return PartialView("_Row", item);               // partial
    return RedirectToAction("Index");               // redirect to action
    return RedirectToAction("Details", new { id }); // with route values
    return Json(new { success = true });            // JSON
    return Content("Plain text");                   // raw content
    return File(bytes, "application/pdf", "doc.pdf"); // download
    return HttpNotFound();                          // 404
    return new HttpStatusCodeResult(403);           // custom status
}
```

In ASP.NET Core, prefer the typed helpers: `Ok()`, `NotFound()`, `BadRequest()`, `Created()`.

## Receiving Input (Parameters)

Action parameters are populated by **model binding** from route values, query string, and form data.

```csharp
// /Product/Search?term=phone&page=2
public ActionResult Search(string term, int page = 1) { ... }

// Bind a whole object from form fields
[HttpPost]
public ActionResult Create(Product product) { ... }

// Explicit sources (Core)
public IActionResult Get([FromRoute] int id, [FromQuery] string sort) { ... }
```

## HTTP Verb Attributes

Restrict which HTTP method an action responds to.

```csharp
[HttpGet]                     // shows the form
public ActionResult Create() => View();

[HttpPost]                    // handles submission
[ValidateAntiForgeryToken]    // CSRF protection
public ActionResult Create(Product product)
{
    if (!ModelState.IsValid) return View(product);
    _repo.Add(product);
    return RedirectToAction("Index");
}
```

Other verbs: `[HttpPut]`, `[HttpDelete]`, `[HttpPatch]`. Use `[ActionName("name")]` to change the
action's route name.

## Passing Data to Views

```csharp
// 1. Strongly typed model (preferred)
return View(product);

// 2. ViewBag — dynamic, no compile-time checking
ViewBag.Title = "Products";
ViewBag.Count = 10;

// 3. ViewData — dictionary, string keys
ViewData["Message"] = "Hello";

// 4. TempData — survives one redirect (uses Session)
TempData["Notice"] = "Saved successfully";
return RedirectToAction("Index");
```

| Mechanism | Lifetime | Typed? |
|-----------|----------|--------|
| Model | current request | yes |
| ViewBag | current request | no (dynamic) |
| ViewData | current request | no (object) |
| TempData | current + next request | no |

## The POST-Redirect-GET Pattern

After a successful POST, **redirect** instead of returning a view directly. This prevents
duplicate form submissions on refresh.

```csharp
[HttpPost]
public ActionResult Create(Product p)
{
    if (!ModelState.IsValid) return View(p);
    _repo.Add(p);
    TempData["Message"] = "Created!";
    return RedirectToAction("Index");   // PRG
}
```

## Controller Base Members

```csharp
Request    // incoming HttpRequest
Response   // outgoing HttpResponse
RouteData  // route values
ModelState // validation state
User       // current principal (authentication)
Url        // URL helper
Server     // server utilities (MVC 5)
```

## Key Takeaways

- Controllers handle requests; public methods are actions mapped from the URL.
- Actions return `ActionResult`/`IActionResult` — `View`, `Redirect`, `Json`, `File`, status codes.
- Use HTTP verb attributes and `[ValidateAntiForgeryToken]` to secure POST actions.
- Prefer strongly typed models; use TempData with the POST-Redirect-GET pattern.

➡️ Next: [Module 04 — Views & Razor](04-views-and-razor.md)
