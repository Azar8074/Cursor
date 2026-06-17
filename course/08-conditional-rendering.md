# Lesson 8 — Conditional rendering: showing things "if"

[← Previous: Handling events](./07-events.md) · [Back to outline](../README.md) · [Next: Lists and keys →](./09-lists-and-keys.md)

---

## Why it matters

Real UIs show different things in different situations: a "Log in" button vs a user's name, a loading spinner vs the data, an error message vs nothing. Since JSX is just JavaScript, you use normal JavaScript logic to decide what to render.

## Option 1: `if` statements (before the `return`)

You can't put an `if` *inside* JSX, but you can use one before the `return`:

```jsx
function Greeting({ isLoggedIn }) {
  if (isLoggedIn) {
    return <h1>Welcome back!</h1>;
  }
  return <h1>Please log in.</h1>;
}
```

Returning early like this is clean when the two cases are very different.

## Option 2: the ternary operator `? :` (inside JSX)

For inline choices, use a ternary — it *is* an expression, so it works inside `{ }`:

```jsx
function Greeting({ isLoggedIn }) {
  return (
    <h1>{isLoggedIn ? "Welcome back!" : "Please log in."}</h1>
  );
}
```

Pattern: `{condition ? <IfTrue /> : <IfFalse />}`. You can use full JSX on each side:

```jsx
<div>
  {isLoggedIn
    ? <button>Log out</button>
    : <button>Log in</button>}
</div>
```

## Option 3: `&&` (show something, or nothing)

When you want to render something *only if* a condition is true (and nothing otherwise), use `&&`:

```jsx
function Inbox({ unreadCount }) {
  return (
    <div>
      <h1>Inbox</h1>
      {unreadCount > 0 && <p>You have {unreadCount} unread messages.</p>}
    </div>
  );
}
```

How it works: in JavaScript `true && X` evaluates to `X`, and `false && X` evaluates to `false` (which React renders as nothing).

### ⚠️ The famous `&&` number gotcha

If the left side is the number `0`, React will literally render `0` on the screen:

```jsx
{items.length && <List />}   // ❌ shows "0" when the list is empty!
```

Fix it by making the left side a real boolean:

```jsx
{items.length > 0 && <List />}   // ✅
```

## Rendering nothing

Returning `null` renders nothing at all:

```jsx
function Warning({ show }) {
  if (!show) return null;
  return <p className="warning">Careful!</p>;
}
```

## Handling multiple cases

For more than two outcomes, an `if/else if` chain or a helper variable keeps things readable:

```jsx
function Status({ state }) {
  let content;
  if (state === "loading") {
    content = <p>Loading…</p>;
  } else if (state === "error") {
    content = <p>Something went wrong.</p>;
  } else {
    content = <p>Done!</p>;
  }

  return <div>{content}</div>;
}
```

This "assign to a variable, then render the variable" pattern is very common and clean.

## Choosing the right tool

| Situation | Use |
|-----------|-----|
| Two totally different return values | `if` with early return |
| Pick A or B inline | ternary `? :` |
| Show X or nothing | `&&` (guard against `0`!) |
| Three or more cases | `if/else if` into a variable |

## Try it yourself

1. A `LoginControl` that shows "Logout" when logged in, "Login" otherwise (ternary).
2. A notification badge that appears only when `count > 0` (`&&`, guarded).
3. A `RequestStatus` component that renders different content for `"idle"`, `"loading"`, `"success"`, `"error"`.
4. A component that returns `null` when a `visible` prop is false.

---

[← Previous: Handling events](./07-events.md) · [Back to outline](../README.md) · [Next: Lists and keys →](./09-lists-and-keys.md)
