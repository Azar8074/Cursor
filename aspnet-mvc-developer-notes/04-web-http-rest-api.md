# 04 — Web Fundamentals, HTTP, REST & Web API

MVC sits on top of the web platform. Deep knowledge of HTTP and REST makes you a
better backend engineer and is heavily tested in interviews.

---

## 1. How the web works (the request you serve)
1. Browser resolves DNS → IP.
2. Opens TCP connection (TLS handshake for HTTPS).
3. Sends an **HTTP request** (method, path, headers, optional body).
4. Server (IIS/Kestrel) routes to your app → MVC pipeline.
5. App returns an **HTTP response** (status, headers, body).
6. Browser parses HTML, fetches assets (CSS/JS/images), renders, runs JS.

---

## 2. HTTP essentials
### Methods (verbs)
| Method | Purpose | Safe | Idempotent |
|--------|---------|------|------------|
| GET | Read | ✅ | ✅ |
| POST | Create / non-idempotent action | ❌ | ❌ |
| PUT | Replace/update (full) | ❌ | ✅ |
| PATCH | Partial update | ❌ | ❌* |
| DELETE | Remove | ❌ | ✅ |
| HEAD/OPTIONS | Metadata / preflight | ✅ | ✅ |

*Safe* = no server state change. *Idempotent* = same effect if repeated.

### Status codes (know the families)
- **2xx** success: 200 OK, 201 Created, 204 No Content.
- **3xx** redirect: 301 (permanent), 302/307 (temporary), 304 Not Modified.
- **4xx** client error: 400 Bad Request, 401 Unauthorized (not authenticated),
  403 Forbidden (no permission), 404 Not Found, 409 Conflict, 422 Unprocessable,
  429 Too Many Requests.
- **5xx** server error: 500 Internal, 502 Bad Gateway, 503 Unavailable, 504 Timeout.

### Headers worth knowing
`Content-Type`, `Accept`, `Authorization`, `Cache-Control`, `ETag`,
`Set-Cookie`/`Cookie`, `Location`, `Content-Length`, CORS headers
(`Access-Control-Allow-Origin`), security headers (HSTS, CSP, X-Frame-Options).

### HTTP versions
HTTP/1.1 (keep-alive), HTTP/2 (multiplexing, header compression), HTTP/3 (QUIC,
over UDP). Mostly transparent to your app but affects performance.

---

## 3. URLs, cookies, sessions
- **URL parts:** scheme, host, port, path, query string, fragment.
- **Cookies:** small client-stored key/values sent with each request. Flag
  `HttpOnly` (no JS access), `Secure` (HTTPS only), `SameSite` (CSRF defense).
- **Sessions:** server-side state keyed by a cookie. Stateful → harder to scale;
  use distributed session store (Redis/SQL) behind a load balancer.

---

## 4. REST principles
REST = an architectural style for APIs over HTTP.
- **Resources** identified by URLs (`/api/products/42`).
- **Verbs** express the action (GET/POST/PUT/DELETE).
- **Representations** (usually JSON) carry state.
- **Stateless** — each request self-contained (no server session).
- Use proper **status codes** and **nouns** (not verbs) in URLs.

```
GET    /api/products          → list
GET    /api/products/42       → one
POST   /api/products          → create (201 + Location)
PUT    /api/products/42       → replace
PATCH  /api/products/42       → partial update
DELETE /api/products/42       → delete (204)
```

Good API design: versioning (`/api/v1/...`), pagination, filtering/sorting,
consistent error shape, HATEOAS (optional), idempotency keys for retries.

---

## 5. Building APIs in .NET
### Classic stack
- **ASP.NET Web API** (`ApiController`) for JSON/HTTP services, separate from MVC
  controllers (different pipeline in .NET Framework).

### Modern stack (recommended)
- **ASP.NET Core** unifies MVC + Web API: `[ApiController]` + `ControllerBase`,
  attribute routing, built-in model validation, content negotiation.
- **Minimal APIs** for lightweight endpoints.

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _svc;
    public ProductsController(IProductService svc) => _svc = svc;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> Get(int id)
    {
        var p = await _svc.GetAsync(id);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductDto dto)
    {
        var created = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
}
```

### Documentation
- **Swagger / OpenAPI** (Swashbuckle / NSwag) to document & test endpoints.

---

## 6. Content negotiation & serialization
- Server picks a representation based on `Accept` header.
- JSON is default. Core uses `System.Text.Json` (fast) or Newtonsoft.
- Control casing, null handling, enums-as-strings, date formats explicitly.

---

## 7. CORS
Browsers block cross-origin requests unless the server opts in. Configure
allowed origins/methods/headers. Preflight `OPTIONS` for non-simple requests.

```csharp
builder.Services.AddCors(o => o.AddPolicy("spa",
    p => p.WithOrigins("https://app.example.com").AllowAnyHeader().AllowAnyMethod()));
app.UseCors("spa");
```

---

## 8. Consuming APIs
- Use **`HttpClient`** via **`IHttpClientFactory`** (avoids socket exhaustion).
- Handle timeouts, retries (Polly), and transient faults.
- Deserialize into DTOs; never trust external data.

```csharp
var resp = await _http.GetFromJsonAsync<WeatherDto>("https://api/weather?city=NYC");
```

---

## 9. Real-time & async patterns
- **SignalR** for server push (WebSockets/SSE fallback): chat, notifications,
  live dashboards.
- **Webhooks** for event callbacks.
- **Background work**: `IHostedService`/`BackgroundService`, Hangfire, queues.

---

## Common pitfalls
- Using GET for state-changing operations.
- Wrong status codes (200 for errors, 401 vs 403 confusion).
- New `HttpClient` per request (socket exhaustion) — use the factory.
- Ignoring CORS until it breaks in the browser.
- Returning entities instead of DTOs from APIs (over-exposure, cycles).
- No versioning or pagination on public APIs.

## Practice / interview questions
1. Difference between PUT and PATCH? Which is idempotent?
2. 401 vs 403 — when each?
3. What makes an API RESTful?
4. Why use `IHttpClientFactory` instead of `new HttpClient()`?
5. Explain CORS and preflight requests.
6. How does content negotiation work?
7. Cookie flags `HttpOnly`, `Secure`, `SameSite` — what do they do?
8. How would you design a paginated, versioned product API?
