# 10 — Performance, Caching & Optimization

Fast apps are a feature. Learn to **measure first**, then optimize the real
bottleneck (usually the database or network, rarely raw CPU).

---

## 1. The golden rule: measure, don't guess
- Profile before optimizing. Tools: Visual Studio Profiler, dotnet-trace,
  dotnet-counters, MiniProfiler, Application Insights / OpenTelemetry, SQL
  Profiler / Extended Events, browser DevTools (Network/Performance).
- Find the **slowest, most-hit** path. Optimize that. Re-measure.

---

## 2. Database performance (usually #1)
- Fix **N+1** queries (file 03); use `Include`/projection.
- Add the right **indexes**; make predicates SARGable (file 06).
- Select only needed columns/rows; **page** large results.
- Use `AsNoTracking()` for reads.
- Cache expensive, rarely-changing query results.
- Consider read replicas / denormalized read models for heavy read loads.

---

## 3. Caching layers
| Cache | Scope | Use | Risk |
|-------|-------|-----|------|
| **Output / response cache** | whole responses | static-ish pages/fragments | stale pages |
| **In-memory** (`IMemoryCache`) | per server | hot lookups, reference data | not shared across nodes |
| **Distributed** (Redis, SQL) | all servers | sessions, shared data | network/serialization cost |
| **HTTP/browser/CDN** | client/edge | static assets, public GETs | cache invalidation |

```csharp
var product = await _cache.GetOrCreateAsync($"product:{id}", entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
    return _repo.GetByIdAsync(id);
});
```

### Cache strategy notes
- **Cache-aside** (load on miss, store) is the common pattern.
- Set sensible **expiration** (absolute/sliding) and **invalidate** on writes.
- Beware **stampede** (many misses at once) — use locks/`GetOrCreate`.
- "There are only two hard things... cache invalidation and naming things."
- Distributed cache: choose a serialization format; watch payload sizes.

---

## 4. Async & throughput
- Use **async I/O** end-to-end to free threads under load (file 01).
- Don't block (`/.Result`/`.Wait()`); don't `Task.Run` for I/O.
- Parallelize independent I/O with `Task.WhenAll`.
- Stream large responses/files instead of buffering in memory.

---

## 5. Reducing allocations & GC pressure
- Avoid needless allocations in hot paths (LINQ chains, string concat, boxing).
- Use `StringBuilder`, pool buffers via `ArrayPool<T>`, and `Span<T>`/`Memory<T>`
  for high-perf scenarios.
- Prefer `ValueTask` only when measured beneficial.
- Reuse `HttpClient` via `IHttpClientFactory`.

---

## 6. Frontend performance
- Bundle + minify CSS/JS; compress (gzip/brotli).
- Cache static assets with content-hash busting (file 05).
- Lazy-load images/scripts; defer non-critical JS.
- Reduce payload size and number of requests (HTTP/2 helps).
- Use a CDN for static/edge content.
- Measure with Lighthouse / Core Web Vitals.

---

## 7. Scaling
- **Vertical** — bigger box (simple, limited).
- **Horizontal** — more instances behind a load balancer (needs **stateless**
  apps; externalize session/cache; sticky sessions are a smell).
- Offload long work to **background jobs/queues** (Hangfire, hosted services,
  Service Bus) so requests stay fast.
- Database scaling: read replicas, partitioning/sharding, connection pooling.

---

## 8. Connection & resource management
- Reuse DB connection pools (don't disable pooling).
- Dispose connections/streams (`using`).
- Set timeouts; add resilience (Polly retries + circuit breaker) for remote calls.
- Watch for thread-pool starvation (often caused by sync-over-async).

---

## 9. A pragmatic optimization checklist
1. Reproduce + measure the slow path.
2. Is it the DB? (most likely) → query/index/N+1.
3. Can the result be cached?
4. Is I/O async and non-blocking?
5. Is the payload (DB rows, JSON, assets) bigger than needed?
6. Re-measure. Stop when "fast enough"; don't micro-optimize blindly.

---

## Common pitfalls
- Optimizing without measuring.
- N+1 queries hiding behind lazy loading.
- Caching without an invalidation plan (stale data bugs).
- Sync-over-async causing thread starvation.
- Stateful servers blocking horizontal scaling.
- Premature micro-optimization while ignoring the DB.

## Practice / interview questions
1. How do you find a performance bottleneck before changing code?
2. Compare in-memory vs distributed caching; when each?
3. What is the cache-aside pattern and cache stampede?
4. Why does async improve scalability (not single-request speed)?
5. What makes an app horizontally scalable?
6. How would you diagnose and fix a slow page?
7. What is thread-pool starvation and a common cause?
8. Strategies to speed up frontend load time?
