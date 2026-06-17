const OPTIONS = ["all", "active", "completed"];

export function TodoFilters({ current, onChange }) {
  return (
    <div className="filters" role="group" aria-label="Filter todos">
      {OPTIONS.map((opt) => (
        <button
          key={opt}
          type="button"
          onClick={() => onChange(opt)}
          className={current === opt ? "active" : ""}
          aria-pressed={current === opt}
        >
          {opt}
        </button>
      ))}
    </div>
  );
}
