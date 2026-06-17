# Lesson 6 — State: data that changes over time

[← Previous: Props](./05-props.md) · [Back to outline](../README.md) · [Next: Handling events →](./07-events.md)

---

## Why it matters

Props are read-only data passed *in*. But apps need data that **changes**: a counter, a form input, whether a menu is open. That's **state**. State is what makes your UI *interactive*.

## The problem with a normal variable

You might think you can just use a regular variable:

```jsx
function Counter() {
  let count = 0;

  return (
    <button onClick={() => { count = count + 1; }}>
      Count: {count}
    </button>
  );
}
```

Click the button and... **nothing visibly changes.** Why? Two reasons:

1. React doesn't know the data changed, so it never re-renders.
2. Even if it did re-render, the function runs again and resets `count` back to `0`.

We need a variable that (a) React *watches*, and (b) *survives* between re-renders. That's exactly what `useState` gives us.

## `useState` — your first Hook

```jsx
import { useState } from "react";

function Counter() {
  const [count, setCount] = useState(0);

  return (
    <button onClick={() => setCount(count + 1)}>
      Count: {count}
    </button>
  );
}
```

Now it works! Let's break down that one important line:

```jsx
const [count, setCount] = useState(0);
```

- `useState(0)` creates a state variable with an **initial value** of `0`.
- It returns an array with two things:
  - `count` → the **current value**.
  - `setCount` → a **function to update it**.
- We use array destructuring `[count, setCount]` to name them. You can name them anything, but the convention is `x` and `setX`.

## The two rules of state

### Rule 1: Update state ONLY with the setter function

Never assign directly. Always call the setter:

```jsx
count = count + 1;       // ❌ React won't notice, won't re-render
setCount(count + 1);     // ✅ React re-renders with the new value
```

When you call `setCount`, React:
1. Saves the new value.
2. Re-runs your component function.
3. Updates the screen to match.

This is "UI = f(state)" in action.

### Rule 2: State updates are not instant

`setCount` schedules an update; it doesn't change `count` on the very next line:

```jsx
function handleClick() {
  setCount(count + 1);
  console.log(count);   // still the OLD value here!
}
```

Think of it as "please use this value on the next render." The new value shows up the next time the component runs.

## Updating based on the previous value

Because updates are batched, calling the setter several times with `count + 1` can misbehave:

```jsx
setCount(count + 1);
setCount(count + 1);   // both used the same old count → only +1 total
```

When the new value depends on the old one, pass a **function** to the setter. React gives you the latest value:

```jsx
setCount(prev => prev + 1);
setCount(prev => prev + 1);   // now correctly +2
```

**Rule of thumb:** if the next state depends on the previous state, use the function form `setX(prev => ...)`.

## State can be any type

```jsx
const [name, setName] = useState("");          // string
const [age, setAge] = useState(0);             // number
const [isOpen, setIsOpen] = useState(false);   // boolean
const [items, setItems] = useState([]);        // array
const [user, setUser] = useState({ name: "", age: 0 }); // object
```

## Updating objects and arrays: make a copy, don't mutate

React compares the *reference* to decide if state changed. If you mutate the same object/array, the reference is the same and React may not re-render. **Always create a new copy.**

### Objects — spread `...` then override

```jsx
const [user, setUser] = useState({ name: "Sara", age: 25 });

// ❌ mutation
user.age = 26;
setUser(user);

// ✅ new object with one field changed
setUser({ ...user, age: 26 });
```

### Arrays — use methods that return a new array

```jsx
const [items, setItems] = useState(["a", "b"]);

// Add
setItems([...items, "c"]);

// Remove (by value)
setItems(items.filter(x => x !== "b"));

// Update one item
setItems(items.map(x => (x === "a" ? "A" : x)));
```

Avoid `push`, `pop`, `splice`, and direct index assignment on state arrays — those mutate.

## Each component instance has its own state

If you render `<Counter />` three times, each one has its own independent `count`. State is local to a single component instance.

## When should something be state?

Ask: *"Does this value change over time due to user interaction, and does the screen need to update when it does?"* If yes → state. If it can be calculated from existing props/state, **don't** store it in state; just compute it during render.

```jsx
// ❌ redundant state
const [fullName, setFullName] = useState("");
// ✅ just compute it
const fullName = firstName + " " + lastName;
```

## Try it yourself

1. Build a counter with **+**, **−**, and **Reset** buttons.
2. Build a toggle that shows/hides a paragraph (boolean state).
3. Build a text input that shows what you type live (`value` + `onChange`) — preview of the next lessons.
4. Build a "shopping list": an array of strings with an **Add** button.

---

[← Previous: Props](./05-props.md) · [Back to outline](../README.md) · [Next: Handling events →](./07-events.md)
