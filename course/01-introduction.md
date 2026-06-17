# Lesson 1 — What is React (and why use it)?

[← Back to outline](../README.md) · [Next: Setting up your environment →](./02-environment-setup.md)

---

## Why it matters

Before learning *how* to use a tool, it helps to know *what problem it solves*. Let's see the problem first, then how React fixes it.

## The problem: keeping the screen in sync with your data

Imagine a simple counter. You have a number, and a button that increases it. With plain JavaScript you'd write something like this:

```html
<p id="count">0</p>
<button id="btn">Add one</button>

<script>
  let count = 0;
  const p = document.getElementById("count");
  const btn = document.getElementById("btn");

  btn.addEventListener("click", () => {
    count = count + 1;       // 1. update the data
    p.textContent = count;   // 2. manually update the screen
  });
</script>
```

Notice you have to do **two things** every time: change the data *and* manually update the DOM (`p.textContent = count`). For a counter this is fine. But in a real app you might have hundreds of pieces of data and thousands of places on screen that depend on them. Manually keeping the screen in sync with the data becomes a nightmare and a giant source of bugs.

## The React idea

React flips this around. You write code that describes **what the screen should look like for the current data**, and React keeps the screen in sync automatically.

The same counter in React:

```jsx
import { useState } from "react";

function Counter() {
  const [count, setCount] = useState(0);

  return (
    <div>
      <p>{count}</p>
      <button onClick={() => setCount(count + 1)}>Add one</button>
    </div>
  );
}
```

You never wrote "find the `<p>` and change its text." You just said:

- The data is `count` (starts at 0).
- The screen shows `{count}`.
- Clicking the button sets `count` to `count + 1`.

When `count` changes, React re-runs your function and updates only the part of the screen that changed. **That's the magic.**

## The one formula to remember

> **UI = f(state)**
>
> Your user interface is a *function* of your data ("state").

You describe the destination ("here's what the screen looks like for this data"), not the turn-by-turn directions ("find this element, change this attribute"). This is called **declarative** programming, and it's why React code is easier to reason about.

## Key vocabulary (just 4 words for now)

| Word | Plain meaning |
|------|---------------|
| **Component** | A reusable piece of UI, written as a function (like `Counter` above). |
| **JSX** | The HTML-looking syntax inside the function. |
| **Props** | Inputs you pass *into* a component (like function arguments). |
| **State** | Data a component remembers and can change over time. |

We'll cover each one in depth. Don't worry about memorizing them now.

## Is React a "framework"?

Technically React is a **library** focused on the UI (the view layer). For full apps you combine it with other tools (a router, a data-fetching library, etc.). Modern toolkits like **Next.js** bundle React with everything you need. We'll start with plain React so you understand the foundation.

## Try it yourself

You don't need to write code yet. Just answer these in your head:

1. In the plain-JavaScript counter, what two steps happen on every click?
2. In the React counter, which of those two steps did *you* not have to write?
3. What does "UI = f(state)" mean in your own words?

---

[← Back to outline](../README.md) · [Next: Setting up your environment →](./02-environment-setup.md)
