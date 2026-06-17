# 07 — Security Best Practices

Security is non-negotiable for any web developer. Know the **OWASP Top 10** and
how ASP.NET MVC defends against each — and how to not accidentally disable those
defenses.

---

## 1. Authentication vs Authorization
- **Authentication (AuthN):** *Who are you?* Verifying identity (login).
- **Authorization (AuthZ):** *What can you do?* Permissions/roles/policies.

```csharp
[Authorize]                          // must be logged in
[Authorize(Roles = "Admin")]         // role-based
[Authorize(Policy = "CanEditOrders")] // policy-based (Core)
public class AdminController : Controller { }
```

### Mechanisms
- **Cookie auth** — classic server-rendered apps (ASP.NET Identity, OWIN/Forms auth).
- **JWT (bearer tokens)** — APIs/SPAs; stateless, signed; store carefully.
- **OAuth 2.0 / OpenID Connect** — delegated auth & SSO (Google, Azure AD/Entra,
  Auth0, IdentityServer/Duende).
- **ASP.NET Identity** — user store, password hashing, lockout, 2FA, external
  logins.

> Prefer a vetted identity system over rolling your own. Never store plaintext
> passwords — use a strong, salted hash (Identity uses PBKDF2; bcrypt/argon2 are
> alternatives).

---

## 2. OWASP Top 10 (and the MVC defense)

### A01 Broken Access Control
- Enforce AuthZ on **every** sensitive action (don't rely on hidden UI).
- Check ownership (does *this* user own *this* record?).
- Beware **IDOR** (changing `?id=123` to access others' data).

### A02 Cryptographic Failures
- HTTPS everywhere (HSTS). Encrypt sensitive data at rest.
- Use the Data Protection API for keys/tokens; don't invent crypto.

### A03 Injection (SQL, command, etc.)
- **Always parameterize** queries. EF/LINQ parameterizes by default; raw SQL must
  use parameters.
```csharp
// SAFE
db.Database.ExecuteSqlInterpolated($"DELETE FROM Logs WHERE Id = {id}");
// UNSAFE: "DELETE FROM Logs WHERE Id = " + id
```

### A04 Insecure Design
- Threat-model features. Apply least privilege, defense in depth, fail securely.

### A05 Security Misconfiguration
- Turn off detailed errors in production. Remove default/sample pages.
- Set security headers (CSP, X-Content-Type-Options, X-Frame-Options, Referrer-Policy).
- Keep `web.config`/`appsettings` secrets out of source control.

### A06 Vulnerable & Outdated Components
- Patch NuGet packages & the runtime. Monitor advisories
  (`dotnet list package --vulnerable`, Dependabot).

### A07 Identification & Authentication Failures
- Strong password policy, account lockout, MFA, secure session/cookie settings,
  rotate/expire tokens.

### A08 Software & Data Integrity Failures
- Validate deserialization; don't deserialize untrusted data with type info.
- Sign/verify update packages and CI artifacts.

### A09 Logging & Monitoring Failures
- Log auth events and errors (without secrets/PII). Alert on anomalies.

### A10 Server-Side Request Forgery (SSRF)
- Validate/allow-list outbound URLs built from user input.

---

## 3. XSS (Cross-Site Scripting)
- Razor **HTML-encodes** `@expression` by default — keep it that way.
- Danger zones: `@Html.Raw`, `innerHTML`, building HTML strings, injecting JSON
  into `<script>` without encoding.
- Defense in depth: **Content Security Policy (CSP)**, encode on output, validate
  on input, `HttpOnly` cookies so XSS can't steal them.

---

## 4. CSRF (Cross-Site Request Forgery)
- MVC anti-forgery tokens defend state-changing requests.
```cshtml
@using (Html.BeginForm()) {
    @Html.AntiForgeryToken()
    ...
}
```
```csharp
[HttpPost, ValidateAntiForgeryToken]
public ActionResult Transfer(TransferVm vm) { ... }
```
- Set cookies `SameSite=Lax/Strict`. Send the token on AJAX POSTs.

---

## 5. Input validation & output encoding
- **Validate on the server always** (client validation is UX only).
- Allow-list (what's permitted) beats deny-list.
- Encode for the **context**: HTML, attribute, JS, URL.
- File uploads: validate type/size, store outside web root, never trust the
  filename, scan if possible.

---

## 6. Secrets & configuration
- Never commit connection strings, API keys, certs.
- Dev: **User Secrets** (`dotnet user-secrets`). Prod: environment variables,
  Azure Key Vault / AWS Secrets Manager.
- Use the **Options pattern** to bind config; protect with Data Protection.

---

## 7. Transport & headers
- Enforce HTTPS + HSTS; redirect HTTP→HTTPS.
- Recommended headers:
```
Content-Security-Policy: default-src 'self'
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Referrer-Policy: no-referrer
Strict-Transport-Security: max-age=31536000; includeSubDomains
```
- Cookies: `Secure; HttpOnly; SameSite`.

---

## 8. API security
- Authenticate (JWT/OAuth), authorize per endpoint.
- Rate limit / throttle (429). Validate & size-limit payloads.
- Don't leak stack traces; return a consistent error shape.
- CORS allow-list (file 04).

---

## Common pitfalls
- Trusting client-side validation.
- `@Html.Raw` with user content.
- Building SQL with string concatenation.
- Missing `[ValidateAntiForgeryToken]` on POSTs.
- IDOR — not checking record ownership.
- Secrets in source control.
- Detailed error pages in production.

## Practice / interview questions
1. AuthN vs AuthZ — define and give an MVC example of each.
2. How does Razor prevent XSS and how might you defeat it?
3. Explain CSRF and the anti-forgery token flow.
4. How do you prevent SQL injection in EF and in raw SQL?
5. What is IDOR and how do you prevent it?
6. Where should secrets live in dev vs prod?
7. Name five security headers and what they do.
8. How would you store user passwords?
