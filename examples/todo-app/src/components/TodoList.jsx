import { TodoItem } from "./TodoItem.jsx";

export function TodoList({ todos, onToggle, onDelete }) {
  // Conditional rendering: handle the empty state gracefully.
  if (todos.length === 0) {
    return <p className="empty">Nothing here yet.</p>;
  }

  return (
    <ul className="list">
      {todos.map((todo) => (
        <TodoItem
          key={todo.id}
          todo={todo}
          onToggle={onToggle}
          onDelete={onDelete}
        />
      ))}
    </ul>
  );
}
