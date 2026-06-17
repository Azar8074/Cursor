# Module 11 — Authentication & Authorization

- **Authentication** = *Who are you?* (verifying identity).
- **Authorization** = *What are you allowed to do?* (granting access).

## ASP.NET Identity

**ASP.NET Identity** is the membership system for managing users, passwords, roles, claims, and
external logins (Google, Facebook, etc.). It works with EF to store users in your database.

### Core building blocks

| Type | Role |
|------|------|
| `IdentityUser` / `ApplicationUser` | the user entity |
| `IdentityRole` | a role |
| `UserManager<TUser>` | create/find/update users, passwords |
| `RoleManager<TRole>` | manage roles |
| `SignInManager<TUser>` | sign in/out, password sign-in |

### Setup (ASP.NET Core)

```csharp
// Program.cs
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(connString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.SignIn.RequireConfirmedAccount = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();
app.UseAuthentication();   // who are you?  (must come before UseAuthorization)
app.UseAuthorization();    // what can you do?
```

## Registering & Signing In

```csharp
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;

    public AccountController(UserManager<ApplicationUser> users,
                            SignInManager<ApplicationUser> signIn)
    {
        _users = users;
        _signIn = signIn;
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = new ApplicationUser { UserName = vm.Email, Email = vm.Email };
        var result = await _users.CreateAsync(user, vm.Password);
        if (result.Succeeded)
        {
            await _signIn.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }
        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        var result = await _signIn.PasswordSignInAsync(
            vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded) return RedirectToAction("Index", "Home");
        ModelState.AddModelError("", "Invalid login");
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
```

## Authorization with [Authorize]

```csharp
[Authorize]                              // any authenticated user
public class DashboardController : Controller { }

[Authorize(Roles = "Admin")]            // must be in the Admin role
public IActionResult AdminPanel() => View();

[Authorize(Roles = "Admin,Manager")]    // any of these roles
public IActionResult Reports() => View();

[AllowAnonymous]                         // exempt a specific action
public IActionResult PublicPage() => View();
```

## Authentication Schemes

| Scheme | Use |
|--------|-----|
| **Cookie authentication** | traditional server-rendered web apps |
| **JWT Bearer tokens** | Web APIs / SPAs / mobile |
| **OAuth / OpenID Connect** | external/social logins, SSO |
| **Windows authentication** | intranet apps |

### Cookie auth (without full Identity)

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Denied";
    });

// Sign in manually
var claims = new List<Claim>
{
    new(ClaimTypes.Name, user.Email),
    new(ClaimTypes.Role, "Admin")
};
var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
```

## Claims-Based & Policy-Based Authorization

Modern authorization is **claims-based**: an identity carries claims (name, role, permissions),
and **policies** define requirements.

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Over18", policy =>
        policy.RequireClaim("Age").RequireAssertion(ctx =>
            int.TryParse(ctx.User.FindFirst("Age")?.Value, out var a) && a >= 18));

    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// Apply
[Authorize(Policy = "Over18")]
public IActionResult RestrictedContent() => View();
```

## Accessing the Current User

```csharp
if (User.Identity.IsAuthenticated)
{
    string name = User.Identity.Name;
    bool isAdmin = User.IsInRole("Admin");
    string? email = User.FindFirst(ClaimTypes.Email)?.Value;
}
```

## Security Best Practices

- **Always hash passwords** — Identity uses PBKDF2 by default. Never store plain text.
- Use **HTTPS** everywhere; set `[RequireHttps]` / HSTS.
- Protect forms with **`[ValidateAntiForgeryToken]`** (CSRF).
- Enable **account lockout** after failed attempts.
- Apply the **principle of least privilege** with roles/policies.
- Consider **two-factor authentication (2FA)** for sensitive apps.
- Never trust client input — validate and authorize on the server.

## Key Takeaways

- Authentication verifies identity; authorization controls access — configure both in the pipeline.
- **ASP.NET Identity** manages users/roles/claims; `UserManager`/`SignInManager` do the work.
- Protect resources with `[Authorize]` (roles) and **policies** (claims-based requirements).
- Use HTTPS, anti-forgery tokens, lockout, and password hashing — security is non-negotiable.

➡️ Next: [Module 12 — Web API & AJAX](12-web-api-and-ajax.md)
