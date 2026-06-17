# 02 — SignalR (Real-Time Communication)

**SignalR** adds real-time, server→client push to .NET apps. Instead of the
client polling for updates, the server pushes them instantly over a persistent
connection. Think chat, live dashboards, notifications, collaborative editing,
progress bars, and live sports/stock tickers.

---

## 1. Why & when to use it
- The server needs to **push** data to clients without them asking.
- Many clients need the **same live updates** (broadcast).
- Bi-directional, low-latency messaging.

**When *not* to:** simple request/response, infrequent updates (a periodic AJAX
refresh may be simpler), or one-way server-to-server (use a queue).

---

## 2. How it works (transports)
SignalR negotiates the best available transport and falls back automatically:
1. **WebSockets** (preferred — full duplex, lowest overhead).
2. **Server-Sent Events**.
3. **Long polling** (last resort).

You code against the SignalR abstraction; it handles transport, reconnection,
and message framing.

---

## 3. Install
ASP.NET Core has SignalR built in (`Microsoft.AspNetCore.SignalR`). For the
JS client:
```bash
npm install @microsoft/signalr
# or use the CDN / LibMan
```
(Classic ASP.NET MVC 5 uses the older `Microsoft.AspNet.SignalR` package — the
API is similar but this guide targets ASP.NET Core.)

---

## 4. Server: a Hub
A **Hub** is the server-side endpoint clients call and that pushes to clients.
```csharp
public class ChatHub : Hub
{
    // Called BY clients
    public async Task SendMessage(string user, string message)
    {
        // Push TO all connected clients
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("ReceiveMessage", "System", "Welcome!");
        await base.OnConnectedAsync();
    }
}
```

```csharp
// Program.cs
builder.Services.AddSignalR();
...
app.MapHub<ChatHub>("/chathub");
```

---

## 5. Client (JavaScript)
```javascript
import * as signalR from "@microsoft/signalr";

const conn = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .withAutomaticReconnect()
    .build();

conn.on("ReceiveMessage", (user, message) => {
    const li = document.createElement("li");
    li.textContent = `${user}: ${message}`;
    document.getElementById("messages").appendChild(li);
});

await conn.start();
document.getElementById("send").onclick = () =>
    conn.invoke("SendMessage", currentUser, input.value);
```

There are also **typed .NET clients** (`Microsoft.AspNetCore.SignalR.Client`)
for desktop/service-to-service and a Blazor integration.

---

## 6. Targeting specific clients
```csharp
Clients.All                          // everyone
Clients.Caller                       // the invoking client
Clients.Others                       // everyone except caller
Clients.Client(connectionId)         // one connection
Clients.User(userId)                 // all connections of a logged-in user
Clients.Group("room-42")             // a group
Clients.GroupExcept("room-42", ids)  // a group minus some
```

### Groups (e.g. chat rooms)
```csharp
public Task JoinRoom(string room) =>
    Groups.AddToGroupAsync(Context.ConnectionId, room);

public Task SendToRoom(string room, string msg) =>
    Clients.Group(room).SendAsync("ReceiveMessage", Context.User?.Identity?.Name, msg);
```

---

## 7. Pushing from outside a Hub (very common)
Inject `IHubContext<THub>` anywhere (a controller, background job, service) to
broadcast:
```csharp
public class OrdersController(IHubContext<NotificationsHub> hub) : Controller
{
    public async Task<IActionResult> Place(OrderVm vm)
    {
        // ... create order ...
        await hub.Clients.User(vm.SellerId)
            .SendAsync("OrderPlaced", new { vm.OrderId, vm.Total });
        return Ok();
    }
}
```

---

## 8. Auth & users
- Hubs respect `[Authorize]`.
- `Context.User` gives the authenticated principal; `Clients.User(id)` maps to a
  user via the `IUserIdProvider`.
- For bearer tokens over WebSockets, pass the token via the query string and
  configure JWT events (WebSockets can't send custom headers).

---

## 9. Scaling out (critical for production)
With multiple servers, a client connected to server A won't get messages sent
from server B — unless you add a **backplane**:
- **Redis backplane** (`Microsoft.AspNetCore.SignalR.StackExchangeRedis`) — fans
  messages out across all servers.
- **Azure SignalR Service** — fully managed; offloads connections entirely
  (recommended on Azure for large scale).

```csharp
builder.Services.AddSignalR()
    .AddStackExchangeRedis("localhost:6379");
```
Also require **sticky sessions** on the load balancer (unless using Azure SignalR
Service), because the connection is stateful.

---

## Pitfalls & gotchas
- Forgetting a backplane when scaling out → missed messages.
- Putting heavy/blocking work in hub methods (they should be fast; offload to a
  queue/background job).
- Storing per-connection state in memory (lost on reconnect/scale).
- Not handling reconnection on the client (`withAutomaticReconnect`).
- Sending large payloads frequently (bandwidth/CPU).
- Auth over WebSockets needing query-string token handling.

## Interview questions
1. What problem does SignalR solve vs polling?
2. What transports does it use and how does fallback work?
3. Difference between `Clients.All`, `Clients.Caller`, `Clients.Group`?
4. How do you push to clients from a controller or background job?
5. Why and how do you scale SignalR across multiple servers?
6. How do groups work (e.g. chat rooms)?
7. How do you authenticate a SignalR connection?
8. When would you NOT use SignalR?
