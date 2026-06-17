# Module 01 — Introduction to ASP.NET MVC

## What is ASP.NET MVC?

**ASP.NET MVC** is a web application framework from Microsoft that implements the
**Model-View-Controller** architectural pattern. It gives you full control over HTML, clean
separation of concerns, and excellent testability — an alternative to the older Web Forms model.

> **Note on flavors:** "ASP.NET MVC 5" runs on the Windows-only **.NET Framework**.
> "**ASP.NET Core MVC**" is the modern, cross-platform successor. The MVC *concepts* (controllers,
> views, routing, model binding) are the same; this course highlights differences where relevant.

## The MVC Pattern

| Component | Responsibility |
|-----------|----------------|
| **Model** | The data and business logic (e.g., `Product`, validation, persistence). |
| **View** | The UI — renders the model as HTML (Razor templates). |
| **Controller** | Handles requests, coordinates the model, selects a view to return. |

The flow: **Request → Routing → Controller → Model → View → Response**.

```text
Browser ──HTTP request──► Routing ──► Controller action
                                          │ uses
                                          ▼
                                        Model (data/business logic)
                                          │ passes data to
                                          ▼
                                        View (Razor) ──HTML──► Browser
```

## Why MVC? Key Benefits

- **Separation of concerns** — UI, logic, and data are decoupled, easier to maintain.
- **Testability** — controllers are plain classes; easy to unit test without a web server.
- **Full control over HTML** — no auto-generated markup or ViewState (unlike Web Forms).
- **Clean URLs & routing** — SEO-friendly, RESTful URLs.
- **Convention over configuration** — predictable folder structure reduces setup.
- **Extensible** — swap view engines, model binders, filters, DI containers.

## ASP.NET MVC vs Web Forms

| Feature | MVC | Web Forms |
|---------|-----|-----------|
| Pattern | MVC | Page Controller / event-driven |
| State | Stateless (HTTP) | ViewState, server controls |
| HTML control | Full | Abstracted |
| Testability | High | Low |
| URLs | Routing-based, clean | `.aspx` file-based |
| Reusability | Partial views, components | User/server controls |

## Typical Project Structure (MVC 5)

```text
MyApp/
├── App_Start/
│   ├── RouteConfig.cs        # route definitions
│   ├── BundleConfig.cs       # CSS/JS bundling
│   └── FilterConfig.cs       # global filters
├── Controllers/
│   └── HomeController.cs
├── Models/
│   └── Product.cs
├── Views/
│   ├── Home/
│   │   └── Index.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml    # master layout
│   │   └── _ViewStart.cshtml
│   └── web.config
├── Content/                  # CSS, images
├── Scripts/                  # JavaScript
├── Global.asax               # application startup
└── Web.config                # configuration
```

In **ASP.NET Core MVC** the structure is `Program.cs` (startup), `Controllers/`, `Models/`,
`Views/`, `wwwroot/` (static files), and `appsettings.json` (config).

## Creating a Project

```bash
# ASP.NET Core MVC (cross-platform)
dotnet new mvc -o MyApp
cd MyApp
dotnet run
```

In Visual Studio (MVC 5): **File → New → Project → ASP.NET Web Application → MVC**.

## A First Controller and View

```csharp
// Controllers/HomeController.cs
public class HomeController : Controller
{
    public ActionResult Index()
    {
        ViewBag.Message = "Welcome to ASP.NET MVC!";
        return View();
    }
}
```

```html
<!-- Views/Home/Index.cshtml -->
@{
    ViewBag.Title = "Home";
}
<h1>@ViewBag.Message</h1>
```

Navigating to `/` (or `/Home/Index`) runs `Index()` and renders `Index.cshtml`.

## Key Takeaways

- ASP.NET MVC implements the Model-View-Controller pattern for web apps.
- Benefits: separation of concerns, testability, full HTML control, clean routing.
- MVC 5 runs on .NET Framework; ASP.NET Core MVC is the modern cross-platform version.
- A request flows: routing → controller action → model → view → HTML response.

➡️ Next: [Module 02 — The MVC Architecture](02-mvc-architecture.md)
