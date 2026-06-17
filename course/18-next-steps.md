# Lesson 18 — Where to go next

[← Previous: Project](./17-project-todo-app.md) · [Back to outline](../README.md)

---

## Congratulations! 🎉

You've covered the core of React:

- Components, JSX, and props
- State, events, and the render cycle
- Conditional rendering, lists, and forms
- Hooks (`useState`, `useEffect`, `useRef`, `useReducer`, `useMemo`, `useCallback`, `useContext`, and custom Hooks)
- Context, routing, data fetching, and performance
- A full Todo app built from scratch

That's genuinely enough to build real applications. Here's how to keep growing.

## Build, build, build

Reading only gets you so far. The fastest way to improve is to build projects. Ideas, roughly increasing in difficulty:

1. **Weather app** — fetch from a weather API, handle loading/error states.
2. **Markdown notes app** — CRUD + `localStorage` persistence (your custom Hook!).
3. **Movie search** — search input → API → results grid → detail page (routing + params).
4. **Expense tracker** — forms, lists, filtering, charts, `useReducer`.
5. **Kanban board** — drag and drop, more complex state.
6. **Full-stack app** — add a real backend and authentication.

> A great exercise: rebuild the same small app twice, a few weeks apart. You'll be amazed how much cleaner the second version is.

## Learn the surrounding ecosystem

You don't need all of these, but here's the modern landscape:

| Need | Popular tools |
|------|---------------|
| **Full framework** | Next.js, React Router (framework mode), Remix |
| **Data fetching** | TanStack Query, SWR |
| **Global state** | Zustand, Redux Toolkit, Jotai |
| **Forms** | React Hook Form |
| **Styling** | Tailwind CSS, CSS Modules, styled-components |
| **Component libraries** | shadcn/ui, MUI, Chakra UI, Radix |
| **Testing** | Vitest, Jest, React Testing Library, Playwright |
| **Animation** | Framer Motion |
| **Types** | TypeScript (highly recommended next!) |

## Strongly recommended next steps

1. **Learn TypeScript.** It catches a huge class of bugs and is the industry standard for React. Start by converting one of your small projects.
2. **Learn a framework — Next.js.** It handles routing, server rendering, data fetching, and deployment. Most React jobs use a framework.
3. **Learn TanStack Query.** It makes data fetching dramatically simpler and more robust than the manual `useEffect` approach.
4. **Learn testing** with Vitest + React Testing Library so you can refactor with confidence.

## Solidify your fundamentals

The official docs are excellent and modern:

- **[react.dev](https://react.dev)** — the official docs. The "Learn" section and the "Thinking in React" guide are gold.
- **[react.dev/reference](https://react.dev/reference/react)** — the full API reference for every Hook.

Concepts worth re-reading once you have experience:

- "Thinking in React" (how to break a UI into components)
- "You Might Not Need an Effect" (avoiding `useEffect` overuse)
- "Render and Commit" / "State as a Snapshot" (the mental model)

## Habits of strong React developers

- **Keep components small and focused.** One job each.
- **Lift state only as high as it needs to go** — no higher.
- **Derive, don't duplicate.** If a value can be computed from existing state, compute it; don't store it.
- **Prefer event handlers over effects** for things that happen because the user did something.
- **Name things clearly.** Future-you will thank present-you.
- **Read error messages fully.** React's errors usually tell you exactly what's wrong.

## Final word

React clicks through repetition. If something still feels fuzzy, that's normal — revisit the relevant lesson, type the examples again, and build something tiny with it. Every experienced React developer was once exactly where you are now.

Now go build something. 🚀

---

[← Previous: Project](./17-project-todo-app.md) · [Back to outline](../README.md)
