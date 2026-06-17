# Lesson 11 — `useEffect`: running side effects

[← Previous: Forms](./10-forms.md) · [Back to outline](../README.md) · [Next: Hooks deep dive →](./12-hooks-deep-dive.md)

---

## Why it matters

So far our components only render UI from props and state. But real apps also need to do things *outside* of rendering: fetch data from a server, set up a timer, read from `localStorage`, subscribe to events, update the document title. These are called **side effects**, and `useEffect` is where they belong.

## What is a "side effect"?

A side effect is anything that reaches *outside* your component to interact with the world:

- Fetching data from an API
- Setting a timer (`setInterval`, `setTimeout`)
- Manually changing the DOM (e.g., `document.title`)
- Subscribing to events (WebSocket, `window` resize)
- Reading/writing browser storage

Rendering should be **pure** (Lesson 4). Side effects don't belong directly in the component body — they belong in `useEffect`, which runs *after* React updates the screen.

## The basic shape

```jsx
import { useEffect } from "react";

useEffect(() => {
  // your side-effect code here
}, [dependencies]);
```

Two parts:

1. A **function** containing the effect code.
2. A **dependency array** that controls *when* the effect runs.

## The dependency array controls timing

This is the heart of `useEffect`. There are three cases:

### 1. `[]` empty array — run once, after the first render

```jsx
useEffect(() => {
  console.log("Component mounted (ran once)");
}, []);
```

Great for initial setup like fetching data when the page loads.

### 2. `[a, b]` with values — run when those values change

```jsx
useEffect(() => {
  console.log("count changed to", count);
}, [count]);   // runs after first render AND whenever count changes
```

### 3. No array at all — run after *every* render

```jsx
useEffect(() => {
  console.log("ran after every render");
});   // usually NOT what you want
```

Leaving out the array is rarely correct and can cause infinite loops. Almost always include a dependency array.

## A practical example: document title

```jsx
import { useState, useEffect } from "react";

function Counter() {
  const [count, setCount] = useState(0);

  useEffect(() => {
    document.title = `Count: ${count}`;
  }, [count]);   // re-run whenever count changes

  return <button onClick={() => setCount(count + 1)}>Count: {count}</button>;
}
```

Every time `count` changes, React updates the screen, then runs the effect to update the browser tab's title.

## A practical example: fetching data

```jsx
import { useState, useEffect } from "react";

function Users() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch("https://jsonplaceholder.typicode.com/users")
      .then((res) => res.json())
      .then((data) => {
        setUsers(data);
        setLoading(false);
      });
  }, []);   // [] → fetch once when the component first appears

  if (loading) return <p>Loading…</p>;

  return (
    <ul>
      {users.map((u) => <li key={u.id}>{u.name}</li>)}
    </ul>
  );
}
```

(We'll go deeper on data fetching — including errors and modern alternatives — in Lesson 15.)

## Cleanup: the returned function

Some effects need to be "undone" — a timer should be cleared, a subscription removed. Return a **cleanup function** from your effect, and React runs it before the next effect run and when the component is removed ("unmounts").

```jsx
useEffect(() => {
  const id = setInterval(() => {
    console.log("tick");
  }, 1000);

  return () => clearInterval(id);   // cleanup: stop the timer
}, []);
```

Without cleanup, you'd stack up multiple timers and leak memory. **Rule of thumb:** if your effect *starts* something (timer, subscription, listener), it should *stop* it in the cleanup.

### Cleanup with an event listener

```jsx
useEffect(() => {
  function handleResize() {
    console.log("window width:", window.innerWidth);
  }
  window.addEventListener("resize", handleResize);
  return () => window.removeEventListener("resize", handleResize);
}, []);
```

## The mental model

For each effect, ask two questions:

1. **What does it synchronize with?** (the dependencies)
2. **Does it need cleanup?** (timers/subscriptions yes; one-off DOM updates no)

Think of `useEffect` as "keep this external thing in sync with my state/props," not as "run this when I click." Event handlers (Lesson 7) are for responding to user actions; effects are for synchronizing with external systems.

## Common mistakes

- **Forgetting dependencies** → effect uses stale values. Include everything from props/state that the effect reads.
- **Setting state unconditionally inside an effect with no/incorrect deps** → infinite loop (render → effect → setState → render → ...).
- **Putting logic in an effect that belongs in an event handler.** If something should happen *because the user did X*, handle it in the event, not an effect.

> Note: In development with `StrictMode`, React runs effects twice on mount to help you catch missing cleanup. This does not happen in production.

## Try it yourself

1. Update `document.title` to show a live counter value.
2. Build a stopwatch using `setInterval` + cleanup with `clearInterval`.
3. Fetch a list of posts from `https://jsonplaceholder.typicode.com/posts` and render the titles.
4. Log the window width on resize, and remove the listener on cleanup.

---

[← Previous: Forms](./10-forms.md) · [Back to outline](../README.md) · [Next: Hooks deep dive →](./12-hooks-deep-dive.md)
