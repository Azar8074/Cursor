# 05 — Message Queues & Brokers (RabbitMQ / Azure Service Bus / Kafka)

Message queues let parts of a system communicate **asynchronously** by passing
messages, instead of calling each other directly. This decouples services,
smooths out load spikes, and makes systems more resilient and scalable.

---

## 1. Why & when to use them
- **Decouple** producers from consumers (they don't need to be online at the same
  time).
- **Smooth load** — absorb bursts; consumers process at their own pace.
- **Reliability** — messages persist until processed; retry on failure.
- **Scale** — add more consumers to process faster.
- **Integration** — connect microservices or external systems via events.

**When *not* to:** simple in-process background work (Hangfire, file 04) or when
you need an immediate synchronous response.

---

## 2. Core concepts
- **Producer / Publisher** — sends messages.
- **Consumer / Subscriber** — receives & processes them.
- **Queue** — point-to-point; each message goes to **one** consumer (work
  distribution).
- **Topic / Exchange / Pub-Sub** — one message fans out to **many** subscribers
  (events).
- **Message** — payload + metadata (headers, correlation id).
- **Acknowledgement (ack/nack)** — consumer confirms processing; unacked messages
  are redelivered.
- **Dead-letter queue (DLQ)** — where messages go after repeated failures.

### Delivery guarantees
- **At-most-once** — may lose messages.
- **At-least-once** — may duplicate (most common) → make consumers **idempotent**.
- **Exactly-once** — hard/expensive; usually emulated via idempotency + dedup.

---

## 3. The three you'll meet
| Broker | Style | Best for |
|--------|-------|----------|
| **RabbitMQ** | Traditional broker (AMQP); exchanges/queues, flexible routing | General-purpose task queues, microservice messaging |
| **Azure Service Bus** | Managed enterprise broker; queues + topics/subscriptions, sessions, transactions | .NET/Azure apps, reliable enterprise messaging |
| **Apache Kafka** | Distributed **log/stream**; high throughput, replayable, retains messages | Event streaming, analytics, high-volume pipelines, event sourcing |

> RabbitMQ/Service Bus = "smart broker, dumb consumer" (broker routes, tracks
> acks). Kafka = "dumb broker, smart consumer" (consumers track their own offset;
> messages are retained and replayable).

---

## 4. RabbitMQ — minimal example
```bash
docker run -d --name rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
dotnet add package RabbitMQ.Client
```

```csharp
// Producer
var factory = new ConnectionFactory { HostName = "localhost" };
using var conn = factory.CreateConnection();
using var channel = conn.CreateModel();
channel.QueueDeclare("orders", durable: true, exclusive: false, autoDelete: false);

var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(order));
var props = channel.CreateBasicProperties();
props.Persistent = true;   // survive broker restart
channel.BasicPublish(exchange: "", routingKey: "orders", basicProperties: props, body: body);
```

```csharp
// Consumer
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (_, ea) =>
{
    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
    var order = JsonSerializer.Deserialize<Order>(json);
    try { Process(order); channel.BasicAck(ea.DeliveryTag, false); }
    catch { channel.BasicNack(ea.DeliveryTag, false, requeue: true); }
};
channel.BasicConsume("orders", autoAck: false, consumer);
```

---

## 5. Azure Service Bus — minimal example
```bash
dotnet add package Azure.Messaging.ServiceBus
```
```csharp
// Send
await using var client = new ServiceBusClient(connectionString);
var sender = client.CreateSender("orders");
await sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(order)));

// Receive (processor)
var processor = client.CreateProcessor("orders", new ServiceBusProcessorOptions());
processor.ProcessMessageAsync += async args =>
{
    var order = JsonSerializer.Deserialize<Order>(args.Message.Body.ToString());
    await Handle(order);
    await args.CompleteMessageAsync(args.Message);   // ack
};
processor.ProcessErrorAsync += args => { /* log */ return Task.CompletedTask; };
await processor.StartProcessingAsync();
```
- **Topics + subscriptions** give pub/sub. **Sessions** give ordered processing.

---

## 6. Use a higher-level library: MassTransit
Writing raw broker code is tedious. **MassTransit** (or **NServiceBus**)
abstracts brokers and adds retries, sagas, scheduling, outbox, and consumers.
```bash
dotnet add package MassTransit.RabbitMQ
```
```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer>();
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("localhost");
        cfg.ConfigureEndpoints(ctx);
    });
});

public record OrderPlaced(int OrderId, decimal Total);

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> ctx)
        => await HandleAsync(ctx.Message.OrderId);
}

// publish
await _publishEndpoint.Publish(new OrderPlaced(order.Id, order.Total));
```

---

## 7. Reliability patterns (important)
- **Idempotent consumers** — handle duplicates safely (track processed message
  ids).
- **Dead-letter queues** — isolate "poison" messages after N failures; alert &
  inspect.
- **Retry with backoff** — transient failures; avoid hot-looping.
- **Outbox pattern** — write the message and the DB change in one transaction to
  avoid "saved but not published" (or vice versa). MassTransit supports this.
- **Correlation IDs** — trace a message across services.
- **Ordering** — usually not guaranteed; use sessions/partitions if you need it.

---

## 8. Choosing & operating
- Throughput/replay/streaming → **Kafka**. Enterprise + Azure → **Service Bus**.
  Flexible general broker on-prem/anywhere → **RabbitMQ**.
- Monitor: queue depth, consumer lag, DLQ size, processing latency.
- Secure: TLS, auth, least-privilege topic/queue permissions.

---

## Pitfalls & gotchas
- Assuming exactly-once delivery (design for at-least-once + idempotency).
- No dead-letter handling → poison messages block/loop forever.
- Auto-ack before processing finishes → message loss on crash.
- Giant messages (put blobs in storage, send a reference).
- Expecting ordering when the broker doesn't guarantee it.
- The dual-write problem (DB + publish) — use the outbox pattern.

## Interview questions
1. Queue vs topic (point-to-point vs pub/sub)?
2. Compare RabbitMQ, Azure Service Bus, and Kafka.
3. What are at-most/at-least/exactly-once delivery?
4. Why must consumers be idempotent?
5. What is a dead-letter queue?
6. Explain the outbox pattern and the problem it solves.
7. How does Kafka differ architecturally (offsets, retention, replay)?
8. When do you use a message queue vs Hangfire vs a direct API call?
