# Lesson 3 — JSX: writing HTML inside JavaScript

[← Previous: Environment setup](./02-environment-setup.md) · [Back to outline](../README.md) · [Next: Components →](./04-components.md)

---

## Why it matters

The HTML-looking code inside React components is called **JSX**. It looks like HTML, but it's actually JavaScript in disguise. Understanding JSX removes 90% of "why doesn't this work?" confusion for beginners.

## What is JSX?

JSX lets you write markup directly in your JavaScript:

```jsx
const element = <h1>Hello, world!</h1>;
```

That's not a string and it's not HTML — it's an *expression* that React turns into UI. Behind the scenes a tool (Babel) converts it into normal function calls. You don't need to know the details, but it helps to know JSX is **just JavaScript**.

## The rules of JSX (read these carefully — they trip up everyone)

### Rule 1: Return one parent element

A component must return a **single** top-level element. This is wrong:

```jsx
return (
  <h1>Title</h1>
  <p>Paragraph</p>   // ❌ two elements side by side
);
```

Wrap them in one parent. Use a real element like `<div>`, or an empty **Fragment** `<>...</>` when you don't want extra HTML:

```jsx
return (
  <>
    <h1>Title</h1>
    <p>Paragraph</p>
  </>
);
```

### Rule 2: Close every tag

Even tags that are "self-closing" in HTML must end with `/>`:

```jsx
<img src="cat.jpg" alt="A cat" />   // ✅
<br />                               // ✅
<input type="text" />               // ✅
```

### Rule 3: `className` instead of `class`

Because `class` is a reserved word in JavaScript, React uses `className`:

```jsx
<div className="card">...</div>
```

Similarly, `for` (on labels) becomes `htmlFor`.

### Rule 4: camelCase for most attributes

HTML attributes that have dashes or are multi-word become camelCase:

```jsx
<button onClick={handleClick}>Click</button>   // onclick → onClick
<label htmlFor="name">Name</label>
<div tabIndex={0} />                            // tabindex → tabIndex
```

## Putting JavaScript inside JSX with `{ }`

Curly braces are a "window" back into JavaScript. Anything inside `{ }` is evaluated as a JavaScript **expression**:

```jsx
const name = "Sara";
const element = <h1>Hello, {name}!</h1>;   // → Hello, Sara!
```

You can put any expression in there:

```jsx
<p>{2 + 2}</p>                       {/* 4 */}
<p>{user.firstName + " " + user.lastName}</p>
<p>{isLoggedIn ? "Welcome back" : "Please log in"}</p>
<img src={user.avatarUrl} />         {/* attribute value from a variable */}
```

> **Expression vs statement:** Only *expressions* (things that produce a value) work inside `{ }`. You **cannot** put an `if` statement or a `for` loop directly inside JSX. We'll learn the workarounds in later lessons (ternaries, `.map()`, etc.).

### A common gotcha

Curly braces for *attribute values* don't use quotes:

```jsx
<img src={imageUrl} />     // ✅ value from a variable
<img src="imageUrl" />     // ❌ literally the text "imageUrl"
```

## Comments in JSX

Use `{/* ... */}`:

```jsx
return (
  <div>
    {/* This is a comment inside JSX */}
    <p>Hello</p>
  </div>
);
```

## Styling inline (the `style` prop is an object)

Inline styles take a JavaScript object, with camelCased property names:

```jsx
<p style={{ color: "blue", fontSize: "20px" }}>Styled text</p>
```

The **double braces** look weird but are simple: the outer `{ }` is "enter JavaScript," and the inner `{ }` is the object literal. For real apps you'll usually use CSS files or `className` instead.

## Try it yourself

Create a component that renders, using one variable each:

1. A heading with your name pulled from a `const name`.
2. A paragraph showing the result of `10 * 5` using `{ }`.
3. An image using `src={someUrl}`.
4. Wrap everything in a Fragment `<>...</>` and give one element a `className`.

---

[← Previous: Environment setup](./02-environment-setup.md) · [Back to outline](../README.md) · [Next: Components →](./04-components.md)
