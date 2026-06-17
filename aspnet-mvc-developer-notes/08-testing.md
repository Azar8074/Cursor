# 08 — Testing

Tests give you the confidence to change code without fear. A senior developer
writes testable code and maintains a healthy test suite.

---

## 1. The testing pyramid
```
        /\        E2E / UI (few, slow, brittle)
       /  \
      /----\      Integration (some, medium)
     /      \
    /--------\    Unit tests (many, fast, isolated)
```
- **Unit** — one class/method in isolation, dependencies mocked. Fast.
- **Integration** — multiple components together (controller + DB + EF).
- **End-to-end** — the whole app via the UI/API (Selenium, Playwright).

Aim for a wide base of fast unit tests, fewer integration, fewest E2E.

---

## 2. Frameworks & tools (.NET)
- **Test runners:** xUnit (popular), NUnit, MSTest.
- **Mocking:** Moq, NSubstitute, FakeItEasy.
- **Assertions:** built-in or **FluentAssertions** (readable).
- **Integration:** `WebApplicationFactory<T>` (Core) for in-memory hosting;
  EF Core InMemory / SQLite in-memory; Testcontainers for real DBs.
- **Coverage:** Coverlet + ReportGenerator.

---

## 3. Anatomy of a unit test (AAA)
```csharp
public class PriceCalculatorTests
{
    [Fact]
    public void Applies_discount_for_premium_customers()
    {
        // Arrange
        var calc = new PriceCalculator();

        // Act
        var result = calc.Total(amount: 100m, isPremium: true);

        // Assert
        result.Should().Be(90m);   // 10% off
    }

    [Theory]
    [InlineData(100, false, 100)]
    [InlineData(100, true, 90)]
    public void Calculates_total(decimal amount, bool premium, decimal expected)
        => new PriceCalculator().Total(amount, premium).Should().Be(expected);
}
```

---

## 4. Testing controllers (why DI matters)
Inject dependencies so you can mock them. Test that actions return the right
result and behave correctly — not the framework itself.

```csharp
[Fact]
public async Task Details_returns_NotFound_when_missing()
{
    var repo = new Mock<IProductRepository>();
    repo.Setup(r => r.GetByIdAsync(42)).ReturnsAsync((Product?)null);
    var controller = new ProductsController(repo.Object);

    var result = await controller.Details(42);

    result.Should().BeOfType<NotFoundResult>();
}
```

> Hard-to-test code (static calls, `new`-ing dependencies, `DateTime.Now`,
> `HttpContext` access) is a design smell. Inject abstractions
> (`IClock`, services) instead.

---

## 5. Integration testing (Core)
```csharp
public class OrdersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public OrdersApiTests(WebApplicationFactory<Program> f) => _client = f.CreateClient();

    [Fact]
    public async Task Get_orders_returns_200()
    {
        var resp = await _client.GetAsync("/api/orders");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```
Use a test database (SQLite in-memory or Testcontainers) and seed known data.

---

## 6. Good test qualities (FIRST)
- **Fast** — milliseconds, run on every save.
- **Isolated** — no shared state, order-independent.
- **Repeatable** — same result every time (no clock/network flakiness).
- **Self-validating** — pass/fail, no manual inspection.
- **Timely** — written close to the code (ideally TDD).

Other habits:
- Test **behavior**, not implementation details.
- One logical assert per test; descriptive names (`Method_State_Expected`).
- Don't mock what you don't own excessively; prefer testing real collaborators
  in integration tests.

---

## 7. TDD (Test-Driven Development)
Red → Green → Refactor: write a failing test, make it pass simply, then clean up.
Great for well-specified logic; pragmatic teams mix TDD with test-after.

---

## 8. What to test (and what not)
- ✅ Business logic, edge cases, validation, mapping, bug regressions.
- ✅ Critical paths via integration tests.
- 🟡 Controllers (thin) — light tests; push logic to services.
- ❌ The framework, trivial getters/setters, third-party libs.

---

## Common pitfalls
- Tests coupled to implementation → break on every refactor.
- Slow/flaky tests (real network, `Thread.Sleep`, shared DB).
- No tests for the bug you just fixed (write a regression test).
- Mocking everything, testing nothing meaningful.
- Untestable design (static/`new` dependencies) — fix with DI.

## Practice / interview questions
1. Explain the testing pyramid and why the base is widest.
2. Unit vs integration test — give an MVC example of each.
3. What is mocking and when is it appropriate?
4. How do you make a controller testable?
5. What does FIRST stand for?
6. How do you test code that depends on `DateTime.Now`?
7. What is TDD and what's the red-green-refactor cycle?
8. How would you integration-test an API endpoint in ASP.NET Core?
