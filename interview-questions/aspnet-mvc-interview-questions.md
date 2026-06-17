# ASP.NET MVC Interview Questions & Answers

A comprehensive bank of ASP.NET MVC interview questions, from fundamentals to advanced. Answers
apply to ASP.NET MVC 5 and note ASP.NET Core MVC differences where relevant.

## Table of Contents

1. [MVC Fundamentals](#mvc-fundamentals)
2. [Controllers & Actions](#controllers--actions)
3. [Views & Razor](#views--razor)
4. [Models & Model Binding](#models--model-binding)
5. [Routing](#routing)
6. [Validation](#validation)
7. [State Management](#state-management)
8. [Filters](#filters)
9. [Security](#security)
10. [Entity Framework & Data](#entity-framework--data)
11. [Web API & AJAX](#web-api--ajax)
12. [Advanced & Core](#advanced--core)

---

## MVC Fundamentals

**1. What is ASP.NET MVC?**
A web framework implementing the Model-View-Controller pattern, giving separation of concerns,
full HTML control, testability, and clean routing.

**2. What is the MVC pattern?**
**Model** (data/business logic), **View** (UI/presentation), **Controller** (handles requests,
coordinates model and view). It separates concerns for maintainability and testability.

**3. What are the advantages of MVC over Web Forms?**
Separation of concerns, testability, full control over HTML (no ViewState), clean RESTful URLs,
better support for parallel development, and lightweight, stateless design.

**4. Explain the MVC request lifecycle.**
Request → **Routing** (URL matched to route, produces RouteData) → **Controller creation** (via
controller factory) → **Action execution** (model binding + filters) → **Result execution** →
**View rendering** (Razor engine) → **Response**.

**5. What is the role of the Controller?**
Handle incoming requests, invoke business/model logic, and return an appropriate `ActionResult`
(usually a view, redirect, or data).

**6. What is a ViewModel and why use it?**
A class shaped specifically for a view. Benefits: avoids exposing domain entities, prevents
over-posting, supports UI-only fields, and decouples UI from the domain.

**7. What is the difference between MVC 5 and ASP.NET Core MVC?**
MVC 5 runs on the Windows-only .NET Framework with `System.Web`. ASP.NET Core MVC is
cross-platform, has built-in DI, a middleware pipeline, unified MVC+Web API, `Program.cs`
startup, `appsettings.json`, Tag Helpers, and `wwwroot` for static files.

**8. What is the difference between the three projects: Model, View, Controller folders?**
Conventions: `Models/` holds data classes, `Views/{Controller}/` holds Razor templates,
`Controllers/` holds controller classes. Convention over configuration locates views automatically.

**9. What design patterns are used in ASP.NET MVC?**
MVC, Front Controller, Factory (controller factory), Dependency Injection/IoC, Repository/Unit of
Work (data layer), and Decorator (filters).

---

## Controllers & Actions

**10. What is an action method?**
A public method on a controller that handles a request and returns an `ActionResult`.

**11. What is an `ActionResult`? Name some types.**
The base return type of actions. Types: `ViewResult`, `PartialViewResult`, `RedirectResult`,
`RedirectToRouteResult`, `JsonResult`, `ContentResult`, `FileResult`, `HttpStatusCodeResult`,
`EmptyResult`.

**12. What is the difference between `ViewResult` and `PartialViewResult`?**
`ViewResult` renders a full view (with layout); `PartialViewResult` renders a fragment without a
layout — typically for AJAX updates.

**13. What is the difference between `RedirectToAction` and `RedirectResult`?**
`RedirectToAction` redirects to another action (builds the URL from routing);
`RedirectResult`/`Redirect` redirects to a literal URL.

**14. What is the difference between `ViewBag`, `ViewData`, and `TempData`?**
`ViewData` is a dictionary (string keys, object values), request-scoped. `ViewBag` is a dynamic
wrapper over ViewData. `TempData` persists for the next request (survives one redirect), backed
by Session/cookies.

**15. How do you restrict an action to a specific HTTP verb?**
Use attributes: `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`.

**16. What is `[NonAction]`?**
Marks a public method so it is NOT treated as an action method.

**17. What is `[ActionName]`?**
Changes the action's name used in routing, decoupling it from the method name.

**18. What is the POST-Redirect-GET pattern?**
After a successful POST, redirect to a GET action so refreshing the page doesn't resubmit the
form. Often combined with TempData for status messages.

**19. Can an action method return multiple result types?**
Yes — its return type is `ActionResult`/`IActionResult`, which can be any concrete result based
on logic.

**20. What is the difference between `Controller` and `ControllerBase` (Core)?**
`ControllerBase` has no view support — used for APIs. `Controller` extends it with view-related
methods (`View()`, `PartialView()`, ViewBag/ViewData).

---

## Views & Razor

**21. What is Razor?**
A view engine/templating syntax that mixes C# with HTML using `@`. Output is HTML-encoded by
default.

**22. What is the difference between `@Html.Partial` and `@Html.RenderPartial`?**
`Partial` returns an `MvcHtmlString` (usable in expressions); `RenderPartial` writes directly to
the response stream (slightly faster, returns void — call in a code block).

**23. What is the difference between `@Html.Partial` and `@Html.Action`?**
`Partial` renders a partial view directly. `Action` invokes a child action (controller logic
runs), then renders its result — use when the partial needs its own data/logic.

**24. What is a layout page?**
A master template (`_Layout.cshtml`) defining the common shell (header/footer/nav). Views inject
content via `@RenderBody()` and `@RenderSection()`.

**25. What is `_ViewStart.cshtml`?**
A file that runs before each view to set common settings (typically the default `Layout`).

**26. What is `_ViewImports.cshtml` (Core)?**
Holds shared directives (`@using`, `@addTagHelper`, `@inject`) applied to all views.

**27. What are sections in Razor?**
Named placeholders (`@section scripts { }`) a view fills, rendered in the layout with
`@RenderSection("scripts", required: false)`.

**28. What are HTML Helpers?**
Methods that generate HTML bound to the model: `@Html.TextBoxFor`, `@Html.LabelFor`,
`@Html.DropDownListFor`, `@Html.ValidationMessageFor`, `@Html.BeginForm`.

**29. What are Tag Helpers (Core)?**
Server-side components that attach to HTML elements via `asp-*` attributes (e.g., `asp-for`,
`asp-action`), giving cleaner, HTML-like markup than HTML Helpers.

**30. How does Razor prevent XSS?**
It HTML-encodes output by default. Use `@Html.Raw()` only for trusted HTML.

**31. What is the difference between `@Html.DisplayFor` and `@Html.EditorFor`?**
`DisplayFor` renders read-only display templates; `EditorFor` renders editable input templates
based on the model's type/metadata.

**32. What is a strongly typed view?**
A view that declares `@model SomeType`, getting compile-time checking and IntelliSense for the
model.

---

## Models & Model Binding

**33. What is model binding?**
The automatic mapping of request data (route values, query string, form fields, JSON body) to
action parameters/model properties by name.

**34. What are the default model binding sources and their precedence?**
Form values, route data, query string (and body for JSON in APIs). In Core you can be explicit:
`[FromRoute]`, `[FromQuery]`, `[FromForm]`, `[FromBody]`, `[FromHeader]`.

**35. What is over-posting (mass assignment) and how do you prevent it?**
A user submits extra fields to set properties you didn't intend. Prevent it with ViewModels
containing only editable fields, or `[Bind(Include=...)]`/`[Bind(Exclude=...)]`.

**36. What is `ModelState`?**
An object holding the results of model binding and validation (`ModelState.IsValid`, errors).

**37. How do you add a custom error to ModelState?**
`ModelState.AddModelError("PropertyName", "Error message");`.

**38. What is a custom model binder?**
A class implementing `IModelBinder` for non-standard binding (e.g., custom formats or composite
keys).

**39. Can model binding handle complex types and collections?**
Yes — nested objects (`Address.City`) and collections (`Items[0].Name`) bind by naming convention.

---

## Routing

**40. What is routing in MVC?**
The system that maps request URLs to controller actions and generates URLs from route data.

**41. What is the default route?**
`{controller}/{action}/{id?}` with defaults `Home`/`Index` and optional `id`.

**42. What is the difference between convention-based and attribute routing?**
Convention-based defines patterns centrally (`MapRoute`/`MapControllerRoute`). Attribute routing
places `[Route]` on controllers/actions for precise per-action control (great for APIs).

**43. What are route constraints?**
Rules restricting what a segment matches: `{id:int}`, `{name:alpha}`, `{id:int:min(1)}`,
regex, etc.

**44. How do you create a custom route?**
Register it before the default route with `MapRoute`/`MapControllerRoute`, specifying URL pattern
and defaults. Order matters — first match wins.

**45. How do you generate URLs instead of hard-coding them?**
`Url.Action`, `Html.ActionLink`, `Url.RouteUrl`, or Tag Helpers (`asp-action`/`asp-controller`).

**46. What are Areas?**
A way to partition a large app into modules (e.g., Admin), each with its own
controllers/views/routes.

**47. What is `RouteData`?**
The collection of values extracted from the matched route (controller, action, parameters).

**48. What is attribute routing token replacement (Core)?**
`[controller]` and `[action]` tokens auto-substitute names: `[Route("api/[controller]")]`.

---

## Validation

**49. How does validation work in MVC?**
Data Annotation attributes on models define rules; the framework validates during model binding
and populates `ModelState`. Unobtrusive jQuery validation provides client-side checks from the
same annotations.

**50. Name common Data Annotation attributes.**
`[Required]`, `[StringLength]`, `[Range]`, `[RegularExpression]`, `[EmailAddress]`, `[Compare]`,
`[Phone]`, `[Url]`, `[DataType]`, `[Display]`.

**51. What is the difference between server-side and client-side validation?**
Client-side runs in the browser (better UX, can be bypassed). Server-side runs on the server
(authoritative, always required).

**52. How do you implement custom validation?**
Derive from `ValidationAttribute` and override `IsValid`, or implement `IValidatableObject` for
model-level/cross-field rules.

**53. What is remote validation?**
`[Remote]` calls a server action via AJAX to validate a field (e.g., username availability) while
the user types.

**54. How is client-side validation enabled?**
Include jQuery, jQuery Validation, and jQuery Unobtrusive Validation scripts; the framework emits
`data-val-*` attributes from annotations. Ensure unobtrusive validation is enabled.

---

## State Management

**55. How do you manage state in stateless HTTP?**
ViewData/ViewBag (request), TempData (next request), Session (per user), Cookies (client), Cache
(shared), hidden fields/query strings (round trip).

**56. What is TempData and how long does it live?**
A dictionary that persists data until it's read (typically across one redirect). Use `Keep`/`Peek`
to retain after reading. Backed by Session (or cookies in Core).

**57. What is the difference between Session and Cache?**
Session is per-user; Cache is shared application-wide. Session stores user-specific data; Cache
stores expensive shared data.

**58. What are the drawbacks of Session state?**
Consumes server memory and complicates load-balancing (use a distributed cache like Redis/SQL
Server for web farms).

**59. What is output caching?**
Caching the rendered output of an action to avoid re-executing it: `[OutputCache(Duration=60)]`
(MVC 5) / Output Caching middleware (Core).

**60. How do you store complex objects in Core Session?**
Serialize them (e.g., JSON) since Core Session stores byte arrays/strings.

---

## Filters

**61. What are filters in MVC?**
Components that run cross-cutting logic before/after stages of the request pipeline.

**62. What are the filter types and order?**
**Authorization** → **Action** → **Result** filters, with **Exception** filters for errors.
(Core adds Resource filters.)

**63. At what levels can filters be applied?**
Action, controller, and global levels.

**64. Give examples of built-in filters.**
`[Authorize]`, `[AllowAnonymous]`, `[ValidateAntiForgeryToken]`, `[OutputCache]`, `[HandleError]`,
`[RequireHttps]`.

**65. How do you create a custom action filter?**
Inherit `ActionFilterAttribute` and override `OnActionExecuting`/`OnActionExecuted` (MVC 5), or
implement `IActionFilter`/`IAsyncActionFilter` (Core).

**66. How do you inject dependencies into filters (Core)?**
Use `[ServiceFilter(typeof(MyFilter))]` or `[TypeFilter]` so the DI container constructs them.

**67. How do you register a global filter?**
MVC 5: `FilterConfig.RegisterGlobalFilters`. Core: `options.Filters.Add<T>()` in
`AddControllersWithViews`.

**68. What is an exception filter?**
A filter (`IExceptionFilter`) that handles unhandled exceptions in actions — for logging and
custom error responses.

---

## Security

**69. What is the difference between authentication and authorization?**
Authentication verifies identity (who you are); authorization controls access (what you can do).

**70. What is the `[Authorize]` attribute?**
Restricts access to authenticated users; can require roles (`[Authorize(Roles="Admin")]`) or
policies.

**71. What is ASP.NET Identity?**
The membership framework for managing users, passwords, roles, claims, external logins, 2FA, and
lockout.

**72. What is a CSRF attack and how does MVC prevent it?**
Cross-Site Request Forgery tricks a user's browser into submitting unwanted requests. MVC defends
with `@Html.AntiForgeryToken()` in forms and `[ValidateAntiForgeryToken]` on actions.

**73. What is an XSS attack and how is it mitigated?**
Cross-Site Scripting injects malicious scripts. Razor HTML-encodes output by default; avoid
`Html.Raw` with untrusted data; validate/sanitize input.

**74. How does EF help prevent SQL injection?**
EF parameterizes queries (and LINQ avoids string concatenation), preventing injection. Avoid raw
SQL with string concatenation.

**75. What is claims-based authorization?**
Authorization based on claims (key-value facts about the user) and policies that define required
claims/requirements.

**76. What is the difference between role-based and policy-based authorization?**
Role-based checks membership in roles. Policy-based is more flexible — define requirements
(claims, custom logic) and apply via `[Authorize(Policy="...")]`.

**77. How do you enforce HTTPS?**
`[RequireHttps]` / `UseHttpsRedirection` + HSTS (`UseHsts`).

---

## Entity Framework & Data

**78. What is Entity Framework?**
An ORM that maps C# classes to database tables, letting you query with LINQ instead of SQL.

**79. What is Code First?**
Defining the model as C# classes and generating the database (and evolving it via migrations).

**80. What is a DbContext?**
The primary class representing a session with the database; exposes `DbSet<T>` properties and
tracks changes (unit of work).

**81. What are migrations?**
A mechanism to evolve the database schema in sync with model changes (`Add-Migration`,
`Update-Database` / `dotnet ef`).

**82. What is the N+1 query problem?**
Lazy loading inside a loop issues one query per item. Fix with eager loading (`Include`).

**83. What is the difference between eager, lazy, and explicit loading?**
Eager: load related data upfront (`Include`). Lazy: load on first access (requires proxies).
Explicit: load on demand via `Entry().Load()`.

**84. What is the repository pattern? Is it needed with EF?**
An abstraction over data access. `DbContext`/`DbSet` already implement unit-of-work/repository, so
add custom repositories only when they add value (testing, swapping sources).

**85. What is `AsNoTracking`?**
Disables change tracking for read-only queries, improving performance.

---

## Web API & AJAX

**86. What is the difference between MVC and Web API?**
MVC returns views (HTML); Web API returns data (JSON/XML) for clients (SPAs, mobile). In ASP.NET
Core they are unified into one framework.

**87. What is REST?**
An architectural style using HTTP verbs (GET/POST/PUT/DELETE) on resources with appropriate
status codes and stateless communication.

**88. What is `[ApiController]` (Core)?**
An attribute enabling API conventions: automatic model validation (400 on invalid), binding
source inference, and attribute routing requirement.

**89. How do you return JSON from an action?**
`return Json(data);` (Core) or `return Json(data, JsonRequestBehavior.AllowGet);` for GET in MVC 5.

**90. How do you call a server action via AJAX?**
With `fetch` or `$.ajax`/`$.post`, hitting an action URL and handling the JSON/HTML response.

**91. What is CORS?**
Cross-Origin Resource Sharing — a mechanism to allow/deny browser requests from other origins;
configured with a CORS policy.

**92. How do you do partial page updates?**
Return a `PartialViewResult` and inject the returned HTML into the DOM via AJAX.

**93. What HTTP status code should a successful POST that creates a resource return?**
201 Created (with a Location header), via `CreatedAtAction`/`Created`.

---

## Advanced & Core

**94. What is dependency injection and how does Core support it?**
Supplying dependencies from outside (usually constructor injection). Core has a **built-in DI
container**; register services in `Program.cs` (`AddTransient/AddScoped/AddSingleton`).

**95. What are the service lifetimes?**
Transient (new each resolution), Scoped (one per request), Singleton (one per app).

**96. What is middleware (Core)?**
Components in the request pipeline that handle requests/responses (auth, static files, routing,
etc.), composed with `app.UseXxx()`.

**97. What is the difference between `IActionResult` and `ActionResult<T>` (Core)?**
`IActionResult` returns any result. `ActionResult<T>` allows returning either a typed value or an
action result, improving API expressiveness and Swagger output.

**98. What is bundling and minification?**
Combining (bundling) and shrinking (minification) CSS/JS to reduce requests and payload size.
MVC 5 uses `System.Web.Optimization`; Core uses build tools + the `environment` Tag Helper.

**99. What is `Program.cs`/`Startup` in Core?**
The application entry point that configures services (DI) and the middleware pipeline.

**100. How do you handle errors globally?**
Exception filters, `UseExceptionHandler` middleware, custom error pages, and `[HandleError]`
(MVC 5). Log via `ILogger`.

**101. What is a View Component (Core)?**
A reusable, self-contained piece of UI with its own logic (like a mini-controller + partial),
invoked with `<vc:...>` or `Component.InvokeAsync`.

**102. What is Kestrel?**
The cross-platform web server built into ASP.NET Core, typically run behind a reverse proxy
(IIS/Nginx) in production.

**103. What is the difference between `AddControllers`, `AddControllersWithViews`, and
`AddRazorPages`?**
`AddControllers` — APIs only. `AddControllersWithViews` — MVC with views. `AddRazorPages` —
Razor Pages page model.

**104. How do you configure the app for different environments?**
Use `ASPNETCORE_ENVIRONMENT`, environment-specific `appsettings.{Environment}.json`, and
`app.Environment.IsDevelopment()` checks.

---

## Quick-Fire Round

- **Where are routes configured in MVC 5?** `App_Start/RouteConfig.cs` (and `Global.asax`).
- **What is `Global.asax`?** The app startup file (Application_Start, etc.) in MVC 5.
- **Default view file extension?** `.cshtml` (Razor C#).
- **What returns the current logged-in user in a controller?** `User` (an `IPrincipal`/`ClaimsPrincipal`).
- **How to pass data from controller to view without a model?** ViewBag/ViewData/TempData.
- **Which folder holds shared views?** `Views/Shared/`.
- **What attribute ignores anti-forgery for an action?** There isn't a need — simply omit `[ValidateAntiForgeryToken]`; for global enforcement use `[IgnoreAntiforgeryToken]` (Core).
- **What's the controller naming convention?** `XxxController` (suffix required).
- **Can a single view use multiple models?** Not directly — use a ViewModel that wraps them.
- **What is scaffolding?** Auto-generating controllers/views/CRUD code from a model.

---

➡️ Practice implementation with the [Coding Challenges](coding-challenges.md).
