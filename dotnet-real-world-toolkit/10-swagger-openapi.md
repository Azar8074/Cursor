# 10 — Swagger / OpenAPI (API Documentation)

**OpenAPI** is a standard, machine-readable description of a REST API.
**Swagger** is the popular tooling around it (the interactive **Swagger UI**, code
generators, etc.). With a couple of lines, your ASP.NET Core API gets live,
testable documentation that stays in sync with your code.

---

## 1. Why & when to use it
- **Living documentation** — generated from your code/attributes, so it doesn't go
  stale.
- **Try it out** — call endpoints from the browser without Postman.
- **Client generation** — generate strongly-typed C#/TypeScript/etc. clients from
  the spec.
- **Contract** — front-end/back-end/partners agree on the API shape.

Essential for any public or team-facing Web API.

---

## 2. The pieces
- **OpenAPI spec** — a JSON/YAML document describing paths, params, schemas,
  responses, security.
- **Swashbuckle** — the classic .NET library that generates the spec + Swagger UI.
- **NSwag** — alternative that also generates clients.
- **.NET 9+** ships built-in OpenAPI document generation
  (`Microsoft.AspNetCore.OpenApi`); you still add a UI (Swagger UI / Scalar).

---

## 3. Setup with Swashbuckle (most common)
```bash
dotnet add package Swashbuckle.AspNetCore
```
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders API", Version = "v1" });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();        // serves /swagger/v1/swagger.json
    app.UseSwaggerUI();      // serves the interactive UI at /swagger
}
```
Open `/swagger` to explore and call your endpoints.

---

## 4. Make the docs rich and accurate
Help Swagger describe responses precisely:
```csharp
/// <summary>Gets a product by id.</summary>
/// <response code="200">The product.</response>
/// <response code="404">Not found.</response>
[HttpGet("{id:int}")]
[ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<ProductDto>> Get(int id) { ... }
```
- `[ApiController]` enables automatic 400 responses for invalid models (documented
  too).
- Enable **XML comments** so `<summary>` text appears in the UI:
```xml
<!-- .csproj -->
<GenerateDocumentationFile>true</GenerateDocumentationFile>
```
```csharp
c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "MyApi.xml"));
```
- Use **DTOs** as request/response types so the generated schemas are clean (don't
  expose EF entities — file references the over-exposure risk).

---

## 5. Documenting authentication
Show a padlock and let users authorize in the UI:
```csharp
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header
});
c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    [new OpenApiSecurityScheme {
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    }] = Array.Empty<string>()
});
```

---

## 6. Versioning
Pair Swagger with **API versioning**
(`Asp.Versioning.Mvc` + `Asp.Versioning.Mvc.ApiExplorer`) to produce one Swagger
doc per version (`v1`, `v2`) so consumers see exactly what each version offers.

---

## 7. Generating clients from the spec
The `swagger.json` can feed code generators:
- **NSwag** — C#/TypeScript clients.
- **OpenAPI Generator** — many languages.
- Azure/Refit integrations.

This gives front-end and consumer teams typed clients for free, regenerated when
the API changes.

---

## 8. Production considerations
- Decide whether to **expose Swagger UI in production**. For internal/public APIs
  it can be fine; for sensitive internal APIs, **disable it or lock it behind
  auth** (it reveals your whole surface).
- Keep the spec accurate — treat `ProducesResponseType`/XML docs as part of the
  endpoint's definition of done.
- Alternative UIs: **Scalar**, **ReDoc** (nicer reading experience).

---

## Pitfalls & gotchas
- Exposing internal/admin APIs via public Swagger UI.
- Returning EF entities → ugly/cyclic schemas; use DTOs.
- Missing `[ProducesResponseType]` → docs don't show real status codes.
- XML comments not enabled → empty descriptions.
- Letting docs drift by hand-writing them instead of generating.
- Forgetting versioning, so breaking changes surprise consumers.

## Interview questions
1. OpenAPI vs Swagger — what's the difference?
2. How do you add Swagger to an ASP.NET Core API?
3. How do you document response types and status codes?
4. Why should request/response models be DTOs, not entities?
5. How do you represent JWT auth in Swagger?
6. How does Swagger help front-end/consumer teams (client generation)?
7. Should Swagger UI be enabled in production? Considerations?
8. How do you handle API versioning in the docs?
