import { useLocalStorage } from "./useLocalStorage.js";
import { useState } from "react";
import { TodoForm } from "./components/TodoForm.jsx";
import { TodoFilters } from "./components/TodoFilters.jsx";
import { TodoList } from "./components/TodoList.jsx";

// Generate a unique id, with a fallback for older environments.
function makeId() {
  if (typeof crypto !== "undefined" && crypto.randomUUID) {
    return crypto.randomUUID();
  }
  return Date.now().toString(36) + Math.random().toString(36).slice(2);
}

export default function App() {
  // The list of todos is the single source of truth, owned here in App.
  const [todos, setTodos] = useLocalStorage("todos", []);
  const [filter, setFilter] = useState("all"); // "all" | "active" | "completed"

  function addTodo(text) {
    const newTodo = { id: makeId(), text, completed: false };
    setTodos((prev) => [...prev, newTodo]);
  }

  function toggleTodo(id) {
    setTodos((prev) =>
      prev.map((t) => (t.id === id ? { ...t, completed: !t.completed } : t))
    );
  }

  function deleteTodo(id) {
    setTodos((prev) => prev.filter((t) => t.id !== id));
  }

  function clearCompleted() {
    setTodos((prev) => prev.filter((t) => !t.completed));
  }

  // Derived data: compute from state during render, don't store separately.
  const visibleTodos = todos.filter((t) => {
    if (filter === "active") return !t.completed;
    if (filter === "completed") return t.completed;
    return true;
  });
  const remaining = todos.filter((t) => !t.completed).length;
  const completedCount = todos.length - remaining;

  return (
    <main className="app">
      <h1>My Todos</h1>

      <TodoForm onAdd={addTodo} />

      <TodoFilters current={filter} onChange={setFilter} />

      <TodoList todos={visibleTodos} onToggle={toggleTodo} onDelete={deleteTodo} />

      <footer className="footer">
        <span>
          {remaining} item{remaining !== 1 ? "s" : ""} left
        </span>
        {completedCount > 0 && (
          <button className="link" onClick={clearCompleted}>
            Clear completed ({completedCount})
          </button>
        )}
      </footer>
    </main>
  );
}
