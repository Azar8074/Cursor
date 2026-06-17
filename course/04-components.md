# Lesson 4 — Components: the building blocks

[← Previous: JSX](./03-jsx.md) · [Back to outline](../README.md) · [Next: Props →](./05-props.md)

---

## Why it matters

Everything you build in React is made of **components**. A component is a reusable, self-contained piece of UI. Once you "think in components," React makes sense.

## What is a component?

A component is **a JavaScript function that returns JSX**. That's it.

```jsx
function Welcome() {
  return <h1>Welcome to my app!</h1>;
}
```

Two rules:

1. **The name must start with a capital letter** (`Welcome`, not `welcome`). React uses this to tell your components apart from regular HTML tags (`<div>` vs `<Welcome>`).
2. **It returns JSX** (or `null` to render nothing).

## Using a component

You use a component like an HTML tag:

```jsx
function App() {
  return (
    <div>
      <Welcome />
      <Welcome />
      <Welcome />
    </div>
  );
}
```

This renders the welcome message three times. **Reusability** — write once, use anywhere.

## Splitting UI into components

Think of a webpage as a tree of components. A typical page:

```
App
├── Navbar
├── Sidebar
├── ProductList
│   ├── ProductCard
│   ├── ProductCard
│   └── ProductCard
└── Footer
```

Each box is a component. Small components combine into bigger ones. This makes your code:

- **Easier to read** — each file does one thing.
- **Easier to reuse** — drop `ProductCard` anywhere.
- **Easier to test and fix** — bugs are isolated.

## Example: composing components

```jsx
function Header() {
  return <header><h1>My Store</h1></header>;
}

function Footer() {
  return <footer><p>© 2025 My Store</p></footer>;
}

function ProductCard() {
  return (
    <div className="card">
      <h2>A cool product</h2>
      <p>$19.99</p>
    </div>
  );
}

function App() {
  return (
    <div>
      <Header />
      <ProductCard />
      <ProductCard />
      <Footer />
    </div>
  );
}

export default App;
```

Right now every `ProductCard` is identical. In the next lesson (**Props**) we'll learn to pass different data into each one.

## One component per file (the common convention)

Most projects put each major component in its own file and `export` it:

```jsx
// Header.jsx
function Header() {
  return <header><h1>My Store</h1></header>;
}

export default Header;
```

```jsx
// App.jsx
import Header from "./Header.jsx";

function App() {
  return <Header />;
}

export default App;
```

- `export default` makes the component available to other files.
- `import Header from "./Header.jsx"` brings it in. The name after `import` is yours to choose (but match the component for sanity).

## Components must be "pure" (an important habit)

A component should be like a math function: **given the same inputs, it returns the same output**, and it should **not** change things outside of itself while rendering.

```jsx
// ❌ Bad: modifying an outside variable during render
let count = 0;
function Bad() {
  count++;               // side effect during render — don't do this
  return <p>{count}</p>;
}
```

Keep rendering predictable. Anything that *changes* things (timers, fetching, etc.) has a proper home we'll cover in the `useEffect` lesson.

## Try it yourself

1. Create a `Greeting` component that returns "Hello!".
2. Create a `Card` component with a title and a paragraph.
3. Create an `App` that shows one `Greeting` and three `Card`s.
4. Move `Card` into its own file and import it.

---

[← Previous: JSX](./03-jsx.md) · [Back to outline](../README.md) · [Next: Props →](./05-props.md)
