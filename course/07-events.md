# Lesson 7 — Handling events (clicks, typing, etc.)

[← Previous: State](./06-state.md) · [Back to outline](../README.md) · [Next: Conditional rendering →](./08-conditional-rendering.md)

---

## Why it matters

Interactivity comes from **events**: the user clicks, types, hovers, submits a form. React gives you a clean, consistent way to respond to all of them.

## The basics

Attach an event handler with a camelCased prop and pass it a **function**:

```jsx
function Button() {
  function handleClick() {
    alert("You clicked me!");
  }

  return <button onClick={handleClick}>Click me</button>;
}
```

Common events: `onClick`, `onChange`, `onSubmit`, `onMouseEnter`, `onKeyDown`, `onFocus`, `onBlur`.

## Pass the function, don't call it

This is the #1 beginner mistake:

```jsx
<button onClick={handleClick}>OK</button>     // ✅ pass the function
<button onClick={handleClick()}>Bad</button>  // ❌ calls it immediately on render
```

With the parentheses `()`, the function runs **the moment the component renders**, not when clicked. You want to give React the function so it can call it *later* when the click happens.

## Passing arguments to a handler

If you need to pass an argument, wrap it in an arrow function:

```jsx
function List() {
  function handleDelete(id) {
    console.log("Delete item", id);
  }

  return (
    <button onClick={() => handleDelete(42)}>Delete</button>
  );
}
```

`() => handleDelete(42)` is a brand-new function that, *when clicked*, calls `handleDelete(42)`. This keeps the "pass a function, don't call it" rule.

## The event object

React passes an **event object** to your handler with details about what happened:

```jsx
function handleClick(event) {
  console.log(event.type);          // "click"
  console.log(event.target);        // the element clicked
}
```

The most common use is reading input values:

```jsx
function NameInput() {
  function handleChange(event) {
    console.log(event.target.value); // the current text
  }

  return <input onChange={handleChange} />;
}
```

> React's event object is called a **SyntheticEvent** — a cross-browser wrapper around the native event. It behaves the same in every browser.

## Combining events with state

Events + state = interactivity. This is the core loop of React:

```jsx
import { useState } from "react";

function Counter() {
  const [count, setCount] = useState(0);

  function increment() {
    setCount(prev => prev + 1);
  }

  return (
    <div>
      <p>Count: {count}</p>
      <button onClick={increment}>+1</button>
    </div>
  );
}
```

The flow: **user clicks → handler runs → state updates → component re-renders → screen updates.**

## `preventDefault` — stop the browser's default behavior

Some elements do something automatically. The classic example: submitting a form reloads the page. Stop it with `event.preventDefault()`:

```jsx
function SearchForm() {
  function handleSubmit(event) {
    event.preventDefault();    // stop the page reload
    console.log("Searching...");
  }

  return (
    <form onSubmit={handleSubmit}>
      <input type="text" />
      <button type="submit">Search</button>
    </form>
  );
}
```

## Inline handlers (fine for small things)

For very short handlers you can write them inline:

```jsx
<button onClick={() => setCount(count + 1)}>+1</button>
```

For anything longer, define a named function above the `return` — it's easier to read.

## Try it yourself

1. A button that toggles a light/dark message ("☀️ Day" / "🌙 Night").
2. A list with three "Delete" buttons; each logs its own index using `() => handleDelete(i)`.
3. A form with a text input and a submit button that `preventDefault`s and shows what was typed.
4. An input that updates a `count` of how many characters you've typed (`event.target.value.length`).

---

[← Previous: State](./06-state.md) · [Back to outline](../README.md) · [Next: Conditional rendering →](./08-conditional-rendering.md)
