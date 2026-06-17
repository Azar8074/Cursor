# 01 — Redis (Distributed Cache & More)

**Redis** is an in-memory, key-value data store. Blazingly fast, it's the default
choice for distributed caching, sessions, rate limiting, pub/sub, leaderboards,
and distributed locks in .NET apps that run on more than one server.

---

## 1. Why & when to use it
- You run **multiple app instances** (load-balanced) and need a **shared** cache
  (in-memory `IMemoryCache` lives in one process only).
- Expensive/repeated reads (reference data, computed results).
- Session state for a stateless, horizontally-scaled app.
- Rate limiting, distributed locks, real-time leaderboards, pub/sub.

**When *not* to:** single-server apps with small caches (use `IMemoryCache`);
as your primary durable database (Redis is memory-first — persistence exists but
it's a cache/data-structure server, not a system of record).

---

## 2. Data types you'll use
- **String** — simple values, counters (`INCR`).
- **Hash** — object-like field/value maps.
- **List** — queues/stacks.
- **Set / Sorted Set** — uniqueness, leaderboards (score-ordered).
- **Pub/Sub** channels; **Streams** for log-like data.
- Every key can have a **TTL** (expiry) — core to caching.

---

## 3. Install (.NET)
```bash
# Recommended low-level client
dotnet add package StackExchange.Redis
# OR the ASP.NET Core distributed cache abstraction backed by Redis
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

Run Redis locally:
```bash
docker run -d --name redis -p 6379:6379 redis:7
```

---

## 4. Option A — `IDistributedCache` (simplest)
```csharp
// Program.cs
builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = builder.Configuration.GetConnectionString("Redis"); // "localhost:6379"
    o.InstanceName = "myapp:";
});
```

```csharp
public class ProductService(IDistributedCache cache, IProductRepository repo)
{
    public async Task<ProductDto?> GetAsync(int id)
    {
        var key = $"product:{id}";
        var cached = await cache.GetStringAsync(key);
        if (cached is not null)
            return JsonSerializer.Deserialize<ProductDto>(cached);

        var product = await repo.GetByIdAsync(id);
        if (product is null) return null;

        await cache.SetStringAsync(key, JsonSerializer.Serialize(product),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
        return product;
    }
}
```
This is the **cache-aside** pattern: check cache → on miss, load + store.

---

## 5. Option B — `StackExchange.Redis` directly (full power)
```csharp
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
```

```csharp
public class LeaderboardService(IConnectionMultiplexer mux)
{
    private IDatabase Db => mux.GetDatabase();

    public Task AddScoreAsync(string user, double score) =>
        Db.SortedSetAddAsync("leaderboard", user, score);

    public async Task<string[]> TopAsync(int n) =>
        (await Db.SortedSetRangeByRankAsync("leaderboard", 0, n - 1, Order.Descending))
            .Select(v => v.ToString()).ToArray();
}
```
> Register the `IConnectionMultiplexer` as a **singleton** — it's thread-safe,
> expensive to create, and multiplexes all commands over one connection.

---

## 6. Common real-world patterns
### Cache-aside (most common)
Load on miss, store with TTL, invalidate/update on writes.

### Output / response caching
Cache whole responses at the edge or app layer (ASP.NET Core output caching can
use Redis).

### Distributed lock
Prevent two servers doing the same job at once:
```csharp
var token = Guid.NewGuid().ToString();
if (await Db.LockTakeAsync("lock:report", token, TimeSpan.FromSeconds(30)))
{
    try { /* exclusive work */ }
    finally { await Db.LockReleaseAsync("lock:report", token); }
}
```
(For robust locking across a Redis cluster, look at **RedLock.net**.)

### Rate limiting
`INCR` a per-user key with a short TTL; reject when over the threshold.

### Pub/Sub
```csharp
var sub = mux.GetSubscriber();
await sub.SubscribeAsync(RedisChannel.Literal("orders"), (ch, msg) => Handle(msg));
await sub.PublishAsync(RedisChannel.Literal("orders"), "order-123-created");
```
(SignalR can also use a **Redis backplane** to scale out — see file 02.)

---

## 7. Cache invalidation strategies
- **TTL/expiry** — simplest; tolerate slightly stale data.
- **Write-through / on-write invalidation** — update or delete the key when the
  source changes.
- **Versioned keys** — bump a version to invalidate a whole group.
- Beware **cache stampede** (many simultaneous misses) — use locks/`GetOrCreate`,
  or slightly randomized TTLs ("jitter").

---

## 8. Production considerations
- **Persistence:** RDB snapshots vs AOF; decide if you need durability.
- **High availability:** Redis Sentinel or **Redis Cluster**; managed options
  (Azure Cache for Redis, AWS ElastiCache, Redis Cloud).
- **Eviction policy:** `maxmemory` + policy (`allkeys-lru`, `volatile-ttl`, …).
- **Security:** require auth (ACL/password), TLS in transit, network isolation.
- **Serialization:** JSON is easy; MessagePack/Protobuf are smaller/faster.
- **Monitor:** hit ratio, memory, evictions, latency, connected clients.

---

## Pitfalls & gotchas
- Creating a new `ConnectionMultiplexer` per request (it should be a singleton).
- Caching without expiry/invalidation → stale data bugs.
- Storing huge objects/blobs (network + memory cost).
- Treating Redis as a durable primary database.
- Ignoring serialization cost/size.
- Cache stampede on hot keys after expiry.

## Interview questions
1. When do you use Redis over `IMemoryCache`?
2. Explain the cache-aside pattern.
3. Why must `IConnectionMultiplexer` be a singleton?
4. How would you implement a distributed lock / rate limiter with Redis?
5. What is cache stampede and how do you prevent it?
6. Eviction policies — what does `allkeys-lru` mean?
7. How do you keep cached data consistent with the database?
8. How do you scale Redis for HA?
