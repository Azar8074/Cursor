# Lesson 16 — Performance: making React fast

[← Previous: Data fetching](./15-data-fetching.md) · [Back to outline](../README.md) · [Next: Project →](./17-project-todo-app.md)

---

## Why it matters

React is fast by default. But as apps grow, unnecessary re-renders can make them sluggish. Knowing *how* React re-renders — and the few tools to control it — lets you fix slowness when it actually happens.

> **Golden rule:** Don't optimize prematurely. Write clear code first. Measure, find the real bottleneck, *then* optimize. Most apps never need most of this lesson.

## How re-rendering works

When a component's **state or props change**, React re-renders that component **and all of its children** by default. Re-rendering means "run the function again and compute the new JSX," then React efficiently updates only the changed DOM.

Re-rendering itself is usually cheap. Problems arise when:

- A component re-renders very often, and
- Its render does expensive work, or it re-renders huge subtrees needlessly.

## Tool 1: `React.memo` — skip re-renders when props didn't change

`React.memo` wraps a component so it only re-renders when its **props** actually change:

```jsx
import { memo } from "react";

const ExpensiveItem = memo(function ExpensiveItem({ name }) {
  console.log("rendering", name);
  return <li>{name}</li>;
});
```

Now if the parent re-renders but `name` is the same, `ExpensiveItem` is skipped.

> **Catch:** memo compares props *shallowly*. If you pass a new object/array/function every render, the props "change" every time and memo does nothing — which leads to the next two tools.

## Tool 2: `useCallback` — stable function props

Every render creates brand-new function instances. If you pass a function to a `memo`-ized child, the child sees a "new" prop each time and re-renders anyway. `useCallback` keeps the same function instance:

```jsx
import { useCallback } from "react";

const handleDelete = useCallback((id) => {
  setItems((prev) => prev.filter((x) => x.id !== id));
}, []);   // stable across renders → memo child won't re-render needlessly
```

## Tool 3: `useMemo` — stable values / expensive calculations

`useMemo` caches a computed value (and keeps object/array references stable) so it only recomputes when dependencies change:

```jsx
import { useMemo } from "react";

const sortedItems = useMemo(
  () => [...items].sort((a, b) => a.price - b.price),
  [items]
);
```

Use it for (a) genuinely expensive computations, or (b) keeping a referentially-stable object/array to pass to `memo`-ized children or context.

## When to use these (and when not to)

These three (`memo`, `useCallback`, `useMemo`) work as a **team**. They're worth it when:

- You have a `memo`-ized child you want to stop re-rendering, AND
- You pass it functions/objects → wrap those in `useCallback`/`useMemo`.

Do **not** wrap everything by default. The memoization itself has a small cost and adds complexity. Profile first.

## Bigger wins than micro-optimizations

These often matter far more than `memo`:

### 1. Code splitting with `lazy` + `Suspense`

Don't ship your whole app in one giant bundle. Load parts on demand:

```jsx
import { lazy, Suspense } from "react";

const Dashboard = lazy(() => import("./Dashboard"));

function App() {
  return (
    <Suspense fallback={<p>Loading…</p>}>
      <Dashboard />
    </Suspense>
  );
}
```

The `Dashboard` code is downloaded only when it's actually needed → faster initial load.

### 2. Virtualize long lists

Rendering 10,000 rows is slow. Libraries like **TanStack Virtual** or **react-window** render only the rows currently visible on screen.

### 3. Use correct `key`s

Bad keys (like array index in a reorderable list — Lesson 9) cause React to recreate/misplace DOM and lose state. Good keys prevent wasted work and bugs.

### 4. Keep state as local as possible

If only one small component needs a piece of state, don't put it high in the tree — that re-renders everything below. Move state *down* to where it's used.

## Measuring: the React DevTools Profiler

Install the **React Developer Tools** browser extension. Its **Profiler** tab records renders and shows which components rendered, how often, and how long they took. This is how you find the *real* bottleneck instead of guessing.

> React 19 introduced the **React Compiler**, which can automatically apply many of these memoization optimizations for you. Over time, manual `memo`/`useMemo`/`useCallback` may become less necessary.

## Try it yourself

1. Add `console.log` to a child component and watch it re-render when the parent's unrelated state changes. Wrap it in `memo` and observe the difference.
2. Pass a function prop to that memo child; see it re-render again. Fix it with `useCallback`.
3. Code-split a "heavy" component with `lazy` + `Suspense`.
4. Open the React Profiler and record an interaction to see what renders.

---

[← Previous: Data fetching](./15-data-fetching.md) · [Back to outline](../README.md) · [Next: Project →](./17-project-todo-app.md)
