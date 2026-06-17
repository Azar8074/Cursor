# 05 — Frontend: Razor, JavaScript & CSS

MVC renders HTML server-side with Razor, but a modern MVC developer also needs
solid client-side skills. You don't need to be a full SPA expert, but you must
be comfortable with the browser, JS, and CSS.

---

## 1. Razor view engine
Razor mixes C# and HTML. Server runs the C#, outputs HTML.

```cshtml
@model OrderListVm

<h1>Orders</h1>
@if (!Model.Orders.Any())
{
    <p>No orders yet.</p>
}
else
{
    <table>
        @foreach (var o in Model.Orders)
        {
            <tr><td>@o.Id</td><td>@o.Customer</td><td>@o.Total.ToString("C")</td></tr>
        }
    </table>
}
```

Key rules:
- `@expression` outputs **HTML-encoded** text (XSS-safe by default).
- `@{ ... }` for code blocks; `@: ` for literal text; `@@` for a literal `@`.
- `@Html.Raw(...)` outputs unencoded — **dangerous**, only for trusted content.
- Strongly-typed views (`@model`) give IntelliSense + compile-time checks.

---

## 2. Layouts, partials, sections, components
- **Layout** (`_Layout.cshtml`) — shared shell (header/nav/footer),
  `@RenderBody()`, `@RenderSection("scripts", required: false)`.
- **`_ViewStart.cshtml`** — sets the default layout.
- **`_ViewImports.cshtml`** (Core) — shared `@using`, tag helpers.
- **Partial views** — reusable markup fragments
  (`@await Html.PartialAsync("_Row", item)`).
- **View Components** (Core) / **child actions** (classic) — partials **with
  their own logic** (e.g. a cart summary that queries data).

```cshtml
@section scripts {
    <script src="~/js/orders.js"></script>
}
```

---

## 3. Forms & helpers
### HTML helpers (classic) vs Tag Helpers (Core)
```cshtml
@* HTML helper *@
@Html.LabelFor(m => m.Email)
@Html.TextBoxFor(m => m.Email, new { @class = "form-control" })
@Html.ValidationMessageFor(m => m.Email)

@* Tag helper (Core) — cleaner, HTML-like *@
<label asp-for="Email"></label>
<input asp-for="Email" class="form-control" />
<span asp-validation-for="Email"></span>
```

- Always include the **anti-forgery token** in POST forms
  (`@Html.AntiForgeryToken()` / `asp-antiforgery`).
- Client + server validation: jQuery Unobtrusive Validation reads data-* attrs
  generated from your data annotations — but **always re-validate on the server**.

---

## 4. JavaScript essentials
You should be comfortable with modern JS even in a server-rendered app.
- **Variables:** `let`/`const` (not `var`), block scope.
- **Types & coercion:** `===` vs `==`, truthy/falsy, `null` vs `undefined`.
- **Functions:** arrow functions, closures, `this` binding.
- **Arrays/objects:** `map`/`filter`/`reduce`, spread/rest, destructuring.
- **Async:** Promises, `async/await`, `fetch`.
- **DOM:** `querySelector`, event listeners, delegation.
- **Modules:** `import`/`export`.

```javascript
async function loadOrders() {
  const res = await fetch('/api/orders', { headers: { 'Accept': 'application/json' } });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  const orders = await res.json();
  const tbody = document.querySelector('#orders tbody');
  tbody.innerHTML = orders
    .map(o => `<tr><td>${o.id}</td><td>${o.customer}</td></tr>`)
    .join('');
}
```

> When injecting server data into the DOM, **encode** it to avoid XSS. Prefer
> `textContent` over `innerHTML` for untrusted strings.

### AJAX with the anti-forgery token
```javascript
fetch('/orders/create', {
  method: 'POST',
  headers: { 'RequestVerificationToken': token, 'Content-Type': 'application/json' },
  body: JSON.stringify(payload)
});
```

---

## 5. jQuery (still common in MVC apps)
Many classic MVC apps use jQuery + Bootstrap. Know it, but lean on vanilla JS
(`fetch`, `querySelector`) for new code.

```javascript
$('#submit').on('click', function () {
  $.post('/orders/create', $('#form').serialize())
    .done(data => { /* ... */ })
    .fail(xhr => { /* ... */ });
});
```

---

## 6. CSS & layout
- **Box model**, specificity, the cascade, flexbox, CSS grid.
- **Responsive design** with media queries; mobile-first.
- **Bootstrap** is ubiquitous in MVC scaffolding — know the grid, components,
  utilities. Modern alternative: Tailwind / plain CSS.
- Methodologies: BEM naming, CSS variables, avoid deep selector nesting.

```css
.card { display: flex; gap: 1rem; padding: 1rem; }
@media (max-width: 600px) { .card { flex-direction: column; } }
```

---

## 7. Assets: bundling, minification, caching
- Combine + minify CSS/JS to reduce requests/size.
- Classic: `System.Web.Optimization` bundles. Core: build tooling / WebOptimizer
  / a JS bundler (Vite/webpack) for richer frontends.
- **Cache busting** with content hashes (`asp-append-version="true"` in Core).
- Serve static files efficiently with far-future cache headers.

---

## 8. When to add a SPA framework
Server-rendered Razor is great for content/forms. Reach for **React / Angular /
Vue / Blazor** when you need rich, stateful interactivity. Options:
- **Sprinkles** of JS on Razor pages (htmx, Alpine, vanilla) — simplest.
- **Razor + API + SPA island** for specific screens.
- **Full SPA** consuming your Web API.
- **Blazor** (C# in the browser via WebAssembly, or server-side) — stay in .NET.

---

## 9. Accessibility & UX basics
- Semantic HTML (`<button>`, `<nav>`, `<label>` tied to inputs).
- Keyboard navigation, focus states, `alt` text, color contrast, ARIA when needed.
- Progressive enhancement: the form should work without JS where feasible.

---

## Common pitfalls
- Using `@Html.Raw` / `innerHTML` with untrusted data → XSS.
- Trusting client-side validation only.
- Forgetting the anti-forgery token in AJAX POSTs.
- Giant unbundled/unminified asset payloads.
- Putting business logic in views.
- `var` and `==` in JS leading to scope/coercion bugs.

## Practice / interview questions
1. How does Razor protect against XSS, and how can you accidentally break it?
2. Difference between a partial view and a view component?
3. `let` vs `const` vs `var`; `==` vs `===`.
4. How do data annotations drive client-side validation?
5. How do you send the anti-forgery token with a `fetch` POST?
6. Explain the CSS box model and specificity.
7. When would you introduce a SPA framework or Blazor?
8. What is cache busting and why does it matter?
