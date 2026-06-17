# Coding Challenges & Solutions (C#)

Hands-on problems commonly asked in C#/.NET interviews. Try to solve each yourself **before**
reading the solution. Solutions favor clarity and idiomatic C#.

## Table of Contents

1. [Strings](#strings)
2. [Arrays & Numbers](#arrays--numbers)
3. [Collections & LINQ](#collections--linq)
4. [Recursion](#recursion)
5. [OOP Design](#oop-design)
6. [Algorithms](#algorithms)
7. [Async](#async)

---

## Strings

### 1. Reverse a string (without `Array.Reverse`)

```csharp
public static string Reverse(string input)
{
    char[] chars = input.ToCharArray();
    int left = 0, right = chars.Length - 1;
    while (left < right)
    {
        (chars[left], chars[right]) = (chars[right], chars[left]);
        left++;
        right--;
    }
    return new string(chars);
}
```

### 2. Check if a string is a palindrome

```csharp
public static bool IsPalindrome(string s)
{
    s = s.ToLower().Where(char.IsLetterOrDigit).Aggregate("", (a, c) => a + c);
    int i = 0, j = s.Length - 1;
    while (i < j)
        if (s[i++] != s[j--]) return false;
    return true;
}
```

### 3. Count the occurrences of each character

```csharp
public static Dictionary<char, int> CharFrequency(string s) =>
    s.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
```

### 4. Find the first non-repeating character

```csharp
public static char? FirstUnique(string s)
{
    var counts = s.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
    foreach (char c in s)
        if (counts[c] == 1) return c;
    return null;
}
```

### 5. Check if two strings are anagrams

```csharp
public static bool AreAnagrams(string a, string b)
{
    string Normalize(string s) =>
        new string(s.ToLower().Where(char.IsLetter).OrderBy(c => c).ToArray());
    return Normalize(a) == Normalize(b);
}
```

### 6. Count words in a sentence

```csharp
public static int WordCount(string sentence) =>
    sentence.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
```

---

## Arrays & Numbers

### 7. FizzBuzz

```csharp
for (int i = 1; i <= 100; i++)
{
    string output = (i % 3, i % 5) switch
    {
        (0, 0) => "FizzBuzz",
        (0, _) => "Fizz",
        (_, 0) => "Buzz",
        _      => i.ToString()
    };
    Console.WriteLine(output);
}
```

### 8. Find the maximum and minimum in an array

```csharp
public static (int min, int max) MinMax(int[] arr)
{
    int min = arr[0], max = arr[0];
    foreach (int n in arr)
    {
        if (n < min) min = n;
        if (n > max) max = n;
    }
    return (min, max);
}
```

### 9. Find the second largest number

```csharp
public static int SecondLargest(int[] arr)
{
    int first = int.MinValue, second = int.MinValue;
    foreach (int n in arr)
    {
        if (n > first) { second = first; first = n; }
        else if (n > second && n != first) second = n;
    }
    return second;
}
```

### 10. Check if a number is prime

```csharp
public static bool IsPrime(int n)
{
    if (n < 2) return false;
    for (int i = 2; i <= Math.Sqrt(n); i++)
        if (n % i == 0) return false;
    return true;
}
```

### 11. Find duplicates in an array

```csharp
public static IEnumerable<int> FindDuplicates(int[] arr)
{
    var seen = new HashSet<int>();
    var dups = new HashSet<int>();
    foreach (int n in arr)
        if (!seen.Add(n)) dups.Add(n);
    return dups;
}
```

### 12. Two Sum — find indices that add up to a target

```csharp
public static int[] TwoSum(int[] nums, int target)
{
    var map = new Dictionary<int, int>();
    for (int i = 0; i < nums.Length; i++)
    {
        int complement = target - nums[i];
        if (map.TryGetValue(complement, out int j))
            return new[] { j, i };
        map[nums[i]] = i;
    }
    return Array.Empty<int>();
}
```

### 13. Reverse an integer

```csharp
public static int ReverseInt(int x)
{
    int result = 0;
    while (x != 0)
    {
        result = result * 10 + x % 10;
        x /= 10;
    }
    return result;
}
```

### 14. Rotate an array by k positions

```csharp
public static void Rotate(int[] nums, int k)
{
    k %= nums.Length;
    Array.Reverse(nums, 0, nums.Length);
    Array.Reverse(nums, 0, k);
    Array.Reverse(nums, k, nums.Length - k);
}
```

---

## Collections & LINQ

### 15. Group a list of people by age range

```csharp
var groups = people
    .GroupBy(p => p.Age / 10 * 10)
    .Select(g => new { Decade = $"{g.Key}s", Count = g.Count() })
    .OrderBy(g => g.Decade);
```

### 16. Remove duplicates while preserving order

```csharp
public static List<T> DistinctPreserveOrder<T>(IEnumerable<T> source)
{
    var seen = new HashSet<T>();
    var result = new List<T>();
    foreach (var item in source)
        if (seen.Add(item)) result.Add(item);
    return result;
}
```

### 17. Find the top N most frequent elements

```csharp
public static IEnumerable<T> TopN<T>(IEnumerable<T> items, int n) =>
    items.GroupBy(x => x)
         .OrderByDescending(g => g.Count())
         .Take(n)
         .Select(g => g.Key);
```

### 18. Flatten a list of lists

```csharp
var flat = listOfLists.SelectMany(inner => inner).ToList();
```

### 19. Sum of even numbers and product of odd numbers

```csharp
var nums = Enumerable.Range(1, 10);
int sumEven = nums.Where(n => n % 2 == 0).Sum();
int productOdd = nums.Where(n => n % 2 != 0).Aggregate(1, (acc, n) => acc * n);
```

### 20. Paginate a collection

```csharp
public static IEnumerable<T> Page<T>(IEnumerable<T> source, int page, int size) =>
    source.Skip((page - 1) * size).Take(size);
```

---

## Recursion

### 21. Factorial

```csharp
public static long Factorial(int n) => n <= 1 ? 1 : n * Factorial(n - 1);
```

### 22. Fibonacci (memoized)

```csharp
public static long Fib(int n, Dictionary<int, long>? memo = null)
{
    memo ??= new Dictionary<int, long>();
    if (n < 2) return n;
    if (memo.TryGetValue(n, out long cached)) return cached;
    long result = Fib(n - 1, memo) + Fib(n - 2, memo);
    memo[n] = result;
    return result;
}
```

### 23. Greatest Common Divisor (Euclid)

```csharp
public static int Gcd(int a, int b) => b == 0 ? a : Gcd(b, a % b);
```

### 24. Sum of digits

```csharp
public static int DigitSum(int n) =>
    n == 0 ? 0 : n % 10 + DigitSum(n / 10);
```

### 25. Power (a^b)

```csharp
public static long Power(int a, int b) => b == 0 ? 1 : a * Power(a, b - 1);
```

---

## OOP Design

### 26. Singleton (thread-safe with `Lazy<T>`)

```csharp
public sealed class Logger
{
    private static readonly Lazy<Logger> _instance = new(() => new Logger());
    public static Logger Instance => _instance.Value;
    private Logger() { }
    public void Log(string message) => Console.WriteLine($"[LOG] {message}");
}
```

### 27. Simple Stack implementation

```csharp
public class MyStack<T>
{
    private readonly List<T> _items = new();
    public int Count => _items.Count;
    public bool IsEmpty => _items.Count == 0;

    public void Push(T item) => _items.Add(item);

    public T Pop()
    {
        if (IsEmpty) throw new InvalidOperationException("Stack is empty");
        T item = _items[^1];
        _items.RemoveAt(_items.Count - 1);
        return item;
    }

    public T Peek() => IsEmpty
        ? throw new InvalidOperationException("Stack is empty")
        : _items[^1];
}
```

### 28. Strategy pattern (pluggable discount)

```csharp
public interface IDiscount { decimal Apply(decimal price); }
public class NoDiscount : IDiscount { public decimal Apply(decimal p) => p; }
public class PercentDiscount : IDiscount
{
    private readonly decimal _pct;
    public PercentDiscount(decimal pct) => _pct = pct;
    public decimal Apply(decimal p) => p * (1 - _pct / 100);
}

public class Checkout
{
    private readonly IDiscount _discount;
    public Checkout(IDiscount discount) => _discount = discount;
    public decimal Total(decimal price) => _discount.Apply(price);
}
```

### 29. Builder pattern (fluent)

```csharp
public class PizzaBuilder
{
    private readonly List<string> _toppings = new();
    private string _size = "Medium";

    public PizzaBuilder Size(string size) { _size = size; return this; }
    public PizzaBuilder AddTopping(string t) { _toppings.Add(t); return this; }
    public string Build() => $"{_size} pizza with {string.Join(", ", _toppings)}";
}

var pizza = new PizzaBuilder().Size("Large").AddTopping("Cheese").AddTopping("Mushroom").Build();
```

---

## Algorithms

### 30. Binary search

```csharp
public static int BinarySearch(int[] sorted, int target)
{
    int low = 0, high = sorted.Length - 1;
    while (low <= high)
    {
        int mid = low + (high - low) / 2;
        if (sorted[mid] == target) return mid;
        if (sorted[mid] < target) low = mid + 1;
        else high = mid - 1;
    }
    return -1;
}
```

### 31. Bubble sort

```csharp
public static void BubbleSort(int[] arr)
{
    for (int i = 0; i < arr.Length - 1; i++)
        for (int j = 0; j < arr.Length - 1 - i; j++)
            if (arr[j] > arr[j + 1])
                (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
}
```

### 32. Quick sort

```csharp
public static void QuickSort(int[] arr, int low, int high)
{
    if (low >= high) return;
    int pivot = arr[high], i = low - 1;
    for (int j = low; j < high; j++)
        if (arr[j] < pivot)
            (arr[++i], arr[j]) = (arr[j], arr[i]);
    (arr[i + 1], arr[high]) = (arr[high], arr[i + 1]);
    int p = i + 1;
    QuickSort(arr, low, p - 1);
    QuickSort(arr, p + 1, high);
}
```

### 33. Check for balanced parentheses

```csharp
public static bool IsBalanced(string s)
{
    var stack = new Stack<char>();
    var pairs = new Dictionary<char, char> { [')'] = '(', [']'] = '[', ['}'] = '{' };
    foreach (char c in s)
    {
        if (c is '(' or '[' or '{') stack.Push(c);
        else if (pairs.ContainsKey(c))
            if (stack.Count == 0 || stack.Pop() != pairs[c]) return false;
    }
    return stack.Count == 0;
}
```

### 34. Count vowels and consonants

```csharp
public static (int vowels, int consonants) CountLetters(string s)
{
    const string vowels = "aeiou";
    int v = 0, c = 0;
    foreach (char ch in s.ToLower())
        if (char.IsLetter(ch))
            if (vowels.Contains(ch)) v++; else c++;
    return (v, c);
}
```

### 35. Merge two sorted arrays

```csharp
public static int[] Merge(int[] a, int[] b)
{
    var result = new int[a.Length + b.Length];
    int i = 0, j = 0, k = 0;
    while (i < a.Length && j < b.Length)
        result[k++] = a[i] <= b[j] ? a[i++] : b[j++];
    while (i < a.Length) result[k++] = a[i++];
    while (j < b.Length) result[k++] = b[j++];
    return result;
}
```

---

## Async

### 36. Run tasks concurrently and aggregate results

```csharp
public static async Task<int[]> FetchAllAsync(string[] urls)
{
    var client = new HttpClient();
    Task<string>[] tasks = urls.Select(u => client.GetStringAsync(u)).ToArray();
    string[] results = await Task.WhenAll(tasks);
    return results.Select(r => r.Length).ToArray();
}
```

### 37. Retry with exponential backoff

```csharp
public static async Task<T> RetryAsync<T>(Func<Task<T>> action, int maxAttempts = 3)
{
    for (int attempt = 1; ; attempt++)
    {
        try { return await action(); }
        catch when (attempt < maxAttempts)
        {
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
        }
    }
}
```

### 38. Timeout an async operation

```csharp
public static async Task<T> WithTimeout<T>(Task<T> task, TimeSpan timeout)
{
    var delay = Task.Delay(timeout);
    var completed = await Task.WhenAny(task, delay);
    if (completed == delay) throw new TimeoutException();
    return await task;
}
```

---

## Tips for Coding Interviews

1. **Clarify** the problem and edge cases before coding (empty input, nulls, duplicates).
2. **Think out loud** — explain your approach and trade-offs.
3. State the **time and space complexity** (Big-O) of your solution.
4. Start with a **brute-force** solution, then optimize.
5. **Test** with examples, including edge cases.
6. Write **clean, readable** code — meaningful names, small functions.
7. Know your **data structures** (Dictionary/HashSet for O(1) lookups are common keys to optimization).

Good luck! Revisit the [C#](csharp-interview-questions.md) and
[ASP.NET MVC](aspnet-mvc-interview-questions.md) question banks to round out your prep.
