# Lesson 15 — Fetching data from an API

[← Previous: Routing](./14-routing.md) · [Back to outline](../README.md) · [Next: Performance →](./16-performance.md)

---

## Why it matters

Almost every real app talks to a server: loading products, posts, user profiles, etc. Fetching data correctly means handling three states well: **loading**, **success**, and **error**.

## The three states of any data fetch

Whenever you fetch data, plan for these three:

1. **Loading** — the request is in flight; show a spinner/skeleton.
2. **Success** — you have the data; show it.
3. **Error** — something failed; show a friendly message.

Beginners often handle only success and ship a broken experience. Always handle all three.

## Fetching with `useEffect` + `fetch`

```jsx
import { useState, useEffect } from "react";

function Posts() {
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    setLoading(true);
    setError(null);

    fetch("https://jsonplaceholder.typicode.com/posts")
      .then((res) => {
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        return res.json();
      })
      .then((data) => setPosts(data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, []);   // [] → fetch once when component mounts

  if (loading) return <p>Loading…</p>;
  if (error)   return <p>Error: {error}</p>;

  return (
    <ul>
      {posts.map((p) => <li key={p.id}>{p.title}</li>)}
    </ul>
  );
}
```

Key points:

- `fetch` does **not** throw on HTTP errors like 404/500 — you must check `res.ok` yourself.
- `.finally()` turns off loading no matter what.
- We reset `error` before each request.

## The cleaner `async/await` version

`async/await` reads more like normal step-by-step code. Note: the effect callback itself can't be `async`, so define an inner async function and call it:

```jsx
useEffect(() => {
  async function loadPosts() {
    try {
      setLoading(true);
      setError(null);
      const res = await fetch("https://jsonplaceholder.typicode.com/posts");
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const data = await res.json();
      setPosts(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  loadPosts();
}, []);
```

## Avoiding race conditions (cleanup matters)

If the thing you depend on changes quickly (e.g. a search query), an older request might finish *after* a newer one and overwrite fresh data with stale data. Guard against it with a cleanup flag:

```jsx
useEffect(() => {
  let active = true;   // is this effect still the latest?

  async function load() {
    const res = await fetch(`/api/search?q=${query}`);
    const data = await res.json();
    if (active) setResults(data);   // ignore if a newer effect ran
  }
  load();

  return () => { active = false; };   // mark stale on cleanup
}, [query]);
```

Modern code often uses `AbortController` to actually cancel the request:

```jsx
useEffect(() => {
  const controller = new AbortController();
  fetch(url, { signal: controller.signal })
    .then((r) => r.json())
    .then(setData)
    .catch((e) => { if (e.name !== "AbortError") setError(e.message); });
  return () => controller.abort();
}, [url]);
```

## POST/PUT/DELETE (sending data)

`fetch` does more than GET. To send data:

```jsx
async function createPost(title) {
  const res = await fetch("https://jsonplaceholder.typicode.com/posts", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ title }),
  });
  return res.json();
}
```

Usually you call these from **event handlers** (e.g., on form submit), not from `useEffect`.

## The modern, recommended approach: a data library

Writing loading/error/caching/race-condition logic by hand for every fetch gets repetitive and bug-prone. In real projects, most teams use a data-fetching library:

- **TanStack Query** (React Query) — the most popular choice
- **SWR**

They handle caching, re-fetching, loading/error states, and deduplication for you. Example with TanStack Query:

```jsx
import { useQuery } from "@tanstack/react-query";

function Posts() {
  const { data, isLoading, error } = useQuery({
    queryKey: ["posts"],
    queryFn: () => fetch("/api/posts").then((r) => r.json()),
  });

  if (isLoading) return <p>Loading…</p>;
  if (error) return <p>Error!</p>;
  return <ul>{data.map((p) => <li key={p.id}>{p.title}</li>)}</ul>;
}
```

Much less code, and far more robust. **Learn the manual `useEffect` way first** (so you understand what's happening), then reach for a library in real apps.

## Try it yourself

1. Fetch users from `https://jsonplaceholder.typicode.com/users` and render names, handling all three states.
2. Add a "Retry" button that re-runs the fetch on error.
3. Build a search box that fetches `/posts?userId=<n>` when a number is entered, with race-condition protection.
4. (Bonus) Install TanStack Query and refetch the user list with `useQuery`.

---

[← Previous: Routing](./14-routing.md) · [Back to outline](../README.md) · [Next: Performance →](./16-performance.md)
