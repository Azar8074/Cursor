# 11 — DevOps, Deployment & Tooling

Writing code is half the job; shipping and operating it reliably is the other
half. These skills make you far more valuable on a team.

---

## 1. Version control with Git (non-negotiable)
- Core: `clone`, `add`, `commit`, `push`, `pull`, `fetch`, `merge`, `rebase`,
  `branch`, `checkout/switch`, `stash`, `log`, `diff`, `cherry-pick`, `revert`.
- **Branching strategies:** trunk-based (short-lived branches) vs GitFlow.
- **Pull requests / code review:** small, focused PRs; clear descriptions;
  meaningful commit messages (consider Conventional Commits).
- **Merge vs rebase:** rebase for a clean local history; merge to preserve
  context. Don't rewrite shared/published history.
- Resolve conflicts confidently; use `.gitignore` (never commit `bin/`, `obj/`,
  secrets, `node_modules`).

---

## 2. Build & dependency management
- **MSBuild / `dotnet build`**, solution & project files (`.csproj`).
- **NuGet** for .NET packages; **npm/yarn** for frontend.
- Pin versions; audit for vulnerabilities (`dotnet list package --vulnerable`).
- Understand **Debug vs Release** builds and configuration transforms
  (`web.config` transforms classic; `appsettings.{Environment}.json` Core).

---

## 3. CI/CD
- **Continuous Integration:** every push builds + runs tests automatically.
- **Continuous Delivery/Deployment:** automated release to environments.
- Tools: **GitHub Actions**, **Azure DevOps Pipelines**, GitLab CI, Jenkins,
  TeamCity.
- A typical pipeline: restore → build → test → (analyze) → publish artifact →
  deploy to staging → (approval) → deploy to prod.

```yaml
# Minimal GitHub Actions for a .NET app
name: ci
on: [push, pull_request]
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

---

## 4. Hosting & web servers
- **IIS** (classic ASP.NET on Windows): app pools, bindings, modules,
  `web.config`. ASP.NET Core runs on **Kestrel**, usually behind IIS/nginx as a
  reverse proxy.
- Environments: Development / Staging / Production — environment-specific config.
- **Self-contained vs framework-dependent** deployments (Core).
- App settings via environment variables; secrets in a vault.

---

## 5. Containers & cloud
- **Docker:** package the app + runtime into an image; consistent across
  environments. Know `Dockerfile`, images vs containers, ports, volumes.
- **Orchestration:** Kubernetes / Azure Container Apps for scaling & resilience.
- **Cloud (Azure is most common for .NET):**
  - **App Service** (PaaS web hosting), **Azure SQL**, **Blob Storage**,
    **Key Vault**, **Application Insights**, **Service Bus**, **Redis Cache**.
  - AWS equivalents: Elastic Beanstalk/ECS, RDS, S3, Secrets Manager, CloudWatch.

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

---

## 6. Configuration & secrets in deployment
- Never bake secrets into images or commit them.
- Use environment variables, vaults (Key Vault/Secrets Manager), and the
  Options pattern.
- Separate config per environment; validate config at startup (fail fast).

---

## 7. Logging, monitoring & observability
- **Structured logging:** Serilog/NLog + `ILogger<T>`; log levels; correlation IDs.
- **Centralized logs:** Seq, ELK/OpenSearch, Application Insights, Datadog.
- **Metrics & tracing:** OpenTelemetry, App Insights; track latency, error rate,
  throughput (the "RED" metrics) and resource saturation.
- **Health checks** (`/health`) for load balancers/orchestrators.
- **Alerting** on error spikes, latency, and resource limits.

```csharp
_logger.LogInformation("Order {OrderId} placed for {Amount:C}", order.Id, order.Total);
```

---

## 8. Database deployment
- Apply EF migrations via pipeline (`dotnet ef migrations script` → reviewed SQL,
  or migration bundles). Avoid auto-migrate-on-startup in production.
- Back up before schema changes; make migrations backward-compatible for
  zero-downtime (expand/contract pattern).

---

## 9. Release safety
- **Blue-green / canary** deployments to reduce risk.
- **Feature flags** to decouple deploy from release.
- Roll-back plan; smoke tests post-deploy.
- Monitor immediately after release.

---

## 10. Developer tooling
- **IDE:** Visual Studio / VS Code / Rider — debugger, breakpoints, watch,
  hot reload.
- **Debugging:** conditional breakpoints, call stacks, immediate window,
  remote debugging, memory/CPU profiling, dump analysis.
- **Static analysis / linting:** Roslyn analyzers, EditorConfig, StyleCop,
  SonarQube. **Code formatting:** `dotnet format`.

---

## Common pitfalls
- Committing secrets or `bin/obj/node_modules`.
- "Works on my machine" — env/config drift; fix with containers + config mgmt.
- Auto-migrating the DB on app start in prod.
- No monitoring/logging — flying blind in production.
- Manual, undocumented deployments.
- Long-lived feature branches → painful merges.

## Practice / interview questions
1. Difference between CI and CD?
2. Outline a CI/CD pipeline for a .NET web app.
3. Merge vs rebase — when each?
4. How do you handle secrets across environments?
5. What does Docker solve and what are image vs container?
6. How do you deploy DB schema changes safely with zero downtime?
7. What is structured logging and why use it?
8. Blue-green vs canary deployment?
