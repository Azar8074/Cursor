# 06 — SQL & Databases

Even with an ORM, the database is where most real-world performance and
correctness problems live. A senior MVC dev reads execution plans, designs
schemas, and writes solid SQL.

---

## 1. Relational fundamentals
- **Tables, rows, columns**; **primary keys**, **foreign keys**, constraints
  (`NOT NULL`, `UNIQUE`, `CHECK`, `DEFAULT`).
- **Relationships:** one-to-one, one-to-many, many-to-many (join table).
- **Normalization** (1NF→3NF): remove redundancy, avoid update anomalies.
  Know when to **denormalize** for read performance.
- **ACID** (Atomicity, Consistency, Isolation, Durability).

---

## 2. Core SQL you must know
```sql
SELECT p.Name, c.Name AS Category, p.Price
FROM   Products p
JOIN   Categories c ON c.Id = p.CategoryId
WHERE  p.Price > 10
ORDER  BY p.Price DESC;
```

- **Joins:** INNER, LEFT/RIGHT OUTER, FULL, CROSS, self-join.
- **Aggregation:** `GROUP BY`, `HAVING`, `COUNT/SUM/AVG/MIN/MAX`.
- **Filtering:** `WHERE`, `IN`, `BETWEEN`, `LIKE`, `IS NULL`.
- **Set ops:** `UNION`/`UNION ALL`, `INTERSECT`, `EXCEPT`.
- **Subqueries** & **CTEs** (`WITH`), correlated subqueries.
- **Window functions:** `ROW_NUMBER()`, `RANK()`, `SUM() OVER (PARTITION BY ...)`
  — extremely useful for analytics & paging.
- **Paging:** `OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY`.

```sql
-- Top order per customer using a window function
WITH ranked AS (
  SELECT *, ROW_NUMBER() OVER (PARTITION BY CustomerId ORDER BY Total DESC) rn
  FROM Orders)
SELECT * FROM ranked WHERE rn = 1;
```

---

## 3. DDL & DML
- **DDL:** `CREATE`, `ALTER`, `DROP` (schema).
- **DML:** `INSERT`, `UPDATE`, `DELETE`, `MERGE`.
- **TCL:** `BEGIN TRAN`, `COMMIT`, `ROLLBACK`.

```sql
CREATE TABLE Products (
  Id        INT IDENTITY PRIMARY KEY,
  Name      NVARCHAR(100) NOT NULL,
  Price     DECIMAL(18,2) NOT NULL DEFAULT 0,
  CategoryId INT NOT NULL REFERENCES Categories(Id)
);
```

---

## 4. Indexing (the #1 performance lever)
- **Clustered index** — physically orders the table (usually the PK). One per table.
- **Non-clustered index** — separate structure pointing to rows. Many allowed.
- **Composite index** — multiple columns; order matters (left-most prefix).
- **Covering index** — `INCLUDE` columns so the query is served from the index.
- Indexes speed reads but slow writes and use storage — index intentionally.
- Watch for **missing index** suggestions and **unused** indexes.

```sql
CREATE NONCLUSTERED INDEX IX_Products_Category_Price
ON Products (CategoryId, Price) INCLUDE (Name);
```

> Rule of thumb: index columns used in `WHERE`, `JOIN`, and `ORDER BY`.

---

## 5. Reading an execution plan
- Look for **table/index scans** vs **seeks** (seeks are usually better for
  selective queries).
- **Key lookups** can indicate a missing covering index.
- High-cost operators, big row estimates vs actuals (stale statistics).
- Use `SET STATISTICS IO, TIME ON`, SSMS "Include Actual Execution Plan".

---

## 6. Transactions & isolation levels
| Level | Dirty read | Non-repeatable | Phantom |
|-------|-----------|----------------|---------|
| Read Uncommitted | ✅ | ✅ | ✅ |
| Read Committed (default) | ❌ | ✅ | ✅ |
| Repeatable Read | ❌ | ❌ | ✅ |
| Serializable | ❌ | ❌ | ❌ |
| Snapshot | ❌ | ❌ | ❌ (row-versioned) |

- Higher isolation = more locks/contention. Pick the lowest that's correct.
- Beware **deadlocks**; keep transactions short, access objects in consistent order.

---

## 7. Stored procedures, views, functions
- **Stored procedures** — precompiled, parameterized; good for complex/batch
  logic and security boundaries.
- **Views** — saved queries; simplify access, can be indexed.
- **Functions** — scalar/table-valued; beware scalar UDFs killing performance.
- **Triggers** — run on DML; powerful but can hide logic — use sparingly.

```sql
CREATE PROCEDURE usp_GetOrders @CustomerId INT AS
BEGIN
  SET NOCOUNT ON;
  SELECT * FROM Orders WHERE CustomerId = @CustomerId;
END
```

---

## 8. Performance & troubleshooting
- **Parameterize** queries (plan reuse + SQL-injection safety).
- Avoid `SELECT *` in production code.
- Avoid functions on indexed columns in `WHERE` (`WHERE YEAR(d)=2024` → non-SARGable).
- Keep statistics fresh; rebuild/reorganize fragmented indexes.
- Detect EF-generated bad SQL via logging / `ToQueryString()` / SQL Profiler /
  Extended Events.
- Consider read replicas, partitioning, and caching for scale (see file 10).

---

## 9. NoSQL awareness
Know when relational isn't the right fit:
- **Document** (MongoDB, Cosmos DB) — flexible schema, denormalized aggregates.
- **Key-value / cache** (Redis) — sessions, caching, leaderboards.
- **Search** (Elasticsearch) — full-text/faceted search.
- Choose based on access patterns, consistency needs, and scale. Polyglot
  persistence is common.

---

## Common pitfalls
- No indexes (or too many).
- N+1 queries from the ORM (file 03).
- Non-SARGable predicates defeating indexes.
- `SELECT *` and pulling unneeded columns/rows.
- Long-running transactions causing locks/deadlocks.
- String-concatenated SQL → injection (file 07).

## Practice / interview questions
1. Clustered vs non-clustered index?
2. What makes a query SARGable? Give a non-SARGable example.
3. Explain the isolation levels and the anomalies they prevent.
4. INNER vs LEFT JOIN — show results differ.
5. What is a covering index?
6. How would you find and fix a slow query?
7. When would you choose NoSQL over a relational DB?
8. Write a query for the 2nd page (20/page) of products ordered by price.
