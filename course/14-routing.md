# Lesson 14 — Routing: multiple pages with React Router

[← Previous: Context API](./13-context-api.md) · [Back to outline](../README.md) · [Next: Data fetching →](./15-data-fetching.md)

---

## Why it matters

React builds **single-page applications (SPAs)**: one HTML page where JavaScript swaps the content. But users still expect multiple "pages" with real URLs (`/`, `/about`, `/products/5`) and working back/forward buttons. **React Router** makes that happen without full page reloads.

## Install

```bash
npm install react-router-dom
```

> This lesson uses React Router v6/v7 syntax, the current standard.

## The core pieces

| Piece | Job |
|-------|-----|
| `BrowserRouter` | Wraps your app; enables routing. |
| `Routes` | A container that picks one matching route. |
| `Route` | Maps a URL `path` to a component (`element`). |
| `Link` | Navigates without reloading the page. |
| `useNavigate` | Navigate from code (e.g., after submit). |
| `useParams` | Read dynamic parts of the URL (like an ID). |

## Basic setup

```jsx
import { BrowserRouter, Routes, Route, Link } from "react-router-dom";

function Home()  { return <h1>Home</h1>; }
function About() { return <h1>About</h1>; }

export default function App() {
  return (
    <BrowserRouter>
      <nav>
        <Link to="/">Home</Link> | <Link to="/about">About</Link>
      </nav>

      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/about" element={<About />} />
      </Routes>
    </BrowserRouter>
  );
}
```

- Wrap everything in `<BrowserRouter>` once (usually in `App` or `main.jsx`).
- `<Routes>` looks at the current URL and renders the first `<Route>` whose `path` matches.
- Use `<Link to="...">` instead of `<a href="...">` so navigation is instant (no full reload).

## Dynamic routes (URL parameters)

Use a colon `:` to mark a dynamic segment, then read it with `useParams`:

```jsx
import { Routes, Route, useParams } from "react-router-dom";

function ProductPage() {
  const { id } = useParams();   // from the URL, e.g. /products/42 → id = "42"
  return <h1>Product #{id}</h1>;
}

function App() {
  return (
    <Routes>
      <Route path="/products/:id" element={<ProductPage />} />
    </Routes>
  );
}
```

Visiting `/products/42` renders "Product #42". Great for detail pages.

## Navigating from code

Sometimes you navigate after an action (e.g., after a successful login), not from a link. Use `useNavigate`:

```jsx
import { useNavigate } from "react-router-dom";

function LoginForm() {
  const navigate = useNavigate();

  function handleSubmit(e) {
    e.preventDefault();
    // ...log in...
    navigate("/dashboard");   // go to another route
  }

  return <form onSubmit={handleSubmit}>{/* ... */}</form>;
}
```

`navigate(-1)` goes back, like the browser's back button.

## A "not found" (404) route

A `path="*"` catches anything that didn't match:

```jsx
<Routes>
  <Route path="/" element={<Home />} />
  <Route path="/about" element={<About />} />
  <Route path="*" element={<h1>404 — Page not found</h1>} />
</Routes>
```

## Nested routes and shared layouts

You often want a shared layout (navbar, footer) around several pages. Nest routes and render child routes with `<Outlet />`:

```jsx
import { Outlet, Link } from "react-router-dom";

function Layout() {
  return (
    <div>
      <nav><Link to="/">Home</Link> | <Link to="/about">About</Link></nav>
      <main>
        <Outlet />   {/* the matched child route renders here */}
      </main>
      <footer>© 2025</footer>
    </div>
  );
}

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Home />} />
        <Route path="/about" element={<About />} />
      </Route>
    </Routes>
  );
}
```

The navbar and footer stay; only the `<Outlet />` content changes between pages.

## `NavLink` for active styling

`NavLink` works like `Link` but knows when it's the active route, so you can style the current page's link:

```jsx
import { NavLink } from "react-router-dom";

<NavLink to="/about" className={({ isActive }) => (isActive ? "active" : "")}>
  About
</NavLink>
```

## Note: frameworks

If you're building a serious app, consider a framework like **Next.js** (file-based routing, server rendering) or **React Router** in framework mode. But understanding these client-side routing basics applies everywhere.

## Try it yourself

1. Set up routes for `/`, `/about`, and `/contact` with a shared navbar.
2. Add a dynamic route `/users/:username` that displays the username via `useParams`.
3. Add a catch-all 404 route.
4. Add a button that calls `navigate("/")` to return home.
5. Use `NavLink` to highlight the current page in the navbar.

---

[← Previous: Context API](./13-context-api.md) · [Back to outline](../README.md) · [Next: Data fetching →](./15-data-fetching.md)
