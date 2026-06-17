# Module 02 — The MVC Architecture & Request Lifecycle

## The Three Components in Depth

### Model

The model represents your **domain data and business rules**. It is independent of the UI.

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    public bool IsExpensive() => Price > 1000;     // business logic
}
```

Models can be: domain entities, **ViewModels** (UI-shaped data), or data-access classes.

### View

The view is a **Razor template** (`.cshtml`) that renders the model into HTML. It should contain
presentation logic only — no business rules.

```html
@model Product
<h2>@Model.Name</h2>
<p>Price: @Model.Price.ToString("C")</p>
```

### Controller

The controller is the **traffic cop**: it receives the request, invokes the model, and returns a result.

```csharp
public class ProductController : Controller
{
    public ActionResult Details(int id)
    {
        Product product = _repository.GetById(id);  // talk to model
        return View(product);                        // pass to view
    }
}
```

## The MVC Request Lifecycle (MVC 5)

Understanding the pipeline is a frequent interview topic.

```text
1. Request           Browser sends HTTP request to the server.
2. Routing           UrlRoutingModule matches the URL to a route → produces
                     RouteData (controller, action, parameters).
3. Controller        MvcHandler uses the IControllerFactory to instantiate the
   Initialization    matching controller.
4. Action            ActionInvoker selects the action method; model binding maps
   Execution         request data → action parameters; filters run around it.
5. Result            The action returns an ActionResult (e.g., ViewResult).
   Execution         The result is executed.
6. View Rendering    For a ViewResult, the view engine (Razor) locates and renders
                     the .cshtml into HTML.
7. Response          HTML is written to the HTTP response and sent to the browser.
```

### Where Filters Fit

Filters wrap action and result execution:

```text
Authorization Filter → Action Filter (OnActionExecuting) → ACTION →
Action Filter (OnActionExecuted) → Result Filter (OnResultExecuting) →
RESULT → Result Filter (OnResultExecuted)   (Exception filters catch errors)
```

## Separation of Concerns

| Layer | Should contain | Should NOT contain |
|-------|----------------|--------------------|
| Model | data, business/validation logic | HTML, HttpContext |
| View | HTML, display logic | business rules, DB calls |
| Controller | request handling, orchestration | heavy business logic, HTML |

> **Thin controllers, fat models/services.** Keep controllers small; push logic into services.

## ASP.NET Core MVC Pipeline (modern)

In ASP.NET Core, requests flow through **middleware** before reaching MVC:

```text
Request → Middleware pipeline (auth, static files, routing) → Endpoint (MVC)
       → Controller action → Result → Response
```

`Program.cs` configures services and middleware:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();   // register MVC services

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
```

## The Front Controller Pattern

ASP.NET MVC uses a **front controller** (`MvcHandler` in MVC 5 / routing middleware in Core):
a single entry point processes all requests, applies routing, and dispatches to the right action.

## Key Takeaways

- Model = data + business logic; View = HTML rendering; Controller = request orchestration.
- The lifecycle: **request → routing → controller → action (+filters + model binding) → result → view → response**.
- Filters wrap the action/result pipeline; the view engine renders Razor to HTML.
- Keep controllers thin; ASP.NET Core adds a middleware pipeline in front of MVC.

➡️ Next: [Module 03 — Controllers & Actions](03-controllers-and-actions.md)
