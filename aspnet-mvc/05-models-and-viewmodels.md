# Module 05 — Models, ViewModels & Model Binding

## Models

A **model** represents data and business logic. In MVC, "model" can mean several things:

- **Domain/Entity model** — maps to your database (e.g., an EF entity).
- **ViewModel** — data shaped specifically for a view.
- **Input/Binding model** — captures form submissions.

```csharp
public class Product            // domain entity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}
```

## ViewModels — Why and When

A **ViewModel** is a class tailored to a specific view. It combines/transforms data and avoids
exposing your entities directly to the UI.

```csharp
public class ProductEditViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    // Data the view needs beyond the entity:
    public IEnumerable<SelectListItem> Categories { get; set; }
    public int SelectedCategoryId { get; set; }
}
```

```csharp
public ActionResult Edit(int id)
{
    var product = _repo.GetById(id);
    var vm = new ProductEditViewModel
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price,
        SelectedCategoryId = product.CategoryId,
        Categories = _repo.GetCategories()
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
    };
    return View(vm);
}
```

### Benefits of ViewModels

- **Security** — avoid over-posting (don't bind sensitive entity fields directly).
- **Decoupling** — UI changes don't ripple into your domain model.
- **Clarity** — the view gets exactly the data it needs, including UI-only fields.
- **Validation** — UI-specific validation rules live on the ViewModel.

## Model Binding

**Model binding** automatically maps incoming request data (route values, query string, form
fields, JSON body) to action parameters / model properties — matching by name.

```csharp
// Form posts Name=Phone&Price=499 → bound to the Product object
[HttpPost]
public ActionResult Create(Product product)
{
    // product.Name == "Phone", product.Price == 499
    return View(product);
}

// Simple types from query string: /Search?term=abc&page=2
public ActionResult Search(string term, int page) { ... }

// Collections and complex types bind too (Items[0].Name, etc.)
public ActionResult Save(List<OrderLine> lines) { ... }
```

### Binding Sources (ASP.NET Core)

```csharp
public IActionResult Update(
    [FromRoute] int id,          // from the URL path
    [FromQuery] string sort,     // from query string
    [FromForm] string name,      // from form body
    [FromBody] Product product,  // from JSON request body
    [FromHeader] string apiKey)  // from a header
{ ... }
```

## Guarding Against Over-Posting

A malicious user could submit extra fields to set properties you didn't intend (e.g., `IsAdmin`).

```csharp
// ❌ Risk: binds every field including ones you don't want
public ActionResult Update(User user) { ... }

// ✅ Whitelist allowed fields (MVC 5)
public ActionResult Update([Bind(Include = "Name,Email")] User user) { ... }

// ✅ Best: bind to a ViewModel that ONLY has editable fields
public ActionResult Update(UserEditViewModel vm) { ... }

// Exclude specific fields
public ActionResult Update([Bind(Exclude = "IsAdmin")] User user) { ... }
```

## ModelState

After binding, `ModelState` holds binding/validation results.

```csharp
[HttpPost]
public ActionResult Create(ProductEditViewModel vm)
{
    if (!ModelState.IsValid)        // validation failed?
        return View(vm);            // redisplay with errors

    // add a manual error
    if (_repo.NameExists(vm.Name))
    {
        ModelState.AddModelError(nameof(vm.Name), "Name already taken");
        return View(vm);
    }

    _repo.Add(MapToEntity(vm));
    return RedirectToAction("Index");
}
```

## Mapping Between Models & ViewModels

For larger apps, use a mapping library like **AutoMapper** to reduce boilerplate.

```csharp
// Manual mapping
var vm = new ProductViewModel { Id = p.Id, Name = p.Name };

// AutoMapper
var vm = _mapper.Map<ProductViewModel>(product);
```

## Custom Model Binders

For non-standard binding (e.g., parse a custom date format), implement `IModelBinder`.

```csharp
public class DateTimeModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext context) { /* custom parse */ }
}
```

## Key Takeaways

- Distinguish **domain models** (data/business) from **ViewModels** (UI-shaped data).
- **Model binding** maps request data to parameters/objects automatically by name.
- Use ViewModels or `[Bind]` whitelists to prevent **over-posting** attacks.
- Check `ModelState.IsValid` and add custom errors with `ModelState.AddModelError`.

➡️ Next: [Module 06 — Routing](06-routing.md)
