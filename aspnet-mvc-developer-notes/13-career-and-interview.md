# 13 — Career Growth, Soft Skills & Interview Prep

Technical skill gets you in the door; communication, judgment, and consistency
get you promoted. After 3 years, this is often what separates mid from senior.

---

## 1. Where you are and where to go
A rough progression for a .NET web developer:

| Level | Focus |
|-------|-------|
| Junior | Implement well-defined tasks, learn the stack |
| **Mid (you, ~3 yrs)** | Own features end-to-end, debug independently, write tests |
| Senior | Design systems, set patterns, mentor, weigh trade-offs, own quality |
| Lead / Architect / Staff | Cross-team design, technical direction, multiplier effect |

To move mid → senior, deliberately build: **architecture** (file 09),
**performance** (file 10), **security** (file 07), **DevOps** (file 11), and the
soft skills below.

---

## 2. High-leverage technical growth areas
1. **Modern .NET / ASP.NET Core** (file 12) — biggest single upgrade.
2. **Cloud** (Azure most relevant for .NET; AWS valuable too) — get a fundamentals
   cert if useful (AZ-900 → AZ-204).
3. **System design** — caching, queues, scaling, data modeling, trade-offs.
4. **Automated testing & CI/CD** — quality and delivery speed.
5. **Observability** — logging, metrics, tracing; debugging production.
6. **One frontend direction** — Blazor (stay in C#) or a JS framework + APIs.
7. **Containers/Kubernetes** basics.
8. **Security mindset** baked into everything.

---

## 3. Soft skills that compound
- **Communication:** write clear PR descriptions, docs, and design notes; explain
  trade-offs to non-engineers. Ability to write is a force multiplier.
- **Code review:** give kind, specific, actionable feedback; receive it without ego.
- **Estimation & scoping:** break work down; surface risks early; under-promise.
- **Ownership:** see issues through to production; follow up on incidents.
- **Collaboration:** unblock others, share context, no "hero" silos.
- **Mentoring:** teaching juniors deepens your own understanding and signals
  seniority.
- **Time/priority management:** focus on impact, say no to low-value work.

---

## 4. Engineering habits of strong developers
- Read code more than you write; understand before changing.
- Leave code better than you found it (boy-scout rule).
- Small, frequent commits/PRs; meaningful messages.
- Automate repetitive tasks.
- Reproduce → diagnose → fix → add a regression test (don't guess-fix).
- Keep a learning log / personal notes (like this repo!).
- Read the docs and source, not just Stack Overflow.

---

## 5. Staying current
- Official: **.NET Blog**, **ASP.NET Core docs (Microsoft Learn)**, release notes.
- People: Microsoft .NET team, David Fowler, Stephen Toub, Andrew Lock,
  Steve Gordon, Nick Chapsas, Tim Corey.
- Communities: r/dotnet, .NET Discord/Slack, local meetups, conferences (.NET
  Conf is free/online).
- Practice: side projects, open source contributions, coding katas.

---

## 6. Building a portfolio
- A few **polished** GitHub projects > many half-finished ones.
- Show breadth: an API, a data layer with EF, tests, CI, a README explaining
  decisions.
- Write about what you learn (blog/notes) — demonstrates communication + depth.
- Contribute to open source (docs, bug fixes) for real-world collaboration signal.

---

## 7. Interview preparation
### Technical topics they probe (map to these notes)
- C# internals & async (file 01)
- MVC lifecycle, filters, model binding (file 02)
- EF/N+1, loading strategies, transactions (file 03)
- HTTP/REST/status codes (file 04)
- SQL joins, indexing, query tuning (file 06)
- Security: XSS/CSRF/SQLi/auth (file 07)
- Testing approach (file 08)
- SOLID, patterns, architecture (file 09)
- Performance & caching (file 10)
- System design (scaling a web app, designing an API)

### Formats
- **Coding:** data structures/algorithms, LINQ problems, small features.
- **System design:** "design a URL shortener / ticketing system / e-commerce
  checkout." Talk through requirements → data model → API → scaling → trade-offs.
- **Behavioral:** use **STAR** (Situation, Task, Action, Result). Prepare stories
  about conflict, failure, leadership, a hard bug, a tough deadline.

### Tips
- Think out loud; clarify requirements before coding.
- State assumptions and trade-offs.
- It's fine to say "I'd look this up" — but know the fundamentals cold.
- Have **questions for them** (team, process, tech, growth).

---

## 8. A simple 90-day self-improvement plan
- **Weeks 1–4:** Audit with the README checklist; close C#/MVC/EF gaps; build a
  small ASP.NET Core app (file 12) to learn the modern stack.
- **Weeks 5–8:** Add EF Core + a real DB; write unit + integration tests; set up
  a CI pipeline; add logging.
- **Weeks 9–12:** Add auth, caching, and a deployment (Docker + cloud); practice
  system design + behavioral stories; polish a portfolio project + README.

---

## Common pitfalls
- Going deep on one stack but never broadening (DevOps, cloud, frontend).
- Cramming algorithms while ignoring fundamentals you use daily.
- Weak communication overshadowing strong coding.
- Many abandoned side projects instead of one finished, documented one.
- Not learning modern .NET while the ecosystem moves on.

## Reflection questions
1. Can you explain your last project's architecture and the trade-offs you made?
2. What's a bug you debugged that taught you something? (Have a STAR story.)
3. What would you change about a system you built, knowing what you know now?
4. Which gap from the README checklist will you close first, and how?
5. Can you teach one of these topics to a junior clearly? (If not, learn it deeper.)
