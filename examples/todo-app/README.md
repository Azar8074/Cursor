# Todo App — Course Example

This is the complete, runnable companion app for **[Lesson 17: Build a Todo app step by step](../../course/17-project-todo-app.md)**.

It demonstrates, in one small app, most of what the course covers:

- Components & composition (`TodoForm`, `TodoFilters`, `TodoList`, `TodoItem`)
- Props (data down) and callbacks (events up)
- `useState` and **lifting state up** to `App`
- Immutable state updates (`...spread`, `.map`, `.filter`)
- A controlled form
- Lists & `key`s
- Conditional rendering (empty state, "clear completed")
- Derived state (filtered list, remaining count)
- A custom Hook with `useEffect` (`useLocalStorage`) for persistence

## Run it

You need **Node.js 18+** (`node -v` to check).

```bash
npm install
npm run dev
```

Open the URL it prints (usually `http://localhost:5173`).

## Build for production

```bash
npm run build      # outputs to dist/
npm run preview    # preview the production build locally
```

## File tour

```
src/
├── main.jsx                 # mounts <App> into the page
├── App.jsx                  # owns todo state + the core logic
├── useLocalStorage.js       # custom Hook: useState that persists
├── index.css                # styling
└── components/
    ├── TodoForm.jsx         # controlled input + add button
    ├── TodoFilters.jsx      # All / Active / Completed
    ├── TodoList.jsx         # maps todos → TodoItem (+ empty state)
    └── TodoItem.jsx         # one todo row (toggle + delete)
```

Try the "extend the app" challenges at the end of Lesson 17!
