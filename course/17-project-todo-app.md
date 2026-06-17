# Lesson 17 — Project: Build a Todo app step by step

[← Previous: Performance](./16-performance.md) · [Back to outline](../README.md) · [Next: Where to go next →](./18-next-steps.md)

---

## Why it matters

Time to combine everything: components, props, state, events, lists, keys, conditional rendering, forms, `useEffect`, and a custom Hook. We'll build a **Todo app** with add, toggle, delete, filtering, and persistence.

A full, runnable version of this exact app lives in [`examples/todo-app`](../examples/todo-app). Run it with:

```bash
cd examples/todo-app
npm install
npm run dev
```

Below, we build it piece by piece so you understand every line.

## What we're building

- Add a todo
- Mark it complete (toggle)
- Delete a todo
- Filter: All / Active / Completed
- A live count of remaining items
- Persist to `localStorage` so todos survive a refresh

## Step 1: Plan the components

Breaking the UI into components first ("Thinking in React"):

```
App                 ← owns the todo state
├── TodoForm        ← input + add button
├── TodoFilters     ← All / Active / Completed buttons
├── TodoList        ← maps over todos
│   └── TodoItem    ← one todo (checkbox, text, delete)
└── (footer)        ← items-left count
```

**Where does state live?** The list of todos is needed by `TodoList`, the count, and the filters. So it lives in their closest common parent: `App`. This is called **lifting state up**.

## Step 2: The data shape

Each todo is an object:

```js
{ id: "abc123", text: "Learn React", completed: false }
```

## Step 3: `App` — owning the state

```jsx
import { useState } from "react";

export default function App() {
  const [todos, setTodos] = useState([]);
  const [filter, setFilter] = useState("all"); // "all" | "active" | "completed"

  function addTodo(text) {
    const newTodo = { id: crypto.randomUUID(), text, completed: false };
    setTodos((prev) => [...prev, newTodo]);   // copy + add (never mutate)
  }

  function toggleTodo(id) {
    setTodos((prev) =>
      prev.map((t) => (t.id === id ? { ...t, completed: !t.completed } : t))
    );
  }

  function deleteTodo(id) {
    setTodos((prev) => prev.filter((t) => t.id !== id));
  }

  // Derived data — compute, don't store in state
  const visibleTodos = todos.filter((t) => {
    if (filter === "active") return !t.completed;
    if (filter === "completed") return t.completed;
    return true;
  });
  const remaining = todos.filter((t) => !t.completed).length;

  return (
    <div className="app">
      <h1>My Todos</h1>
      <TodoForm onAdd={addTodo} />
      <TodoFilters current={filter} onChange={setFilter} />
      <TodoList todos={visibleTodos} onToggle={toggleTodo} onDelete={deleteTodo} />
      <p>{remaining} item{remaining !== 1 ? "s" : ""} left</p>
    </div>
  );
}
```

Notice:

- All three updaters **copy** state (`...prev`, `.map`, `.filter`) — never mutate (Lesson 6).
- `visibleTodos` and `remaining` are **derived** from state during render — we don't store them separately (avoids bugs and stale data).
- `App` passes data **down** (props) and receives changes **up** (callback functions) — the core React data flow.

## Step 4: `TodoForm` — a controlled input

```jsx
import { useState } from "react";

function TodoForm({ onAdd }) {
  const [text, setText] = useState("");

  function handleSubmit(e) {
    e.preventDefault();
    const trimmed = text.trim();
    if (!trimmed) return;     // ignore empty
    onAdd(trimmed);           // tell the parent
    setText("");              // clear the input
  }

  return (
    <form onSubmit={handleSubmit}>
      <input
        value={text}
        onChange={(e) => setText(e.target.value)}
        placeholder="What needs to be done?"
      />
      <button type="submit">Add</button>
    </form>
  );
}
```

This is a **controlled component** (Lesson 10). `TodoForm` owns only its own input text; the actual todo list belongs to `App`.

## Step 5: `TodoFilters`

```jsx
function TodoFilters({ current, onChange }) {
  const options = ["all", "active", "completed"];
  return (
    <div className="filters">
      {options.map((opt) => (
        <button
          key={opt}
          onClick={() => onChange(opt)}
          className={current === opt ? "active" : ""}
        >
          {opt}
        </button>
      ))}
    </div>
  );
}
```

## Step 6: `TodoList` and `TodoItem`

```jsx
function TodoList({ todos, onToggle, onDelete }) {
  if (todos.length === 0) {
    return <p className="empty">Nothing here yet.</p>;   // conditional rendering
  }
  return (
    <ul className="list">
      {todos.map((todo) => (
        <TodoItem
          key={todo.id}                 // stable, unique key (Lesson 9)
          todo={todo}
          onToggle={onToggle}
          onDelete={onDelete}
        />
      ))}
    </ul>
  );
}

function TodoItem({ todo, onToggle, onDelete }) {
  return (
    <li className={todo.completed ? "done" : ""}>
      <label>
        <input
          type="checkbox"
          checked={todo.completed}
          onChange={() => onToggle(todo.id)}
        />
        <span>{todo.text}</span>
      </label>
      <button onClick={() => onDelete(todo.id)}>✕</button>
    </li>
  );
}
```

Each item only knows about *itself* and calls the callbacks from `App` with its own `id`. This "pass the id back up" pattern keeps `App` as the single source of truth.

## Step 7: Persistence with a custom Hook

Let's make todos survive a page refresh using the `useLocalStorage` Hook from Lesson 12:

```jsx
import { useState, useEffect } from "react";

function useLocalStorage(key, initialValue) {
  const [value, setValue] = useState(() => {
    const stored = localStorage.getItem(key);
    return stored ? JSON.parse(stored) : initialValue;
  });

  useEffect(() => {
    localStorage.setItem(key, JSON.stringify(value));
  }, [key, value]);

  return [value, setValue];
}
```

Then in `App`, swap one line:

```jsx
// const [todos, setTodos] = useState([]);
const [todos, setTodos] = useLocalStorage("todos", []);
```

That's it — because the custom Hook has the same `[value, setValue]` shape as `useState`, everything else just works. Add some todos, refresh the page, and they're still there. ✨

## What you just used

| Concept | Where |
|---------|-------|
| Components & composition | every file |
| Props (data down) | `todos`, `current`, `todo` |
| Callbacks (events up) | `onAdd`, `onToggle`, `onDelete`, `onChange` |
| State + lifting state up | `todos`/`filter` in `App` |
| Immutable updates | `...prev`, `.map`, `.filter` |
| Controlled form | `TodoForm` |
| Lists & keys | `TodoList` |
| Conditional rendering | empty state, "items left" |
| Derived state | `visibleTodos`, `remaining` |
| `useEffect` + custom Hook | `useLocalStorage` |

## Try it yourself (extend the app)

1. Add an **"Edit"** feature (double-click to rename a todo).
2. Add a **"Clear completed"** button.
3. Add a **count** next to each filter (e.g., "Active (3)").
4. Persist the selected `filter` too.
5. Add a due date and sort by it.

See the complete working source in [`examples/todo-app/src`](../examples/todo-app/src).

---

[← Previous: Performance](./16-performance.md) · [Back to outline](../README.md) · [Next: Where to go next →](./18-next-steps.md)
