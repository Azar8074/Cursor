# Lesson 5 — Props: passing data into components

[← Previous: Components](./04-components.md) · [Back to outline](../README.md) · [Next: State →](./06-state.md)

---

## Why it matters

In the last lesson every `ProductCard` was identical. Real apps need each card to show *different* data. **Props** are how you pass data into a component — think of them as a component's "arguments."

## Props = function arguments for components

When you use a component, you pass data using attributes:

```jsx
<Welcome name="Sara" />
<Welcome name="Ali" />
```

Inside the component, you receive a single object called `props` holding all those values:

```jsx
function Welcome(props) {
  return <h1>Hello, {props.name}!</h1>;
}
```

- `<Welcome name="Sara" />` → `props` is `{ name: "Sara" }` → renders "Hello, Sara!"
- `<Welcome name="Ali" />` → `props` is `{ name: "Ali" }` → renders "Hello, Ali!"

Same component, different data. 🎉

## Destructuring props (the cleaner, common style)

Typing `props.` everywhere gets old. Most React code "destructures" props right in the function signature:

```jsx
function Welcome({ name }) {
  return <h1>Hello, {name}!</h1>;
}
```

`{ name }` means "pull `name` out of the props object." With multiple props:

```jsx
function ProductCard({ title, price }) {
  return (
    <div className="card">
      <h2>{title}</h2>
      <p>${price}</p>
    </div>
  );
}

function App() {
  return (
    <div>
      <ProductCard title="Keyboard" price={49.99} />
      <ProductCard title="Mouse" price={19.99} />
    </div>
  );
}
```

## Passing different types of props

Strings use quotes; everything else uses curly braces `{ }`:

```jsx
<Item name="Book" />            {/* string */}
<Item price={9.99} />           {/* number */}
<Item inStock={true} />         {/* boolean */}
<Item tags={["new", "sale"]} /> {/* array */}
<Item meta={{ id: 1 }} />       {/* object */}
<Item onBuy={handleBuy} />      {/* a function! */}
```

> Shorthand: `<Item inStock />` is the same as `<Item inStock={true} />`.

## Default values for props

Give a prop a fallback using default parameter syntax:

```jsx
function Button({ label = "Click me", color = "blue" }) {
  return <button style={{ background: color }}>{label}</button>;
}

<Button />                       {/* "Click me", blue */}
<Button label="Save" />          {/* "Save", blue */}
<Button label="Delete" color="red" />
```

## The `children` prop (content between tags)

Whatever you put *between* a component's tags arrives as a special prop called `children`:

```jsx
function Card({ children }) {
  return <div className="card">{children}</div>;
}

function App() {
  return (
    <Card>
      <h2>Title</h2>
      <p>This whole block is "children".</p>
    </Card>
  );
}
```

This is incredibly useful for wrapper/layout components (cards, modals, layouts).

## The golden rule: props are read-only

A component must **never modify its own props**. Props flow *down* from parent to child, and the child treats them as read-only inputs.

```jsx
function Bad({ count }) {
  count = count + 1;   // ❌ never reassign/mutate props
  return <p>{count}</p>;
}
```

If a component needs data that *changes*, that's **state** — our next lesson. The rule of thumb:

> **Props** = data passed *in* from a parent (read-only).
> **State** = data a component *owns* and can change.

## Data flows one way (down)

React has **one-way data flow**: data goes from parent → child via props. A child cannot reach up and change a parent's data directly. To let a child *trigger* a change, the parent passes down a **function** as a prop:

```jsx
function Parent() {
  function handleClick() {
    alert("Child clicked!");
  }
  return <Child onAction={handleClick} />;
}

function Child({ onAction }) {
  return <button onClick={onAction}>Tell parent</button>;
}
```

This pattern — "pass data down, pass events up" — is everywhere in React.

## Try it yourself

1. Build a `Profile` component taking `name`, `role`, and `avatarUrl` props.
2. Render three different profiles in `App`.
3. Add a default value for `role` (e.g. `"Member"`).
4. Build a `Panel` component that renders its `children` inside a styled box.

---

[← Previous: Components](./04-components.md) · [Back to outline](../README.md) · [Next: State →](./06-state.md)
