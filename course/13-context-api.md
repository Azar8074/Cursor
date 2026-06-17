# Lesson 13 — Context API: sharing data without "prop drilling"

[← Previous: Hooks deep dive](./12-hooks-deep-dive.md) · [Back to outline](../README.md) · [Next: Routing →](./14-routing.md)

---

## Why it matters

Props flow down one level at a time. But sometimes deeply nested components need the same data (the logged-in user, the theme, the language). Passing props through many layers that don't even use them is called **prop drilling**, and it's painful. **Context** solves this.

## The problem: prop drilling

```jsx
function App() {
  const user = { name: "Sara" };
  return <Page user={user} />;
}
function Page({ user }) {        // doesn't use user, just passes it
  return <Sidebar user={user} />;
}
function Sidebar({ user }) {     // doesn't use user, just passes it
  return <Avatar user={user} />;
}
function Avatar({ user }) {      // finally uses it!
  return <span>{user.name}</span>;
}
```

`Page` and `Sidebar` don't need `user` — they're just couriers. Context lets `Avatar` read `user` directly.

## Context in 3 steps

### Step 1: Create a context

```jsx
// UserContext.js
import { createContext } from "react";

export const UserContext = createContext(null);
```

`createContext(defaultValue)` makes a context object. The default is used only if a component reads the context with no provider above it.

### Step 2: Provide a value

Wrap part of your tree in the context's `Provider` and give it a `value`:

```jsx
import { UserContext } from "./UserContext";

function App() {
  const user = { name: "Sara" };

  return (
    <UserContext.Provider value={user}>
      <Page />
    </UserContext.Provider>
  );
}
```

Now everything inside can access `user` — no more passing props through `Page` and `Sidebar`.

### Step 3: Consume the value with `useContext`

```jsx
import { useContext } from "react";
import { UserContext } from "./UserContext";

function Avatar() {
  const user = useContext(UserContext);
  return <span>{user.name}</span>;
}
```

`Page` and `Sidebar` no longer touch `user` at all. 🎉

## Context + state = a shared, updatable store

Context shines when combined with state. Put the state in a provider component and share both the value and its updater:

```jsx
// ThemeContext.jsx
import { createContext, useContext, useState } from "react";

const ThemeContext = createContext();

export function ThemeProvider({ children }) {
  const [theme, setTheme] = useState("light");
  const toggleTheme = () => setTheme((t) => (t === "light" ? "dark" : "light"));

  return (
    <ThemeContext.Provider value={{ theme, toggleTheme }}>
      {children}
    </ThemeContext.Provider>
  );
}

// A custom Hook for convenience
export function useTheme() {
  return useContext(ThemeContext);
}
```

```jsx
// App.jsx
import { ThemeProvider, useTheme } from "./ThemeContext";

function ThemedButton() {
  const { theme, toggleTheme } = useTheme();
  return (
    <button onClick={toggleTheme}>
      Current theme: {theme} (click to switch)
    </button>
  );
}

export default function App() {
  return (
    <ThemeProvider>
      <ThemedButton />
    </ThemeProvider>
  );
}
```

Any component, at any depth, can now read or toggle the theme. This pattern — a provider component exposing state + actions, plus a custom `useX` Hook — is the standard way to build app-wide stores with Context.

## When to use Context (and when not to)

✅ **Good for:** truly global, rarely-changing data — current user/auth, theme, language, etc.

⚠️ **Be careful:** when a context value changes, **all** components consuming it re-render. Don't put fast-changing or huge state in one giant context, or you'll cause performance issues.

❌ **Don't reach for Context too early.** If only a parent and one child need the data, plain props are simpler and clearer.

## Context vs state-management libraries

Context is built-in and perfect for small/medium global state. For large apps with complex, frequently-changing global state, dedicated libraries like **Zustand**, **Redux Toolkit**, or **Jotai** offer more power and better performance. They're optional — learn Context first.

## Try it yourself

1. Create a `UserContext`, provide a user at the top, and read it in a deeply nested component.
2. Build a `ThemeProvider` with `theme` + `toggleTheme`, plus a `useTheme` Hook.
3. Apply the theme to the page background using the context value.
4. Add a second consumer somewhere else in the tree and confirm both stay in sync.

---

[← Previous: Hooks deep dive](./12-hooks-deep-dive.md) · [Back to outline](../README.md) · [Next: Routing →](./14-routing.md)
