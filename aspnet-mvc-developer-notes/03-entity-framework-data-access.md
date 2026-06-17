# 03 — Entity Framework & Data Access

Most MVC apps talk to a relational database through an ORM. EF is the default,
but the **concepts** (change tracking, loading strategies, the N+1 problem,
transactions) apply to any data layer.

> Covers **EF6** (classic, .NET Framework) and **EF Core** differences.

---

## 1. What an ORM does
Maps relational tables ↔ .NET objects so you write LINQ instead of SQL. It
generates SQL, materializes results into entities, tracks changes, and persists
them. Trade-off: convenience vs control. You must still understand the SQL it
produces.

---

## 2. Modeling approaches
- **Code First** (most common) — define POCO classes, EF creates/maps the DB,
  use **migrations** to evolve schema.
- **Database First** — generate model from an existing DB.
- **Model First** — design in a designer (legacy, rarely used now).

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }       // navigation property
}
```

### Fluent API vs Data Annotations
Configure mapping with attributes (`[Key]`, `[Required]`, `[Table]`) or the
Fluent API in `OnModelCreating` (more powerful, keeps entities clean).

```csharp
protected override void OnModelCreating(ModelBuilder mb)
{
    mb.Entity<Product>(e =>
    {
        e.HasKey(p => p.Id);
        e.Property(p => p.Price).HasPrecision(18, 2);
        e.HasOne(p => p.Category).WithMany(c => c.Products)
         .HasForeignKey(p => p.CategoryId);
    });
}
```

---

## 3. DbContext
- The **unit of work** + identity map. Tracks entities, batches saves.
- **Lifetime: scoped per request** in web apps. Never a singleton; not thread-safe.
- Dispose it (DI handles this in Core).

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
}
```

---

## 4. Change tracking & SaveChanges
EF snapshots loaded entities and detects modifications. `SaveChanges()` writes
inserts/updates/deletes in a transaction.

```csharp
var p = db.Products.First(x => x.Id == 1);
p.Price = 9.99m;          // tracked as Modified
db.SaveChanges();         // UPDATE ... WHERE Id = 1
```

- **No-tracking queries** for read-only data are faster:
  `db.Products.AsNoTracking().ToList()`.

---

## 5. Loading related data (know all three)
| Strategy | How | When |
|----------|-----|------|
| **Eager** | `Include`/`ThenInclude` | You know you need the relation |
| **Lazy** | virtual nav props + proxies | Convenient but causes N+1 |
| **Explicit** | `.Entry(x).Reference/Collection().Load()` | Load on demand, controlled |

```csharp
// Eager
var orders = db.Orders
    .Include(o => o.Customer)
    .Include(o => o.Lines).ThenInclude(l => l.Product)
    .ToList();
```

### The N+1 query problem (must understand)
Lazy loading inside a loop runs one query per item:
```csharp
foreach (var o in db.Orders.ToList())     // 1 query
    Console.WriteLine(o.Customer.Name);   // +1 query EACH iteration => N+1
```
Fix with eager loading (`Include`) or a projection.

### Projection (often the best)
Select only what you need into a DTO/ViewModel — less data, no tracking, single
query:
```csharp
var vm = db.Orders.Select(o => new OrderListItem
{
    Id = o.Id,
    Customer = o.Customer.Name,
    Total = o.Lines.Sum(l => l.Qty * l.Price)
}).ToList();
```

---

## 6. CRUD
```csharp
// Create
db.Products.Add(new Product { Name = "Pen", Price = 1.5m });
await db.SaveChangesAsync();

// Read
var p = await db.Products.FindAsync(id);

// Update
p.Price = 2.0m;
await db.SaveChangesAsync();

// Delete
db.Products.Remove(p);
await db.SaveChangesAsync();
```
Prefer the **async** variants (`ToListAsync`, `FirstOrDefaultAsync`,
`SaveChangesAsync`) in web apps.

---

## 7. Migrations (Code First)
```bash
# EF Core CLI
dotnet ef migrations add AddProductPrice
dotnet ef database update

# EF6 (Package Manager Console)
Add-Migration AddProductPrice
Update-Database
```
- Review the generated migration before applying.
- Keep migrations in source control; never edit applied ones.
- Have a plan for production (idempotent scripts, `dotnet ef migrations script`).

---

## 8. Transactions & concurrency
- `SaveChanges` is itself transactional. For multiple operations:
```csharp
using var tx = db.Database.BeginTransaction();
try { /* ... */ db.SaveChanges(); tx.Commit(); }
catch { tx.Rollback(); throw; }
```
- **Optimistic concurrency** — add a `[Timestamp] byte[] RowVersion` /
  concurrency token; EF throws `DbUpdateConcurrencyException` on conflict.
- **Pessimistic** — DB locks (rare with EF; use sparingly).

---

## 9. Raw SQL & stored procedures
When LINQ isn't enough (complex queries, performance):
```csharp
// EF Core
var products = db.Products
    .FromSqlInterpolated($"SELECT * FROM Products WHERE Price > {min}")
    .ToList();

db.Database.ExecuteSqlInterpolated($"UPDATE Products SET Price = Price * {1.1}");
```
Use parameterized/interpolated forms to avoid **SQL injection**. Never string-
concatenate user input into SQL.

---

## 10. Performance tips
- Use `AsNoTracking()` for reads.
- Project to DTOs; don't pull whole entities/graphs you don't need.
- Avoid N+1 (Include or project).
- Page large results (`Skip`/`Take`).
- Watch for client-side evaluation (operations EF can't translate run in memory).
- Batch where possible; consider `ExecuteUpdate`/`ExecuteDelete` (EF Core 7+).
- Profile the actual SQL (EF logging, SQL Profiler, `ToQueryString()`).
- Add proper DB **indexes** (see file 06).

---

## 11. Repository & Unit of Work — do you need them?
`DbContext` already implements unit-of-work and `DbSet<T>` is a repository.
Adding another layer can help testability/abstraction but often adds ceremony.
Decide deliberately; don't cargo-cult. (See file 09.)

---

## Common pitfalls
- Lazy loading causing N+1 in views/loops.
- Long-lived / shared `DbContext` (threading bugs, memory growth).
- Tracking entities you only read.
- Returning `IQueryable` out of the data layer (query runs after context disposed).
- String-concatenated SQL → injection.
- Forgetting `await` on async DB calls.

## Practice / interview questions
1. Explain the N+1 problem and three ways to fix it.
2. Eager vs lazy vs explicit loading — when each?
3. What lifetime should `DbContext` have in a web app and why?
4. How does EF change tracking work?
5. How do you implement optimistic concurrency?
6. When would you drop to raw SQL or a stored procedure?
7. What does `AsNoTracking()` do and when do you use it?
8. How do migrations work and how do you apply them safely in production?
