# React — A Complete Course (Made Easy to Understand)

Welcome! This is a beginner-friendly, step-by-step course on **React**, the most popular JavaScript library for building user interfaces.

This course assumes you know a little bit of **HTML, CSS, and JavaScript**. If you can write a `for` loop and a function in JavaScript, you are ready. We explain everything else along the way, in plain language, with lots of small examples.

> Tip: Don't just read — type the examples yourself and break them on purpose. That's how React "clicks".

---

## How this course works

Each lesson lives in the [`course/`](./course) folder and is numbered in the order you should read it. Every lesson has:

- **A short "why it matters" intro** — so you know what problem we are solving.
- **Plain-language explanations** — no jargon without a definition.
- **Small, copy-pasteable examples**.
- **A "Try it yourself" challenge** at the end.

There is also a [`examples/`](./examples) folder with a small, runnable React app you can open and play with.

---

## Course outline

### Part 1 — The Fundamentals

1. [What is React (and why use it)?](./course/01-introduction.md)
2. [Setting up your environment](./course/02-environment-setup.md)
3. [JSX — writing HTML inside JavaScript](./course/03-jsx.md)
4. [Components — the building blocks](./course/04-components.md)
5. [Props — passing data into components](./course/05-props.md)

### Part 2 — Making things interactive

6. [State — data that changes over time](./course/06-state.md)
7. [Handling events (clicks, typing, etc.)](./course/07-events.md)
8. [Conditional rendering — showing things "if"](./course/08-conditional-rendering.md)
9. [Lists and keys — rendering many items](./course/09-lists-and-keys.md)
10. [Forms and inputs](./course/10-forms.md)

### Part 3 — Hooks (the modern React superpower)

11. [`useEffect` — running side effects](./course/11-useeffect.md)
12. [Hooks deep dive (`useRef`, `useReducer`, `useMemo`, `useCallback`, custom hooks)](./course/12-hooks-deep-dive.md)

### Part 4 — Building real apps

13. [Context API — sharing data without "prop drilling"](./course/13-context-api.md)
14. [Routing — multiple pages with React Router](./course/14-routing.md)
15. [Fetching data from an API](./course/15-data-fetching.md)
16. [Performance — making React fast](./course/16-performance.md)

### Part 5 — Put it all together

17. [Project: Build a Todo app step by step](./course/17-project-todo-app.md)
18. [Where to go next](./course/18-next-steps.md)

---

## Quick start (run the example app)

You need **Node.js 18+** installed. Check with `node -v`.

```bash
cd examples/todo-app
npm install
npm run dev
```

Then open the URL it prints (usually `http://localhost:5173`).

See [`examples/todo-app/README.md`](./examples/todo-app/README.md) for details.

---

## A 30-second mental model of React

React is built on one big idea:

> **Your UI is a function of your data.**
>
> `UI = f(state)`

You describe *what* the screen should look like for a given set of data (the "state"). When the data changes, React figures out *how* to update the screen for you. You almost never touch the DOM directly.

That's the whole game. Everything else in this course is just details that support that idea.

Happy learning! Start with [Lesson 1 →](./course/01-introduction.md)
