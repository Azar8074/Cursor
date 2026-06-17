# Module 08 — Entity Framework & Data Access

**Entity Framework (EF)** is Microsoft's **ORM** (Object-Relational Mapper). It lets you work with
a database using C# objects and LINQ instead of raw SQL. EF Core is the modern, cross-platform
version; EF6 is the .NET Framework version used with MVC 5.

## Approaches

| Approach | Description |
|----------|-------------|
| **Code First** | Define C# classes → EF generates the database (most popular). |
| **Database First** | Generate classes from an existing database. |
| **Model First** | Design in a visual designer → generate both (legacy). |

## Code First: Entities

```csharp
public class Product
{
    public int Id { get; set; }                 // convention: 'Id' = primary key
    public string Name { get; set; }
    public decimal Price { get; set; }

    public int CategoryId { get; set; }         // foreign key
    public Category Category { get; set; }      // navigation property
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Product> Products { get; set; } // one-to-many
}
```

## The DbContext

The `DbContext` is the gateway to the database — it represents a session/unit of work.

```csharp
using Microsoft.EntityFrameworkCore;   // EF Core (EF6: System.Data.Entity)

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        builder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);
    }
}
```

### Registering it (ASP.NET Core)

```csharp
// Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
```

```json
// appsettings.json
{
  "ConnectionStrings": {
    "Default": "Server=.;Database=ShopDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

## Migrations

Migrations evolve the database schema as your model changes.

```bash
# EF Core CLI
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet ef migrations remove          # undo last (if not applied)
dotnet ef database update <Name>     # migrate to a specific migration
```

```powershell
# EF6 / Package Manager Console
Enable-Migrations
Add-Migration InitialCreate
Update-Database
```

## CRUD Operations

```csharp
public class ProductService
{
    private readonly AppDbContext _db;
    public ProductService(AppDbContext db) => _db = db;

    // CREATE
    public async Task AddAsync(Product p)
    {
        _db.Products.Add(p);
        await _db.SaveChangesAsync();
    }

    // READ
    public async Task<List<Product>> GetAllAsync() =>
        await _db.Products.Include(p => p.Category).ToListAsync();

    public async Task<Product?> GetByIdAsync(int id) =>
        await _db.Products.FindAsync(id);

    public async Task<List<Product>> SearchAsync(string term) =>
        await _db.Products
            .Where(p => p.Name.Contains(term))
            .OrderBy(p => p.Name)
            .ToListAsync();

    // UPDATE
    public async Task UpdateAsync(Product p)
    {
        _db.Products.Update(p);          // or modify a tracked entity
        await _db.SaveChangesAsync();
    }

    // DELETE
    public async Task DeleteAsync(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p != null)
        {
            _db.Products.Remove(p);
            await _db.SaveChangesAsync();
        }
    }
}
```

## Querying with LINQ

```csharp
var expensive = await _db.Products
    .Where(p => p.Price > 100)
    .OrderByDescending(p => p.Price)
    .Select(p => new { p.Name, p.Price })   // projection
    .ToListAsync();

var count   = await _db.Products.CountAsync();
var any     = await _db.Products.AnyAsync(p => p.Price > 1000);
var first   = await _db.Products.FirstOrDefaultAsync(p => p.Id == 5);
var paged   = await _db.Products.Skip(20).Take(10).ToListAsync();
```

## Loading Related Data

```csharp
// Eager loading — load related data up front
var products = await _db.Products.Include(p => p.Category).ToListAsync();
var deep = await _db.Orders
    .Include(o => o.Lines)
        .ThenInclude(l => l.Product)
    .ToListAsync();

// Explicit loading
_db.Entry(product).Reference(p => p.Category).Load();

// Lazy loading — loads on first access (requires proxies; can cause N+1 queries)
```

> **N+1 problem:** lazy-loading inside a loop fires a query per item. Use `Include` (eager
> loading) to fetch related data in one query.

## Tracking vs No-Tracking

```csharp
// Read-only queries: skip change tracking for performance
var readonly = await _db.Products.AsNoTracking().ToListAsync();
```

## Repository & Unit of Work Patterns

A common abstraction over `DbContext` for testability and separation:

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}
```

> Note: `DbContext` already implements unit-of-work and `DbSet<T>` is a repository, so add these
> abstractions only when they earn their keep (e.g., for swapping data sources or mocking).

## Transactions & Concurrency

```csharp
using var tx = await _db.Database.BeginTransactionAsync();
try
{
    // multiple SaveChanges...
    await tx.CommitAsync();
}
catch
{
    await tx.RollbackAsync();
    throw;
}
```

Optimistic concurrency: add a `[Timestamp] public byte[] RowVersion { get; set; }` and handle
`DbUpdateConcurrencyException`.

## Key Takeaways

- EF is an ORM that maps C# classes to tables; **Code First** + **migrations** is the common workflow.
- `DbContext` + `DbSet<T>` are your data gateway; query with LINQ, persist with `SaveChanges`.
- Use **eager loading** (`Include`) to avoid the N+1 problem; `AsNoTracking` for read-only queries.
- Prefer async methods (`ToListAsync`, `SaveChangesAsync`) in web apps for scalability.

➡️ Next: [Module 09 — State Management](09-state-management.md)
