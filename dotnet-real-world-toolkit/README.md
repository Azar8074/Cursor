# .NET Real-World Toolkit — Practical Libraries & Tools

> Hands-on notes for the libraries and tools you actually reach for on real
> ASP.NET / .NET projects: caching, real-time, background jobs, scheduling,
> messaging, logging, mapping, validation, resilience, and API docs.

These complement the [ASP.NET MVC Developer Notes](../aspnet-mvc-developer-notes/README.md).
That set teaches the **fundamentals**; this set teaches the **practical
ecosystem** you bolt onto a production app.

## Table of Contents

| # | Tool / Topic | File | Solves |
|---|--------------|------|--------|
| 01 | **Redis** (cache, distributed state) | [01-redis.md](./01-redis.md) | Fast shared cache, sessions, pub/sub, locks |
| 02 | **SignalR** (real-time) | [02-signalr.md](./02-signalr.md) | Server→client push (chat, notifications, live UI) |
| 03 | **Quartz.NET** (scheduling) | [03-quartz-net.md](./03-quartz-net.md) | Cron-style scheduled jobs |
| 04 | **Hangfire** (background jobs) | [04-hangfire.md](./04-hangfire.md) | Fire-and-forget / delayed / recurring jobs with a dashboard |
| 05 | **Message queues** (RabbitMQ / Azure Service Bus / Kafka) | [05-message-queues.md](./05-message-queues.md) | Decoupling, async workflows, integration |
| 06 | **Serilog** (structured logging) | [06-serilog-logging.md](./06-serilog-logging.md) | Searchable, structured logs & sinks |
| 07 | **AutoMapper & FluentValidation** | [07-automapper-fluentvalidation.md](./07-automapper-fluentvalidation.md) | Entity↔DTO mapping & expressive validation |
| 08 | **MediatR & CQRS** | [08-mediatr-cqrs.md](./08-mediatr-cqrs.md) | Thin controllers, decoupled request handlers |
| 09 | **Polly** (resilience) | [09-polly-resilience.md](./09-polly-resilience.md) | Retries, circuit breakers, timeouts |
| 10 | **Swagger / OpenAPI** | [10-swagger-openapi.md](./10-swagger-openapi.md) | Interactive API docs & client generation |

## How to use these

Each file follows the same shape:
- **What it is / when to use it** (and when *not* to).
- **Install** (NuGet package names).
- **Minimal working setup** (ASP.NET Core focused, with classic notes where relevant).
- **Common real-world patterns.**
- **Pitfalls & gotchas.**
- **Interview questions.**

## Quick decision guide

| If you need to... | Reach for |
|-------------------|-----------|
| Cache data shared across servers | **Redis** (01) |
| Push live updates to browsers | **SignalR** (02) |
| Run a job on a cron schedule | **Quartz.NET** (03) or Hangfire recurring (04) |
| Offload slow work from a request | **Hangfire** (04) or a **queue** (05) |
| Decouple services / integrate systems | **Message queue** (05) |
| Get searchable production logs | **Serilog** (06) |
| Map entities ↔ DTOs | **AutoMapper** (07) |
| Validate complex input cleanly | **FluentValidation** (07) |
| Keep controllers thin / do CQRS | **MediatR** (08) |
| Survive flaky network calls | **Polly** (09) |
| Document & test your API | **Swagger** (10) |

> Rule of thumb: add a library only when it earns its keep. Each one is a
> dependency to learn, secure, update, and operate.
