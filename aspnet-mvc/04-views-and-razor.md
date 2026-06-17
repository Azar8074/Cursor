# Module 04 — Views & Razor

## What is a View?

A **view** generates the HTML UI. ASP.NET MVC uses the **Razor** view engine — `.cshtml` files
that mix C# and HTML cleanly. Views live in `Views/{ControllerName}/{ActionName}.cshtml` and
shared ones in `Views/Shared/`.

## Razor Syntax Basics

Razor switches to C# with `@`.

```html
<!-- Inline expression -->
<p>Today is @DateTime.Now.DayOfWeek</p>

<!-- Code block -->
@{
    var greeting = "Hello";
    var count = 5;
}
<p>@greeting — @count items</p>

<!-- Explicit expression -->
<p>@(count + 1)</p>

<!-- Email - @ is escaped automatically by context, or use @@ -->
<p>Contact: support@@example.com</p>
```

### Control Flow in Razor

```html
@if (Model.IsActive)
{
    <span class="badge">Active</span>
}
else
{
    <span>Inactive</span>
}

<ul>
@foreach (var item in Model.Items)
{
    <li>@item.Name — @item.Price.ToString("C")</li>
}
</ul>

@switch (Model.Status)
{
    case "new": <span>New</span>; break;
    default: <span>—</span>; break;
}
```

### Mixing text & markup inside code

```html
@foreach (var n in Model.Numbers)
{
    <text>Number: </text> @n <br />
    @: Plain line of text with @n
}
```

## Strongly Typed Views

Declare the model type with `@model` to get IntelliSense and compile-time checking.

```html
@model MyApp.Models.Product

<h2>@Model.Name</h2>
<p>@Model.Price.ToString("C")</p>
```

For collections:

```html
@model IEnumerable<MyApp.Models.Product>

@foreach (var product in Model)
{
    <div>@product.Name</div>
}
```

## Layouts (Master Pages)

A **layout** defines the common page shell (header, nav, footer). Views render into it.

```html
<!-- Views/Shared/_Layout.cshtml -->
<!DOCTYPE html>
<html>
<head>
    <title>@ViewBag.Title - My App</title>
    @RenderSection("styles", required: false)
</head>
<body>
    <header>My Application</header>
    <main>
        @RenderBody()                <!-- the view's content goes here -->
    </main>
    <footer>&copy; @DateTime.Now.Year</footer>
    @RenderSection("scripts", required: false)
</body>
</html>
```

A view supplies sections:

```html
@{
    ViewBag.Title = "Home";
    Layout = "~/Views/Shared/_Layout.cshtml";
}
<h1>Welcome</h1>

@section scripts {
    <script src="~/js/home.js"></script>
}
```

### _ViewStart.cshtml

Runs before every view; sets a default layout so you don't repeat it.

```html
@{
    Layout = "~/Views/Shared/_Layout.cshtml";
}
```

### _ViewImports.cshtml (Core)

Shared `@using`/`@addTagHelper` directives for all views.

## Partial Views

Reusable view fragments (no layout). Names conventionally start with `_`.

```html
<!-- Views/Shared/_ProductCard.cshtml -->
@model Product
<div class="card">
    <h3>@Model.Name</h3>
    <p>@Model.Price.ToString("C")</p>
</div>
```

```html
<!-- Rendering it -->
@Html.Partial("_ProductCard", product)
@{ Html.RenderPartial("_ProductCard", product); }   <!-- writes directly, faster -->

<!-- Async (Core) -->
<partial name="_ProductCard" model="product" />
```

`@Html.Action` / View Components let a partial run its own controller logic.

## HTML Helpers

Generate HTML elements bound to the model (MVC 5 style).

```html
@Html.TextBoxFor(m => m.Name)
@Html.LabelFor(m => m.Name)
@Html.EditorFor(m => m.Price)
@Html.DropDownListFor(m => m.CategoryId, Model.Categories)
@Html.ValidationMessageFor(m => m.Name)
@Html.ActionLink("Edit", "Edit", new { id = Model.Id })

@using (Html.BeginForm("Create", "Product", FormMethod.Post))
{
    @Html.AntiForgeryToken()
    @Html.TextBoxFor(m => m.Name)
    <button type="submit">Save</button>
}
```

## Tag Helpers (ASP.NET Core)

A cleaner, HTML-like alternative to HTML helpers.

```html
<form asp-controller="Product" asp-action="Create" method="post">
    <label asp-for="Name"></label>
    <input asp-for="Name" class="form-control" />
    <span asp-validation-for="Name"></span>
    <button type="submit">Save</button>
</form>

<a asp-controller="Product" asp-action="Edit" asp-route-id="@Model.Id">Edit</a>
```

## Encoding & Security

Razor **HTML-encodes output by default**, protecting against XSS. To render raw HTML
(only with trusted content):

```html
@Html.Raw(Model.TrustedHtml)
```

## Key Takeaways

- Razor mixes C# and HTML with `@`; output is **HTML-encoded by default** (XSS-safe).
- Use **strongly typed** views (`@model`) for IntelliSense and safety.
- Layouts provide a shared shell (`@RenderBody`, `@RenderSection`); `_ViewStart` sets defaults.
- Reuse markup with **partial views**; use HTML helpers (MVC 5) or **Tag Helpers** (Core) for forms.

➡️ Next: [Module 05 — Models & ViewModels](05-models-and-viewmodels.md)
