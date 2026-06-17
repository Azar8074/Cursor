# 04 — Hangfire (Background Jobs)

**Hangfire** runs background work in .NET without a separate Windows Service or
process. It persists jobs to a database (so they survive restarts), retries
failures automatically, and ships with a **web dashboard** to monitor everything.

---

## 1. Why & when to use it
- **Offload slow work** from the request thread (send email, generate PDF, call a
  slow API) so the user gets a fast response.
- **Delayed** work ("send reminder in 24h").
- **Recurring** jobs (cron) — an alternative to Quartz.
- You want **durability + retries + a dashboard** with minimal setup.

**When *not* to:** very high-throughput event streaming (use a real message
broker — file 05); precise enterprise scheduling with complex calendars
(Quartz may fit better).

---

## 2. Job types
| Type | Method | Use |
|------|--------|-----|
| **Fire-and-forget** | `Enqueue` | Run once, ASAP, in background |
| **Delayed** | `Schedule` | Run once after a delay |
| **Recurring** | `AddOrUpdate` | Run on a cron schedule |
| **Continuation** | `ContinueJobWith` | Run after a parent job succeeds |
| **Batch** (Pro) | `Batch.StartNew` | Group jobs |

---

## 3. Install
```bash
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.SqlServer   # or Hangfire.Redis.StackExchange, PostgreSql, etc.
```

---

## 4. Setup (ASP.NET Core)
```csharp
builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("Hangfire")));

builder.Services.AddHangfireServer();   // processes the jobs

var app = builder.Build();
app.UseHangfireDashboard("/hangfire");  // secure this in production!
```
Hangfire auto-creates its tables in the configured database on first run.

---

## 5. Enqueue jobs
```csharp
public class CheckoutController(IBackgroundJobClient jobs) : Controller
{
    public IActionResult Complete(Order order)
    {
        // Fire-and-forget: returns immediately, email sent in background
        jobs.Enqueue<IEmailService>(svc => svc.SendReceiptAsync(order.Id));

        // Delayed
        jobs.Schedule<IReminderService>(s => s.SendFeedbackRequest(order.Id),
            TimeSpan.FromDays(3));

        return RedirectToAction("ThankYou");
    }
}
```

### Recurring jobs
```csharp
RecurringJob.AddOrUpdate<IReportService>(
    "daily-sales-report",
    svc => svc.GenerateDailyReport(),
    Cron.Daily(6));   // every day at 06:00
```

### Continuations
```csharp
var id = jobs.Enqueue<IVideoService>(s => s.Transcode(videoId));
jobs.ContinueJobWith<INotifier>(id, n => n.NotifyReady(videoId));
```

---

## 6. The dashboard
`/hangfire` shows queued, processing, succeeded, failed, scheduled, and recurring
jobs — with the ability to **requeue** and **inspect exceptions/retries**.
**Secure it**: it exposes job data and lets users trigger jobs. Restrict by auth
in production:
```csharp
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new MyAdminAuthFilter() }   // IDashboardAuthorizationFilter
});
```

---

## 7. Retries & reliability
- Failed jobs **retry automatically** (default 10 attempts, increasing delays).
  Tune with `[AutomaticRetry(Attempts = 5)]`.
- Jobs are **persisted**, so a server crash doesn't lose them.
- **At-least-once execution:** a job may run more than once (e.g. after a crash
  mid-run). Make jobs **idempotent**.
- Pass **IDs, not whole objects** — arguments are serialized; large/complex
  payloads bloat storage and can fail to serialize. Re-load data inside the job.

```csharp
[AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
public async Task SendReceiptAsync(int orderId)
{
    var order = await _repo.GetAsync(orderId);   // reload, don't pass the object
    await _email.SendAsync(order.CustomerEmail, BuildReceipt(order));
}
```

---

## 8. Scaling & queues
- Run **multiple Hangfire servers** (processes/instances) — they coordinate via
  the storage; jobs are processed once.
- Use **named queues** to prioritize/segregate work:
```csharp
builder.Services.AddHangfireServer(o => o.Queues = new[] { "critical", "default" });

[Queue("critical")]
public Task ChargePayment(int orderId) { ... }
```
- Storage choice matters: SQL Server is common; Redis storage offers higher
  throughput.

---

## 9. Hangfire vs Quartz vs queues
| Need | Best fit |
|------|----------|
| Offload work from a request, simple setup, dashboard | **Hangfire** |
| Rich cron scheduling, calendars, enterprise scheduling | **Quartz.NET** (file 03) |
| High-throughput, cross-service, durable messaging | **Message broker** (file 05) |

---

## Pitfalls & gotchas
- **Non-idempotent jobs** — at-least-once means duplicates can happen.
- Passing large/complex objects as arguments (serialize IDs instead).
- Unsecured dashboard exposed publicly.
- Treating Hangfire as a high-volume message bus (it's not).
- Forgetting `AddHangfireServer()` (jobs enqueue but never run).
- Long jobs blocking workers — tune worker count / use queues.

## Interview questions
1. Hangfire vs Quartz vs a message queue — when each?
2. What job types does Hangfire support?
3. Why must background jobs be idempotent?
4. Why pass IDs instead of full objects to a job?
5. How does Hangfire achieve durability and retries?
6. How do you scale Hangfire across servers?
7. How do you secure the dashboard?
8. What does "at-least-once execution" imply for your code?
