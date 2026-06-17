# Module 13 — Bundling, Minification & Areas

## Part A — Bundling & Minification (Performance)

Loading many separate CSS/JS files means many HTTP requests, slowing page loads.

- **Bundling**: combine multiple files into one request.
- **Minification**: strip whitespace, comments, and shorten names to reduce file size.

### MVC 5 — System.Web.Optimization

```csharp
// App_Start/BundleConfig.cs
public class BundleConfig
{
    public static void RegisterBundles(BundleCollection bundles)
    {
        bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            "~/Scripts/jquery-{version}.js"));

        bundles.Add(new ScriptBundle("~/bundles/app").Include(
            "~/Scripts/site.js",
            "~/Scripts/widgets.js"));

        bundles.Add(new StyleBundle("~/Content/css").Include(
            "~/Content/bootstrap.css",
            "~/Content/site.css"));

        BundleTable.EnableOptimizations = true;   // force bundling even in debug
    }
}
```

Render in a view/layout:

```html
@Styles.Render("~/Content/css")
@Scripts.Render("~/bundles/jquery")
@Scripts.Render("~/bundles/app")
```

> In **Debug** mode files are rendered individually (easier debugging); in **Release** they're
> bundled and minified automatically — controlled by `<compilation debug="true|false">` in Web.config.

### ASP.NET Core — different approach

Core doesn't ship the old bundling framework. Instead use:

- **Build-time bundlers**: Webpack, Vite, esbuild, or the `BundlerMinifier`/`WebOptimizer` packages.
- **`environment` Tag Helper** to switch between dev and prod assets:

```html
<environment include="Development">
    <link rel="stylesheet" href="~/css/site.css" />
</environment>
<environment exclude="Development">
    <link rel="stylesheet" href="~/css/site.min.css" asp-append-version="true" />
</environment>
```

- `asp-append-version="true"` adds a content hash for **cache busting**.

## Part B — Areas (Modular Apps)

**Areas** partition a large application into smaller functional units, each with its own
Controllers, Views, and routes. Classic example: an **Admin** area separate from the public site.

### Structure

```text
Areas/
└── Admin/
    ├── Controllers/
    │   └── DashboardController.cs
    ├── Views/
    │   └── Dashboard/
    │       └── Index.cshtml
    └── AdminAreaRegistration.cs   (MVC 5)
```

### MVC 5 — Area registration

```csharp
public class AdminAreaRegistration : AreaRegistration
{
    public override string AreaName => "Admin";

    public override void RegisterArea(AreaRegistrationContext context)
    {
        context.MapRoute(
            "Admin_default",
            "Admin/{controller}/{action}/{id}",
            new { action = "Index", id = UrlParameter.Optional }
        );
    }
}
```

### ASP.NET Core — Areas

```csharp
[Area("Admin")]
public class DashboardController : Controller
{
    public IActionResult Index() => View();   // URL: /Admin/Dashboard
}
```

Register an area route:

```csharp
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
```

### Linking across areas

```html
<!-- Tag Helper -->
<a asp-area="Admin" asp-controller="Dashboard" asp-action="Index">Admin</a>

<!-- HTML Helper (MVC 5) -->
@Html.ActionLink("Admin", "Index", "Dashboard", new { area = "Admin" }, null)

<!-- Leave the current area -->
<a asp-area="" asp-controller="Home" asp-action="Index">Public Site</a>
```

### When to use Areas

- Large applications with **distinct modules** (Admin, Customer portal, Reporting).
- Teams working on separate sections.
- When you want logical/folder separation without splitting into multiple projects.

## Key Takeaways

- **Bundling + minification** reduce request count and payload size for faster page loads.
- MVC 5 uses `System.Web.Optimization`; Core uses build tools and the `environment` Tag Helper
  with `asp-append-version` for cache busting.
- **Areas** modularize large apps into self-contained sections with their own routes/views.
- Use the `{area:exists}` route and `asp-area` helpers to route and link across areas.

➡️ Next: [Module 14 — Dependency Injection](14-dependency-injection.md)
