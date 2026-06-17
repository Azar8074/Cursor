# Module 14 — Asynchronous Programming (async / await)

Asynchronous programming lets your app stay **responsive** and **scalable** by not blocking
threads while waiting on I/O (network, disk, database).

## Why Async?

- **UI apps**: keep the interface responsive during long operations.
- **Server apps** (ASP.NET): free up threads to handle more requests while waiting on I/O.

Async is about **not blocking a thread while waiting**, not about doing more work in parallel
(that's `Parallel`/threads).

## Task and Task&lt;T&gt;

A `Task` represents an asynchronous operation; `Task<T>` one that returns a value.

```csharp
Task plain = Task.CompletedTask;
Task<int> withResult = Task.FromResult(42);
```

## async / await Basics

```csharp
public async Task<string> DownloadAsync(string url)
{
    using var client = new HttpClient();
    string content = await client.GetStringAsync(url); // non-blocking wait
    return content;
}

// Calling it
public async Task RunAsync()
{
    string html = await DownloadAsync("https://example.com");
    Console.WriteLine(html.Length);
}
```

- `async` marks a method that contains `await` and returns `Task`, `Task<T>`, or `ValueTask`.
- `await` suspends the method until the awaited task completes, then resumes — **without blocking
  the calling thread**.

## Return Types

| Return type | Use |
|-------------|-----|
| `Task` | async method with no return value |
| `Task<T>` | async method returning `T` |
| `ValueTask<T>` | perf-sensitive paths that often complete synchronously |
| `void` | **only** for event handlers — avoid otherwise (can't await or catch) |

```csharp
public async Task DoWorkAsync() { await Task.Delay(100); }
public async Task<int> ComputeAsync() { await Task.Delay(100); return 42; }
private async void Button_Click(object s, EventArgs e) { await DoWorkAsync(); }
```

## Running Tasks Concurrently

```csharp
// Sequential — total time = sum of both
var a = await FetchAsync("url1");
var b = await FetchAsync("url2");

// Concurrent — total time = the longer of the two
Task<string> t1 = FetchAsync("url1");
Task<string> t2 = FetchAsync("url2");
await Task.WhenAll(t1, t2);
string r1 = t1.Result, r2 = t2.Result;

// Or get results array
string[] results = await Task.WhenAll(t1, t2);

// First to finish
Task<string> winner = await Task.WhenAny(t1, t2);
```

## CPU-bound Work: Task.Run

For CPU-intensive work, offload to a thread-pool thread:

```csharp
int result = await Task.Run(() => HeavyComputation());
```

> Don't wrap I/O in `Task.Run` — call the native async I/O API (`...Async`) instead.

## Exception Handling

```csharp
try
{
    await DownloadAsync("https://invalid");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
```

With `Task.WhenAll`, multiple exceptions are aggregated; awaiting rethrows the first. Inspect
`task.Exception` (an `AggregateException`) for all of them.

## Cancellation

```csharp
public async Task ProcessAsync(CancellationToken token)
{
    for (int i = 0; i < 100; i++)
    {
        token.ThrowIfCancellationRequested();
        await Task.Delay(100, token);
    }
}

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
try { await ProcessAsync(cts.Token); }
catch (OperationCanceledException) { Console.WriteLine("Cancelled"); }
```

## Common Pitfalls

### 1. Blocking on async (deadlocks)

```csharp
// ❌ Can deadlock in UI/ASP.NET contexts
string x = DownloadAsync(url).Result;
DownloadAsync(url).Wait();

// ✅ Await all the way up
string y = await DownloadAsync(url);
```

### 2. async void

```csharp
// ❌ Can't await, exceptions crash the process
async void Process() { await Task.Delay(1); }

// ✅ Return Task
async Task ProcessAsync() { await Task.Delay(1); }
```

### 3. Forgetting await

```csharp
SaveAsync();        // ❌ fire-and-forget; exceptions lost, may not finish
await SaveAsync();  // ✅
```

### 4. ConfigureAwait (library code)

```csharp
// In libraries, avoid capturing the sync context for performance
await SomeAsync().ConfigureAwait(false);
```

## Async All the Way

Once you go async, propagate it up the call chain. Don't mix blocking calls (`.Result`, `.Wait()`)
with async code.

```csharp
public async Task<List<Order>> GetOrdersAsync()
{
    await using var conn = new SqlConnection(_connString);
    await conn.OpenAsync();
    // ... await query ...
    return orders;
}
```

## Key Takeaways

- `async`/`await` free the thread during I/O waits — crucial for scalable servers and responsive UIs.
- Return `Task`/`Task<T>`; use `async void` only for event handlers.
- Use `Task.WhenAll` for concurrency; `Task.Run` only for CPU-bound work.
- Never block on async with `.Result`/`.Wait()`; await all the way up. Support cancellation.

➡️ Next: [Module 15 — Files, Streams & Serialization](15-files-and-serialization.md)
