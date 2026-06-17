# Lesson 9 — Lists and keys: rendering many items

[← Previous: Conditional rendering](./08-conditional-rendering.md) · [Back to outline](../README.md) · [Next: Forms →](./10-forms.md)

---

## Why it matters

Almost every app shows lists: products, messages, todos, search results. You'll render these from arrays. The tool is JavaScript's `.map()`, plus a special prop called `key`.

## Rendering a list with `.map()`

`.map()` transforms an array of data into an array of JSX elements:

```jsx
function FruitList() {
  const fruits = ["Apple", "Banana", "Cherry"];

  return (
    <ul>
      {fruits.map((fruit) => (
        <li key={fruit}>{fruit}</li>
      ))}
    </ul>
  );
}
```

This produces three `<li>` elements. Inside JSX, `{array.map(...)}` is the standard way to render many things.

## Mapping over arrays of objects (the realistic case)

Usually your data is objects, not plain strings:

```jsx
function UserList() {
  const users = [
    { id: 1, name: "Sara", role: "Admin" },
    { id: 2, name: "Ali", role: "Member" },
    { id: 3, name: "Maya", role: "Member" },
  ];

  return (
    <ul>
      {users.map((user) => (
        <li key={user.id}>
          {user.name} — {user.role}
        </li>
      ))}
    </ul>
  );
}
```

## What is a `key` and why is it required?

Notice `key={user.id}`. The `key` is a special prop that gives each list item a **stable, unique identity**. React uses keys to track which items changed, were added, or were removed, so it can update the screen efficiently and correctly.

If you forget keys, React works but warns you in the console: *"Each child in a list should have a unique key prop."*

### Rules for good keys

1. **Unique among siblings** — no two items in the same list share a key.
2. **Stable** — the same item keeps the same key across renders.
3. Use a real **ID** from your data whenever possible (`user.id`, `product.sku`).

### ⚠️ Don't use the array index as a key (usually)

```jsx
{users.map((user, index) => (
  <li key={index}>{user.name}</li>   // ❌ risky
))}
```

Using `index` causes bugs when the list can be **reordered, filtered, or have items inserted/removed**, because the index of an item changes. React may then associate the wrong state/DOM with the wrong item (e.g., text typed in one input jumps to another). Only use the index if the list is static and never reorders — and even then, a real ID is safer.

If your data has no ID, generate one when you create the item:

```jsx
const newItem = { id: crypto.randomUUID(), text: "..." };
```

## Filtering and transforming before mapping

Because it's all JavaScript, you can chain array methods:

```jsx
{users
  .filter((u) => u.role === "Member")
  .map((u) => <li key={u.id}>{u.name}</li>)}
```

## Rendering a component for each item

Combine lists with components and props:

```jsx
function ProductCard({ product }) {
  return (
    <div className="card">
      <h3>{product.name}</h3>
      <p>${product.price}</p>
    </div>
  );
}

function Shop({ products }) {
  return (
    <div className="grid">
      {products.map((p) => (
        <ProductCard key={p.id} product={p} />
      ))}
    </div>
  );
}
```

> Put the `key` on the **outermost element returned by `.map()`** — here that's `<ProductCard>`, not something inside it.

## Handling empty lists

Always consider the "no items" case:

```jsx
{products.length === 0
  ? <p>No products found.</p>
  : products.map((p) => <ProductCard key={p.id} product={p} />)}
```

## Try it yourself

1. Render a `<ul>` of your five favorite movies from an array of strings.
2. Make an array of objects `{ id, name, price }` and render a card for each.
3. Add a filter so only items with `price < 50` show.
4. Show "No items" when the array is empty.
5. Explain to yourself why `key={index}` would break a reorderable list.

---

[← Previous: Conditional rendering](./08-conditional-rendering.md) · [Back to outline](../README.md) · [Next: Forms →](./10-forms.md)
