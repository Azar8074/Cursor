# Module 15 — Deployment & Best Practices

## Configuration Management

Keep settings out of code and separate per environment.

### ASP.NET Core (`appsettings.json` + environment overrides)

```json
// appsettings.json
{
  "ConnectionStrings": {
    "Default": "Server=.;Database=App;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "App": { "Name": "MyApp", "PageSize": 20 }
}
```

```csharp
// Strongly typed options
public class AppOptions { public string Name { get; set; } public int PageSize { get; set; } }

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));

// Inject IOptions<AppOptions>
public HomeController(IOptions<AppOptions> options) { var name = options.Value.Name; }
```

- Environment-specific files: `appsettings.Development.json`, `appsettings.Production.json`.
- The active environment is set by `ASPNETCORE_ENVIRONMENT` (Development/Staging/Production).

### MVC 5 (`Web.config`) + transforms

```xml
<connectionStrings>
  <add name="Default" connectionString="..." providerName="System.Data.SqlClient" />
</connectionStrings>
<appSettings>
  <add key="PageSize" value="20" />
</appSettings>
```

`Web.Release.config` transforms apply during publish.

## Secrets Management

- **Never** commit secrets (connection strings, API keys) to source control.
- Development: **User Secrets** (`dotnet user-secrets set "Key" "value"`).
- Production: environment variables, **Azure Key Vault**, AWS Secrets Manager, etc.

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Server=...;"
```

## Building & Publishing

```bash
# ASP.NET Core
dotnet publish -c Release -o ./publish

# Run the published app
dotnet ./publish/MyApp.dll
```

For MVC 5: **Visual Studio → right-click project → Publish** (Web Deploy, folder, or Azure).

## Hosting Options

| Platform | Notes |
|----------|-------|
| **IIS (Windows)** | classic; Core uses the ASP.NET Core Module + Kestrel behind IIS |
| **Kestrel** (Core) | cross-platform built-in web server; usually behind a reverse proxy |
| **Nginx / Apache** | reverse proxy in front of Kestrel on Linux |
| **Azure App Service** | managed PaaS, easy CI/CD |
| **Docker / Kubernetes** | containerized deployments |
| **IIS Express** | local development only |

### Dockerizing (Core)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

## Production Best Practices

### Security

- Force **HTTPS** + HSTS; redirect HTTP → HTTPS.
- Use **anti-forgery tokens** on all state-changing POSTs.
- Validate and encode all input/output (XSS, SQL injection — EF parameterizes for you).
- Don't leak stack traces — show friendly error pages in production.
- Keep dependencies patched; scan for vulnerabilities.

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
```

### Performance

- Enable **response compression** and **caching** (output/response/distributed cache).
- Use **async** I/O end-to-end.
- Bundle/minify static assets; serve via CDN; set cache headers.
- Use `AsNoTracking()` for read-only EF queries; avoid N+1 with `Include`.
- Profile and add database indexes for hot queries.

### Reliability & Observability

- Centralized **logging** (`ILogger`, Serilog) to a sink (file, Seq, ELK, App Insights).
- **Health checks** (`AddHealthChecks` / `MapHealthChecks("/health")`).
- Graceful error handling and retries for transient failures (e.g., Polly).
- Monitoring/alerting (Application Insights, Prometheus/Grafana).

```csharp
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
app.MapHealthChecks("/health");
```

### Code Quality

- **Thin controllers**, business logic in services.
- Use **DI** and program to interfaces.
- Write **unit/integration tests**; run them in CI.
- Follow consistent naming and structure; use ViewModels.
- Handle exceptions globally; log with context.

## CI/CD

Automate build → test → publish → deploy with GitHub Actions, Azure DevOps, GitLab CI, etc.

```yaml
# .github/workflows/dotnet.yml (sketch)
name: build
on: [push]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.0.x' }
      - run: dotnet restore
      - run: dotnet build --no-restore -c Release
      - run: dotnet test --no-build -c Release
```

## Pre-Deployment Checklist

- [ ] Connection strings/secrets configured for the target environment (not in source control).
- [ ] `ASPNETCORE_ENVIRONMENT` / build config set to Production/Release.
- [ ] Database migrations applied.
- [ ] HTTPS enforced; security headers set.
- [ ] Friendly error pages enabled; detailed errors disabled.
- [ ] Logging/monitoring/health checks wired up.
- [ ] Static assets bundled, minified, cache-busted.
- [ ] Tests passing in CI.

## Key Takeaways

- Externalize configuration per environment; **never commit secrets**.
- Publish with `dotnet publish -c Release`; host on IIS, Kestrel+proxy, Azure, or containers.
- In production: enforce HTTPS, friendly errors, logging, health checks, caching, and async I/O.
- Automate build/test/deploy with CI/CD and follow a deployment checklist.

🎉 You've completed the ASP.NET MVC track! Test yourself with the
➡️ [ASP.NET MVC Interview Questions](../interview-questions/aspnet-mvc-interview-questions.md).
