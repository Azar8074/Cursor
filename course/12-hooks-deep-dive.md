# Lesson 12 — Hooks deep dive

[← Previous: useEffect](./11-useeffect.md) · [Back to outline](../README.md) · [Next: Context API →](./13-context-api.md)

---

## Why it matters

You already know two Hooks: `useState` and `useEffect`. React has a few more that solve specific problems, and you can even write your **own** Hooks. This lesson is your reference for the rest.

## First: the Rules of Hooks

All Hooks (names start with `use`) follow two non-negotiable rules:

1. **Only call Hooks at the top level** of a component. Never inside loops, conditions, or nested functions.
2. **Only call Hooks from React functions** — components or custom Hooks (not plain JS functions).

```jsx
// ❌ Breaks rule 1: Hook inside a condition
if (loggedIn) {
  const [x, setX] = useState(0);
}

// ✅ Always called, every render, in the same order
const [x, setX] = useState(0);
if (loggedIn) { /* use x */ }
```

Why? React tracks Hooks by their **call order**. Calling them conditionally scrambles that order and breaks everything.

---

## `useRef` — a value that persists without re-rendering

`useRef` gives you a "box" (`.current`) that survives re-renders but **does not** trigger a re-render when changed. Two main uses:

### Use 1: Reference a DOM element

```jsx
import { useRef } from "react";

function SearchBox() {
  const inputRef = useRef(null);

  function focusInput() {
    inputRef.current.focus();   // directly touch the DOM node
  }

  return (
    <>
      <input ref={inputRef} />
      <button onClick={focusInput}>Focus the input</button>
    </>
  );
}
```

### Use 2: Store a mutable value that shouldn't cause re-renders

```jsx
const timerId = useRef(null);

function start() {
  timerId.current = setInterval(/* ... */, 1000);
}
function stop() {
  clearInterval(timerId.current);
}
```

> **`useRef` vs `useState`:** Changing state re-renders the component; changing a ref does not. Use state for data shown on screen; use a ref for "behind the scenes" values (DOM nodes, timer IDs, previous values).

---

## `useReducer` — state logic that's getting complex

When state updates get complicated (many sub-values, or the next state depends heavily on an "action"), `useReducer` organizes the logic better than several `useState` calls.

```jsx
import { useReducer } from "react";

function reducer(state, action) {
  switch (action.type) {
    case "increment": return { count: state.count + 1 };
    case "decrement": return { count: state.count - 1 };
    case "reset":     return { count: 0 };
    default:          return state;
  }
}

function Counter() {
  const [state, dispatch] = useReducer(reducer, { count: 0 });

  return (
    <div>
      <p>{state.count}</p>
      <button onClick={() => dispatch({ type: "increment" })}>+</button>
      <button onClick={() => dispatch({ type: "decrement" })}>−</button>
      <button onClick={() => dispatch({ type: "reset" })}>Reset</button>
    </div>
  );
}
```

- A **reducer** is a pure function `(state, action) => newState`.
- You trigger changes by `dispatch`ing an **action** (a plain object describing what happened).
- This centralizes all the "how state changes" logic in one place — great for forms, wizards, and anything with many related transitions.

> If you know Redux, this is the same idea built into React.

---

## `useMemo` — cache an expensive calculation

`useMemo` remembers (memoizes) the result of a calculation and only recomputes it when its dependencies change.

```jsx
import { useMemo } from "react";

function ProductList({ products, query }) {
  const filtered = useMemo(() => {
    // pretend this is expensive
    return products.filter((p) => p.name.includes(query));
  }, [products, query]);   // recompute only when these change

  return <ul>{filtered.map((p) => <li key={p.id}>{p.name}</li>)}</ul>;
}
```

Without `useMemo`, the filter would re-run on *every* render, even ones unrelated to `products`/`query`. Use it only when a computation is genuinely expensive — don't sprinkle it everywhere.

---

## `useCallback` — cache a function

`useCallback` is `useMemo` for functions. It returns the *same* function instance between renders (unless dependencies change). This matters when passing callbacks to optimized child components (see Lesson 16).

```jsx
import { useCallback } from "react";

const handleClick = useCallback(() => {
  console.log("clicked", id);
}, [id]);   // same function unless `id` changes
```

A new function is created on every render by default; `useCallback` prevents that when it would cause unnecessary child re-renders.

---

## `useContext` — read shared data

`useContext` lets a component read a value from React Context without passing props through every level. We dedicate the whole next lesson to it, so here's just the shape:

```jsx
const theme = useContext(ThemeContext);
```

---

## Custom Hooks — extract and reuse logic

The real superpower: **you can write your own Hooks** to package up reusable stateful logic. A custom Hook is just a function whose name starts with `use` and that calls other Hooks.

### Example: `useToggle`

```jsx
import { useState } from "react";

function useToggle(initial = false) {
  const [value, setValue] = useState(initial);
  const toggle = () => setValue((v) => !v);
  return [value, toggle];
}

// Usage — clean and reusable:
function Panel() {
  const [isOpen, toggleOpen] = useToggle();
  return (
    <>
      <button onClick={toggleOpen}>{isOpen ? "Hide" : "Show"}</button>
      {isOpen && <p>Now you see me!</p>}
    </>
  );
}
```

### Example: `useLocalStorage`

```jsx
import { useState, useEffect } from "react";

function useLocalStorage(key, initialValue) {
  const [value, setValue] = useState(() => {
    const stored = localStorage.getItem(key);
    return stored ? JSON.parse(stored) : initialValue;
  });

  useEffect(() => {
    localStorage.setItem(key, JSON.stringify(value));
  }, [key, value]);

  return [value, setValue];
}

// Usage: behaves like useState, but persists across page reloads
const [name, setName] = useLocalStorage("name", "");
```

Custom Hooks let you share logic between components without repeating yourself. If you find two components doing the same stateful thing, extract a custom Hook.

---

## Quick reference

| Hook | Use it for |
|------|-----------|
| `useState` | Local state that, when changed, updates the UI. |
| `useEffect` | Side effects (fetch, timers, subscriptions, DOM). |
| `useRef` | Persisted value or DOM reference that doesn't re-render. |
| `useReducer` | Complex/related state transitions in one place. |
| `useMemo` | Cache an expensive computed value. |
| `useCallback` | Cache a function identity for optimized children. |
| `useContext` | Read shared data from a Context provider. |
| custom `useX` | Reuse stateful logic across components. |

## Try it yourself

1. Build a `useToggle` and use it to show/hide a sidebar.
2. Use `useRef` to auto-focus an input when the page loads (`useEffect` + `ref.current.focus()`).
3. Rebuild the counter with `useReducer` (actions: increment, decrement, reset, setStep).
4. Write a `useLocalStorage` Hook and persist a dark-mode preference.

---

[← Previous: useEffect](./11-useeffect.md) · [Back to outline](../README.md) · [Next: Context API →](./13-context-api.md)
