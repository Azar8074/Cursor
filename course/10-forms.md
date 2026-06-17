# Lesson 10 — Forms and inputs

[← Previous: Lists and keys](./09-lists-and-keys.md) · [Back to outline](../README.md) · [Next: useEffect →](./11-useeffect.md)

---

## Why it matters

Forms are how users give you data: logins, search boxes, comments, settings. React handles forms a little differently from plain HTML, using a pattern called **controlled components**.

## Controlled components: React owns the value

The idea: the input's value is stored in **state**, and the input always displays that state. The flow is a loop:

1. The input's `value` comes from state.
2. When the user types, `onChange` updates the state.
3. New state → re-render → input shows the new value.

```jsx
import { useState } from "react";

function NameForm() {
  const [name, setName] = useState("");

  return (
    <div>
      <input
        type="text"
        value={name}
        onChange={(e) => setName(e.target.value)}
      />
      <p>Hello, {name || "stranger"}!</p>
    </div>
  );
}
```

This is called "controlled" because **React state is the single source of truth** for what's in the input. The greeting updates live as you type.

> **Why bother?** Because the value lives in state, you can validate it, transform it, reset it, or use it elsewhere — all easily.

## Different input types

### Text / email / password / textarea

All work the same way (`value` + `onChange`):

```jsx
<input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
<textarea value={bio} onChange={(e) => setBio(e.target.value)} />
```

### Checkbox — use `checked`, not `value`

```jsx
const [agreed, setAgreed] = useState(false);

<input
  type="checkbox"
  checked={agreed}
  onChange={(e) => setAgreed(e.target.checked)}
/>
```

Note: checkboxes read `e.target.checked` (a boolean), not `e.target.value`.

### Select dropdown

```jsx
const [color, setColor] = useState("red");

<select value={color} onChange={(e) => setColor(e.target.value)}>
  <option value="red">Red</option>
  <option value="green">Green</option>
  <option value="blue">Blue</option>
</select>
```

## Handling many fields with one state object

A form with several fields can share one state object. Use the input's `name` attribute to know which field changed:

```jsx
function SignupForm() {
  const [form, setForm] = useState({ username: "", email: "", password: "" });

  function handleChange(e) {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  }

  return (
    <form>
      <input name="username" value={form.username} onChange={handleChange} />
      <input name="email" value={form.email} onChange={handleChange} />
      <input name="password" type="password" value={form.password} onChange={handleChange} />
    </form>
  );
}
```

The trick is `[name]: value` — a **computed property name**. It updates exactly the field whose `name` matches, while `...prev` keeps the others.

## Handling submit

Put `onSubmit` on the `<form>` (not `onClick` on the button) and call `preventDefault` to stop the page reload:

```jsx
function LoginForm() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  function handleSubmit(e) {
    e.preventDefault();
    console.log("Logging in:", email, password);
    // ...send to server...
    setPassword("");   // reset a field if you like
  }

  return (
    <form onSubmit={handleSubmit}>
      <input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Email" />
      <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Password" />
      <button type="submit">Log in</button>
    </form>
  );
}
```

## Simple validation

Because values live in state, validating is just JavaScript:

```jsx
const emailIsValid = email.includes("@");

return (
  <form onSubmit={handleSubmit}>
    <input value={email} onChange={(e) => setEmail(e.target.value)} />
    {!emailIsValid && email.length > 0 && (
      <p style={{ color: "red" }}>Please enter a valid email.</p>
    )}
    <button type="submit" disabled={!emailIsValid}>Submit</button>
  </form>
);
```

## A note on bigger forms

For large, complex forms, libraries like **React Hook Form** or **Formik** reduce boilerplate and handle validation elegantly. But understanding controlled components first is essential — those libraries build on the same ideas.

## Try it yourself

1. A live "character counter" textarea that shows how many characters were typed.
2. A signup form (username, email, password) using one state object + `handleChange`.
3. A checkbox "I agree to terms" that enables/disables the submit button.
4. On submit, `preventDefault`, log the form data, and clear the fields.

---

[← Previous: Lists and keys](./09-lists-and-keys.md) · [Back to outline](../README.md) · [Next: useEffect →](./11-useeffect.md)
