# 06 — Serilog & Structured Logging

Good logging is how you understand and debug a running production system.
**Serilog** is the most popular .NET logging library because it produces
**structured logs** (key/value properties, not just text) that you can search,
filter, and aggregate.

---

## 1. Structured vs plain-text logging
Plain text:
```
Order 123 placed for customer 45 totalling 99.50
```
Structured (Serilog) — the message *and* named properties are captured:
```csharp
logger.LogInformation("Order {OrderId} placed for customer {CustomerId} totalling {Total}",
    order.Id, order.CustomerId, order.Total);
```
Stored as JSON you can query: `OrderId = 123`, `CustomerId = 45`, `Total = 99.50`.
Now you can search "all logs where `CustomerId = 45`" — impossible with flat text.

> Use **message templates** with named placeholders. Don't string-interpolate
> (`$"...{order.Id}..."`) — that throws away the structure.

---

## 2. Install
```bash
dotnet add package Serilog.AspNetCore
# Sinks (outputs):
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Seq         # great structured log server
```

---

## 3. Setup (ASP.NET Core)
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)   // read from appsettings
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://localhost:5341"));

var app = builder.Build();
app.UseSerilogRequestLogging();   // concise, structured HTTP request logs
```

`appsettings.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": { "Microsoft.AspNetCore": "Warning" }
    },
    "WriteTo": [{ "Name": "Console" }]
  }
}
```

You still inject the standard `ILogger<T>` everywhere — Serilog plugs in behind
`Microsoft.Extensions.Logging`.

---

## 4. Log levels (use them deliberately)
| Level | Use |
|-------|-----|
| **Trace/Verbose** | Very detailed dev diagnostics |
| **Debug** | Internal state useful while debugging |
| **Information** | Normal app events (request handled, order placed) |
| **Warning** | Unexpected but handled (retry, fallback, deprecated path) |
| **Error** | A failure of the current operation (exception caught) |
| **Critical/Fatal** | App-wide failure, data loss, can't continue |

Set a sensible production minimum (usually `Information`), and override noisy
namespaces to `Warning`.

---

## 5. Enrichment & context
Add properties automatically to every log in a scope:
```csharp
using (LogContext.PushProperty("OrderId", order.Id))
{
    _logger.LogInformation("Starting processing");   // includes OrderId
    _logger.LogInformation("Charged card");          // includes OrderId
}
```
Common enrichers: machine name, thread id, environment, **correlation/request id**
(tie all logs of one request together — essential in distributed systems).

---

## 6. Sinks (where logs go)
- **Console** — dev, and containerized apps (stdout → collected by the platform).
- **File** (rolling) — simple persistence.
- **Seq** — purpose-built structured log server with great querying (dev/prod).
- **Cloud/aggregators** — Application Insights, Elasticsearch/OpenSearch (ELK),
  Datadog, Splunk, Loki.
- You can write to multiple sinks at once.

---

## 7. Logging exceptions correctly
```csharp
try { await _payment.ChargeAsync(order); }
catch (Exception ex)
{
    // pass the exception as the FIRST arg so the stack trace is captured
    _logger.LogError(ex, "Failed to charge order {OrderId}", order.Id);
    throw;   // rethrow if you can't handle it
}
```

---

## 8. What to log (and what NOT to)
**Do:** request start/end + duration, key business events, warnings/errors with
context, external call results, correlation ids.

**Don't:**
- **Secrets / PII** — passwords, tokens, full card numbers, personal data
  (GDPR/PCI). Mask or omit.
- Inside tight loops at high volume (cost + noise).
- Huge payloads (truncate).
- Using logs as your metrics system (use metrics/tracing for that).

---

## 9. Beyond logs: the observability trio
- **Logs** — discrete events (Serilog).
- **Metrics** — aggregated numbers over time (latency, error rate, throughput) —
  OpenTelemetry/Prometheus/App Insights.
- **Traces** — follow a request across services (distributed tracing) —
  OpenTelemetry.
A mature system uses all three with shared **correlation IDs**.

---

## Pitfalls & gotchas
- String-interpolating messages (loses structure).
- Logging secrets/PII.
- Wrong level (everything at `Information`, or errors at `Debug`).
- Not passing the exception object to `LogError` (no stack trace).
- Over-logging in hot paths (performance + cost + noise).
- No correlation id, so you can't follow one request.

## Interview questions
1. Structured vs plain-text logging — why does it matter?
2. Why use message templates instead of string interpolation?
3. Explain the log levels and when to use each.
4. What is a sink? Name a few.
5. How do you correlate all logs belonging to one request?
6. What should you never log, and why?
7. How do you log an exception so the stack trace is preserved?
8. Logs vs metrics vs traces — what's the difference?
