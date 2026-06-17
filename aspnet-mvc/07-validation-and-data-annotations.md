# Module 07 — Validation & Data Annotations

Validation ensures incoming data is correct before you process it. ASP.NET MVC supports both
**server-side** and **client-side** validation, driven mostly by **Data Annotation attributes**.

## Data Annotation Attributes

Apply attributes from `System.ComponentModel.DataAnnotations` to model/ViewModel properties.

```csharp
using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, MinimumLength = 2)]
    public string Name { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email")]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Min 8 characters")]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }

    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int Age { get; set; }

    [Phone]
    public string Phone { get; set; }

    [Url]
    public string Website { get; set; }

    [RegularExpression(@"^[A-Z]{2}\d{4}$", ErrorMessage = "Format: AA1234")]
    public string Code { get; set; }

    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }

    [CreditCard]
    public string Card { get; set; }
}
```

### Common Annotations

| Attribute | Purpose |
|-----------|---------|
| `[Required]` | value must be provided |
| `[StringLength(max, MinimumLength=)]` | string length bounds |
| `[MinLength]` / `[MaxLength]` | length (also for collections) |
| `[Range(min, max)]` | numeric/date range |
| `[RegularExpression(pattern)]` | regex match |
| `[EmailAddress]`, `[Phone]`, `[Url]`, `[CreditCard]` | format validators |
| `[Compare("OtherProp")]` | two fields must match |
| `[DataType(...)]` | rendering hint (Password, Date, …) |
| `[Display(Name=)]` | label text |
| `[DisplayFormat]` | formatting / null display |

## Server-Side Validation

The framework validates the bound model and populates `ModelState`.

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Register(RegisterViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);          // redisplay with error messages

    // ... save ...
    return RedirectToAction("Success");
}
```

> **Always validate on the server.** Client-side validation improves UX but can be bypassed.

## Displaying Errors in the View

```html
@model RegisterViewModel

@using (Html.BeginForm())
{
    @Html.AntiForgeryToken()

    @Html.ValidationSummary(true, "Please fix the following:")

    @Html.LabelFor(m => m.Email)
    @Html.TextBoxFor(m => m.Email)
    @Html.ValidationMessageFor(m => m.Email)

    <button type="submit">Register</button>
}
```

In ASP.NET Core with Tag Helpers:

```html
<form asp-action="Register" method="post">
    <div asp-validation-summary="All"></div>
    <label asp-for="Email"></label>
    <input asp-for="Email" />
    <span asp-validation-for="Email"></span>
    <button type="submit">Register</button>
</form>
```

## Client-Side Validation

MVC generates `data-val-*` attributes from the annotations. **jQuery Validation** + **jQuery
Unobtrusive Validation** turn them into live, client-side checks — no extra JS needed.

```html
@section scripts {
    <partial name="_ValidationScriptsPartial" />   <!-- Core -->
    <!-- MVC 5: @Scripts.Render("~/bundles/jqueryval") -->
}
```

Ensure `ClientValidationEnabled` and `UnobtrusiveJavaScriptEnabled` are on (default).

## Custom Validation Attributes

```csharp
public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext ctx)
    {
        if (value is DateTime date && date <= DateTime.Now)
            return new ValidationResult(ErrorMessage ?? "Date must be in the future");
        return ValidationResult.Success;
    }
}

// Usage
[FutureDate]
public DateTime AppointmentDate { get; set; }
```

## IValidatableObject (model-level validation)

For rules that span multiple properties:

```csharp
public class Booking : IValidatableObject
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (End <= Start)
            yield return new ValidationResult(
                "End must be after start", new[] { nameof(End) });
    }
}
```

## Remote Validation (server check while typing)

```csharp
[Remote("IsEmailAvailable", "Account", ErrorMessage = "Email already in use")]
public string Email { get; set; }

// Controller action
public JsonResult IsEmailAvailable(string email)
    => Json(!_repo.EmailExists(email), JsonRequestBehavior.AllowGet);
```

## Key Takeaways

- Decorate models with **Data Annotations** to declare validation rules once.
- Check `ModelState.IsValid` on the server — **always** validate server-side.
- Client-side validation comes "free" via unobtrusive jQuery validation from the same annotations.
- Use custom attributes, `IValidatableObject`, and `[Remote]` for complex/cross-field/async rules.

➡️ Next: [Module 08 — Entity Framework & Data Access](08-entity-framework.md)
