# 09 — Polly (Resilience & Transient-Fault Handling)

**Polly** makes your app resilient to the inevitable failures of distributed
systems: a network blip, a momentarily overloaded API, a slow database. It
provides **retries, circuit breakers, timeouts, fallbacks, bulkheads, and rate
limiting** as composable policies.

---

## 1. Why & when to use it
Any time you call something that can fail transiently:
- HTTP calls to other services / third-party APIs.
- Database / cache / message-broker calls.
- Anything across a network.

Without resilience, one flaky dependency can cascade into outages.

---

## 2. The core strategies
| Strategy | What it does | Use for |
|----------|--------------|---------|
| **Retry** | Re-attempt failed calls | Transient errors (timeouts, 503) |
| **Circuit Breaker** | Stop calling a failing dependency for a while | Protect a struggling service & fail fast |
| **Timeout** | Abort a call that takes too long | Avoid hanging requests |
| **Fallback** | Provide a default when all else fails | Graceful degradation |
| **Bulkhead Isolation** | Limit concurrent calls | Stop one dependency exhausting resources |
| **Rate Limiter** | Cap call rate | Respect API quotas / protect downstream |
| **Hedging** (v8) | Issue a parallel retry before the first finishes | Reduce tail latency |

> **Polly v8** introduced the modern **`ResiliencePipeline`** API. Older code uses
> `Policy.Handle<...>()`. Both shown below.

---

## 3. Install
```bash
dotnet add package Polly                              # core
dotnet add package Microsoft.Extensions.Http.Resilience   # HttpClient integration (recommended)
```

---

## 4. Retry with exponential backoff + jitter
Backoff avoids hammering a recovering service; jitter avoids synchronized retry
storms.
```csharp
// Polly v8 pipeline
var pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true
    })
    .Build();

var result = await pipeline.ExecuteAsync(async ct => await CallApiAsync(ct));
```

```csharp
// Classic v7 style
var retry = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
```

> **Only retry idempotent operations.** Retrying a non-idempotent POST can create
> duplicates — use idempotency keys or don't retry it.

---

## 5. Circuit breaker
After N consecutive failures, "open" the circuit: subsequent calls fail
immediately (fast) for a cooldown, giving the dependency time to recover. Then it
goes "half-open" to test, and "closes" again on success.
```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,                       // 50% failures
        SamplingDuration = TimeSpan.FromSeconds(30),
        MinimumThroughput = 10,
        BreakDuration = TimeSpan.FromSeconds(15)
    })
    .Build();
```

---

## 6. Combine strategies (order matters)
A typical robust pipeline: **timeout → retry → circuit breaker**.
```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddTimeout(TimeSpan.FromSeconds(10))      // overall per attempt
    .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3, UseJitter = true })
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions())
    .Build();
```

---

## 7. The best fit: `HttpClient` + `IHttpClientFactory`
Attach resilience to a typed/named client so every call is protected:
```csharp
builder.Services.AddHttpClient<WeatherClient>(c =>
        c.BaseAddress = new Uri("https://api.weather.com"))
    .AddStandardResilienceHandler();   // sensible defaults: retry + breaker + timeout
```
`AddStandardResilienceHandler()` (from `Microsoft.Extensions.Http.Resilience`)
gives you a production-ready pipeline out of the box; customize as needed.

---

## 8. Fallback (graceful degradation)
```csharp
var pipeline = new ResiliencePipelineBuilder<ProductDto>()
    .AddFallback(new FallbackStrategyOptions<ProductDto>
    {
        FallbackAction = _ => Outcome.FromResultAsValueTask(ProductDto.Empty)
    })
    .AddRetry(new RetryStrategyOptions<ProductDto>())
    .Build();
```

---

## 9. Good practices
- Set **timeouts on everything** — a hung call is worse than a failed one.
- **Retry only transient + idempotent** operations; cap attempts.
- Always use **backoff + jitter**, never tight retry loops.
- Add a **circuit breaker** so you fail fast and let dependencies recover.
- **Log/observe** retries and breaker state (feed metrics — file 06).
- Make downstream calls **idempotent** so retries are safe.
- Test failure paths (chaos/fault injection — Polly has `Simmy`/chaos strategies).

---

## Pitfalls & gotchas
- Retrying non-idempotent operations → duplicates/corruption.
- Retry without backoff → you DDoS your own dependency.
- Too many retries → amplify load during an outage; long user waits.
- No timeout → threads/connections pile up (thread-pool starvation).
- Retrying behind a circuit breaker pointlessly (compose them correctly).
- Hiding persistent failures with silent fallbacks (alert on them).

## Interview questions
1. Name Polly's main resilience strategies.
2. Why use exponential backoff with jitter?
3. Explain the circuit breaker states (closed/open/half-open).
4. Which operations are safe to retry and why?
5. In what order would you compose timeout, retry, and circuit breaker?
6. How do you add resilience to an `HttpClient`?
7. What is a bulkhead and what does it protect against?
8. How does a hung dependency cause thread-pool starvation, and how do timeouts help?
