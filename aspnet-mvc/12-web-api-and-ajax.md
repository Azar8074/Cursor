# Module 12 — Web API & AJAX

Modern web apps expose **HTTP APIs** that return JSON, consumed by JavaScript (AJAX), SPAs
(React/Angular/Vue), and mobile apps. ASP.NET provides ASP.NET Web API (MVC 5) and built-in API
support in ASP.NET Core MVC.

## REST Basics

REST (Representational State Transfer) maps **HTTP verbs** to operations on **resources**.

| Verb | Operation | Example | Success status |
|------|-----------|---------|----------------|
| GET | read | `GET /api/products` | 200 OK |
| GET | read one | `GET /api/products/5` | 200 / 404 |
| POST | create | `POST /api/products` | 201 Created |
| PUT | replace | `PUT /api/products/5` | 200 / 204 |
| PATCH | partial update | `PATCH /api/products/5` | 200 / 204 |
| DELETE | delete | `DELETE /api/products/5` | 204 No Content |

## A Web API Controller (ASP.NET Core)

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase   // ControllerBase = no View support
{
    private readonly IProductService _service;
    public ProductsController(IProductService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> Get(int id)
    {
        var product = await _service.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(ProductDto dto)
    {
        var created = await _service.AddAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductDto dto)
    {
        if (id != dto.Id) return BadRequest();
        await _service.UpdateAsync(dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
```

- `[ApiController]` enables automatic model validation, binding source inference, and 400 responses.
- `ControllerBase` (not `Controller`) is used for APIs (no view rendering).

### Registration

```csharp
builder.Services.AddControllers();   // API controllers
// ...
app.MapControllers();
```

## ASP.NET Web API 2 (MVC 5 era)

```csharp
public class ProductsController : ApiController
{
    public IHttpActionResult Get(int id)
    {
        var product = _repo.GetById(id);
        return product == null ? (IHttpActionResult)NotFound() : Ok(product);
    }

    public IHttpActionResult Post(Product product)
    {
        _repo.Add(product);
        return Created($"api/products/{product.Id}", product);
    }
}
```

Configured in `WebApiConfig.cs` with `config.MapHttpAttributeRoutes()`.

## Returning JSON from a Regular MVC Action

```csharp
public ActionResult GetData()
{
    var data = new { name = "Alice", age = 30 };
    return Json(data);                       // Core: returns JSON
    // MVC 5 GET: return Json(data, JsonRequestBehavior.AllowGet);
}
```

## Consuming an API with AJAX

### Fetch API (modern, no jQuery)

```javascript
// GET
const res = await fetch('/api/products');
const products = await res.json();

// POST
await fetch('/api/products', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name: 'Phone', price: 499 })
});

// DELETE
await fetch(`/api/products/${id}`, { method: 'DELETE' });
```

### jQuery AJAX (common in MVC 5 projects)

```javascript
$.ajax({
    url: '/api/products',
    method: 'GET',
    success: function (data) {
        data.forEach(p => $('#list').append(`<li>${p.name}</li>`));
    },
    error: function (xhr) { console.error(xhr.responseText); }
});

$.post('/api/products', { name: 'Pen', price: 2.5 }, function (result) {
    console.log('created', result);
});
```

## Partial Page Updates (AJAX in MVC)

Return a **partial view** and inject it into the page:

```csharp
public ActionResult ProductRow(int id)
    => PartialView("_ProductRow", _repo.GetById(id));
```

```javascript
fetch(`/Product/ProductRow/${id}`)
    .then(r => r.text())
    .then(html => document.getElementById('row').innerHTML = html);
```

## CORS (Cross-Origin Resource Sharing)

Allow other origins (e.g., a separate SPA) to call your API:

```csharp
builder.Services.AddCors(o => o.AddPolicy("Spa", p =>
    p.WithOrigins("https://myspa.com").AllowAnyHeader().AllowAnyMethod()));

app.UseCors("Spa");
```

## Documentation with Swagger / OpenAPI

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();   // interactive API docs at /swagger
}
```

## Best Practices

- Use proper **HTTP status codes** and verbs (don't tunnel everything through GET/POST).
- Return **DTOs**, not EF entities, to control the API surface and avoid over-exposure.
- **Version** your API (`/api/v1/...`).
- Validate input (`[ApiController]` does 400 automatically) and handle errors consistently.
- Secure with authentication (JWT) and authorization; enable CORS deliberately.

## Key Takeaways

- REST maps HTTP verbs (GET/POST/PUT/DELETE) to resource operations with meaningful status codes.
- Use `[ApiController]` + `ControllerBase` in Core; `ApiController` in classic Web API.
- Consume APIs from JS with `fetch` or jQuery AJAX; return partial views for HTML fragment updates.
- Return DTOs, version your API, document with Swagger, and configure CORS for cross-origin clients.

➡️ Next: [Module 13 — Bundling, Minification & Areas](13-bundling-and-areas.md)
