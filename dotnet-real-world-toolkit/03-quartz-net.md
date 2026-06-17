# 03 — Quartz.NET (Job Scheduling)

**Quartz.NET** is a mature, full-featured job scheduling library (a port of Java
Quartz). Use it for **cron-style scheduled tasks**: nightly reports, cleanup
jobs, polling an inbox, sending reminders, recurring sync, etc.

---

## 1. Why & when to use it
- You need **precise, cron-based scheduling** ("every weekday at 6am",
  "every 15 minutes").
- You want triggers, calendars (skip holidays), misfire handling, and
  **clustering** (one job instance across many servers).

**Alternatives:**
- **Hangfire** (file 04) — also does recurring jobs + a dashboard + persistence;
  often simpler if you also need queued/background jobs.
- **`IHostedService` / `BackgroundService` + `PeriodicTimer`** — fine for simple
  fixed-interval loops with no persistence/clustering needs.
- Cloud schedulers (Azure Functions Timer trigger, AWS EventBridge).

---

## 2. Core concepts
- **Job** — the unit of work (`IJob.Execute`).
- **JobDetail** — a job definition + its data map.
- **Trigger** — *when* a job runs (simple interval or **cron**).
- **Scheduler** — runs jobs per their triggers.
- **JobDataMap** — pass parameters to a job.

---

## 3. Install
```bash
dotnet add package Quartz
dotnet add package Quartz.Extensions.Hosting   # ASP.NET Core integration + DI
```

---

## 4. Define a job
```csharp
public class SendRemindersJob : IJob
{
    private readonly IReminderService _reminders;
    private readonly ILogger<SendRemindersJob> _logger;

    public SendRemindersJob(IReminderService reminders, ILogger<SendRemindersJob> logger)
    {
        _reminders = reminders;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Reminder job started at {Time}", DateTimeOffset.Now);
        await _reminders.SendDueRemindersAsync();
    }
}
```

---

## 5. Register & schedule (ASP.NET Core)
```csharp
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("send-reminders");

    q.AddJob<SendRemindersJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(t => t
        .ForJob(jobKey)
        .WithIdentity("send-reminders-trigger")
        // Every weekday at 06:00
        .WithCronSchedule("0 0 6 ? * MON-FRI"));
});

builder.Services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);
```
DI works inside jobs (constructor injection) thanks to `Quartz.Extensions.Hosting`.

---

## 6. Cron expression cheat sheet
Quartz cron has **7 fields** (seconds first; day-of-week and day-of-month use
`?`):
```
┌───────────── second (0-59)
│ ┌───────────── minute (0-59)
│ │ ┌───────────── hour (0-23)
│ │ │ ┌───────────── day-of-month (1-31 or ?)
│ │ │ │ ┌───────────── month (1-12 or JAN-DEC)
│ │ │ │ │ ┌───────────── day-of-week (1-7 or SUN-SAT or ?)
│ │ │ │ │ │ ┌───────────── year (optional)
0 0 6 ? * MON-FRI *
```
Examples:
- `0 0/15 * * * ?` — every 15 minutes.
- `0 0 0 1 * ?` — midnight on the 1st of every month.
- `0 30 9 ? * MON` — 09:30 every Monday.

---

## 7. Simple interval trigger (no cron)
```csharp
q.AddTrigger(t => t
    .ForJob(jobKey)
    .StartNow()
    .WithSimpleSchedule(s => s.WithIntervalInMinutes(10).RepeatForever()));
```

---

## 8. Passing data to jobs
```csharp
q.AddJob<ReportJob>(opts => opts
    .WithIdentity(jobKey)
    .UsingJobData("reportType", "daily"));

// In the job:
var type = context.MergedJobDataMap.GetString("reportType");
```

---

## 9. Persistence & clustering (production)
- By default jobs are stored **in memory** (lost on restart).
- For durability and HA, use a **persistent JobStore (ADO.NET)** backed by a DB,
  and enable **clustering** so multiple instances coordinate and a job runs on
  exactly **one** node.
```csharp
q.UsePersistentStore(s =>
{
    s.UseProperties = true;
    s.UseSqlServer(connectionString);
    s.UseClustering();             // run once across the cluster
    s.UseNewtonsoftJsonSerializer();
});
```
- Run the Quartz DB schema setup scripts for your provider first.

---

## 10. Misfire handling & concurrency
- **Misfire:** what to do if a trigger was missed (app down, thread busy) —
  fire now, do nothing, etc. Configure per trigger.
- Prevent overlapping runs of the same job with `[DisallowConcurrentExecution]`.
```csharp
[DisallowConcurrentExecution]
public class SyncJob : IJob { /* ... */ }
```

---

## Pitfalls & gotchas
- Long-running work blocking the scheduler thread pool — keep jobs efficient or
  offload.
- In-memory store losing jobs on restart (use persistent store if needed).
- Running the same job on every node without clustering (duplicate work).
- Overlapping executions when a run takes longer than the interval (use
  `[DisallowConcurrentExecution]`).
- Time zones — specify the trigger time zone explicitly.
- Confusing Quartz 7-field cron with the standard 5-field cron.

## Interview questions
1. When do you choose Quartz.NET over `BackgroundService`/Hangfire?
2. Explain Job, Trigger, and Scheduler.
3. How do you ensure a scheduled job runs on only one server in a farm?
4. What is a misfire and how is it handled?
5. How do you pass parameters to a job?
6. How do you prevent overlapping job executions?
7. Write a cron expression for "every weekday at 6:00 AM".
8. How do you persist scheduled jobs across restarts?
