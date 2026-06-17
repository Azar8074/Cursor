# Lesson 2 — Setting up your environment

[← Previous: Introduction](./01-introduction.md) · [Back to outline](../README.md) · [Next: JSX →](./03-jsx.md)

---

## Why it matters

To write React you need a few tools on your computer. The good news: modern tooling makes this almost painless. We'll use **Vite** (pronounced "veet"), a fast, beginner-friendly build tool.

## Step 1 — Install Node.js

React tooling runs on **Node.js**. Download the **LTS** version from [nodejs.org](https://nodejs.org).

Check it worked by opening a terminal and running:

```bash
node -v
npm -v
```

You should see version numbers. You need **Node 18 or newer**.

> `npm` ("Node Package Manager") comes with Node. It downloads code libraries (called "packages") for you.

## Step 2 — Create a new React app with Vite

In your terminal, run:

```bash
npm create vite@latest my-first-app
```

It will ask a few questions:

- **Select a framework:** choose `React`
- **Select a variant:** choose `JavaScript` (or `TypeScript` if you know it)

Then:

```bash
cd my-first-app
npm install      # download dependencies
npm run dev      # start the dev server
```

Open the printed URL (usually `http://localhost:5173`). You'll see a starter page. 🎉

## Step 3 — Understand the important files

A fresh Vite + React project looks roughly like this:

```
my-first-app/
├── index.html          ← the single HTML page
├── package.json        ← lists your dependencies & scripts
├── vite.config.js      ← build tool config
└── src/
    ├── main.jsx        ← the entry point; mounts React into the page
    ├── App.jsx         ← your first/root component
    └── index.css       ← global styles
```

The two files you'll touch most are `src/main.jsx` and `src/App.jsx`.

**`index.html`** has just one important line — an empty box for React to fill:

```html
<div id="root"></div>
```

**`src/main.jsx`** connects React to that box:

```jsx
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./App.jsx";
import "./index.css";

createRoot(document.getElementById("root")).render(
  <StrictMode>
    <App />
  </StrictMode>
);
```

In plain English: "Find the `<div id="root">`, and render my `<App />` component inside it."

> **What is `StrictMode`?** A development-only helper that warns you about common mistakes. It can make some code run twice in development on purpose — that's expected, not a bug.

**`src/App.jsx`** is your first component. Replace its contents with this to start clean:

```jsx
function App() {
  return <h1>Hello, React!</h1>;
}

export default App;
```

Save the file. The browser updates instantly — this is called **Hot Module Replacement (HMR)**.

## Step 4 — Recommended editor setup

- Use **VS Code**.
- Install the **ESLint** extension (catches errors) and **Prettier** (auto-formats your code).

## The dev workflow you'll repeat forever

1. `npm run dev` once, leave it running.
2. Edit a file in `src/`.
3. Save → browser updates automatically.
4. Read errors in the terminal and the browser overlay.

## Try it yourself

1. Create a project with Vite.
2. Change `App.jsx` to show your name and your favorite food.
3. Cause an error on purpose (e.g. delete a closing `}`), read the error message, then fix it. Getting comfortable reading errors is a real skill.

---

[← Previous: Introduction](./01-introduction.md) · [Back to outline](../README.md) · [Next: JSX →](./03-jsx.md)
