import { useState, useEffect } from "react";

// A custom Hook that works just like useState, but persists the value
// to localStorage so it survives page reloads. (See Lesson 12.)
export function useLocalStorage(key, initialValue) {
  const [value, setValue] = useState(() => {
    try {
      const stored = localStorage.getItem(key);
      return stored ? JSON.parse(stored) : initialValue;
    } catch {
      return initialValue;
    }
  });

  useEffect(() => {
    try {
      localStorage.setItem(key, JSON.stringify(value));
    } catch {
      // Ignore write errors (e.g. storage full or unavailable).
    }
  }, [key, value]);

  return [value, setValue];
}
