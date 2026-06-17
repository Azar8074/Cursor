# 07 — AutoMapper & FluentValidation

Two libraries you'll see on almost every layered .NET project:
- **AutoMapper** — copies data between objects (entity ↔ DTO/ViewModel).
- **FluentValidation** — expresses validation rules in clean, testable C# instead
  of attributes.

---

# Part A — AutoMapper

## 1. The problem it solves
You constantly map between **entities** (DB) and **DTOs/ViewModels** (API/UI) to
avoid over-posting and over-exposure. Hand-writing this is repetitive:
```csharp
var dto = new ProductDto { Id = p.Id, Name = p.Name, Price = p.Price /* ... */ };
```
AutoMapper does it by convention (matching property names).

## 2. Install & configure
```bash
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```
```csharp
builder.Services.AddAutoMapper(typeof(Program));   // scans for Profiles
```

## 3. Define a Profile
```csharp
public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>();                       // by convention
        CreateMap<Product, ProductListItem>()
            .ForMember(d => d.CategoryName,
                       o => o.MapFrom(s => s.Category.Name));    // custom member
        CreateMap<CreateProductDto, Product>()
            .ForMember(d => d.Id, o => o.Ignore());
    }
}
```

## 4. Use it
```csharp
public class ProductsController(IMapper mapper, AppDbContext db) : Controller
{
    public async Task<IActionResult> Get(int id)
    {
        var product = await db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        return product is null ? NotFound() : Ok(mapper.Map<ProductDto>(product));
    }
}
```

### ProjectTo — map **in the database** (great with EF)
```csharp
var items = await db.Products
    .ProjectTo<ProductListItem>(mapper.ConfigurationProvider)   // builds SQL SELECT
    .ToListAsync();
```
`ProjectTo` translates the mapping into the SQL projection, so only needed
columns are queried (avoids loading whole entities).

## 5. Good practices
- Keep maps in **Profiles**; validate config at startup
  (`configuration.AssertConfigurationIsValid()`).
- Prefer `ProjectTo` for read queries with EF.
- Don't hide complex business logic in mappings — keep them dumb.
- Some teams prefer **manual mapping** or source generators (**Mapperly**) for
  clarity/performance — AutoMapper's "magic" can obscure bugs. Choose deliberately.

## AutoMapper pitfalls
- Silent mismatches when property names drift (validate config!).
- Lazy-loading triggered during mapping (N+1) — project or include first.
- Overusing it for complex transformations (becomes hard to debug).

---

# Part B — FluentValidation

## 1. The problem it solves
Data annotations (`[Required]`, `[Range]`) are fine for simple cases but get
awkward for **conditional**, **cross-field**, or **complex** rules. FluentValidation
puts rules in a dedicated, unit-testable class.

## 2. Install
```bash
dotnet add package FluentValidation.AspNetCore
```
```csharp
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();   // integrates with ModelState
```

## 3. Define a validator
```csharp
public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator(IProductService products)
    {
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress();

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);

        RuleFor(x => x.CouponCode)
            .NotEmpty()
            .When(x => x.UsesCoupon);            // conditional rule

        RuleForEach(x => x.Items).SetValidator(new OrderItemValidator());

        // async / external check
        RuleFor(x => x.ProductId)
            .MustAsync(async (id, ct) => await products.ExistsAsync(id))
            .WithMessage("Product does not exist.");
    }
}
```

## 4. Use it
With auto-validation, invalid requests populate `ModelState` / return 400
automatically. Or run manually:
```csharp
var result = await _validator.ValidateAsync(dto);
if (!result.IsValid)
    return BadRequest(result.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
```

## 5. Why it's nice
- Rules live in one **testable** class (no controller clutter).
- Supports **conditional**, **cross-property**, **collection**, and **async** rules.
- Custom messages, severities, rule sets, and **DI** (inject services into
  validators).
- Easy to unit test:
```csharp
var result = new CreateOrderValidator(productSvc).TestValidate(dto);
result.ShouldHaveValidationErrorFor(x => x.Quantity);
```

## FluentValidation pitfalls
- Async DB calls in validators add latency — keep them lean; some checks belong
  in the service/domain layer.
- Duplicating rules across DTO validation and domain logic — decide ownership.
- Forgetting to register validators (rules silently don't run).

---

## When to use which
- **AutoMapper** → reduce boilerplate mapping between layers.
- **FluentValidation** → express and test input validation rules cleanly.
They're complementary and frequently used together (often alongside MediatR —
file 08 — where a pipeline behavior runs validation before each handler).

## Interview questions
1. Why map entities to DTOs at all (security/coupling)?
2. What does AutoMapper's `ProjectTo` do and why prefer it with EF?
3. How do you guard against silent AutoMapper mismatches?
4. When would you choose manual mapping over AutoMapper?
5. FluentValidation vs data annotations — pros/cons?
6. How do you write a conditional or cross-field rule?
7. How do you inject a service into a validator and run an async check?
8. Where should validation live: DTO, controller, or domain?
