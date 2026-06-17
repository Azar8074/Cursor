# Module 15 — Files, Streams & Serialization

## The System.IO Namespace

Provides types for reading/writing files and directories.

## Reading & Writing Text Files

### Simple one-shot helpers (`File` static class)

```csharp
using System.IO;

// Write
File.WriteAllText("notes.txt", "Hello\nWorld");
File.WriteAllLines("list.txt", new[] { "line1", "line2" });
File.AppendAllText("notes.txt", "\nAppended");

// Read
string text = File.ReadAllText("notes.txt");
string[] lines = File.ReadAllLines("list.txt");

// Existence & metadata
bool exists = File.Exists("notes.txt");
File.Copy("notes.txt", "backup.txt", overwrite: true);
File.Move("backup.txt", "archive.txt");
File.Delete("archive.txt");
```

### Async versions (preferred for I/O)

```csharp
await File.WriteAllTextAsync("data.txt", "content");
string content = await File.ReadAllTextAsync("data.txt");
```

## Streams

A **stream** is an abstraction over a sequence of bytes (file, network, memory). Use stream
readers/writers for large data so you don't load everything into memory.

```csharp
// Writing with StreamWriter
using (var writer = new StreamWriter("log.txt", append: true))
{
    writer.WriteLine("Log entry at " + DateTime.Now);
}

// Reading line-by-line with StreamReader (memory friendly)
using (var reader = new StreamReader("log.txt"))
{
    string? line;
    while ((line = reader.ReadLine()) != null)
        Console.WriteLine(line);
}

// Raw bytes with FileStream
using (var fs = new FileStream("data.bin", FileMode.Create))
{
    byte[] bytes = { 1, 2, 3, 4 };
    fs.Write(bytes, 0, bytes.Length);
}

// MemoryStream — an in-memory byte buffer
using var ms = new MemoryStream();
```

Common stream types: `FileStream`, `MemoryStream`, `NetworkStream`, `GZipStream`, `BufferedStream`.

## Working with Directories & Paths

```csharp
Directory.CreateDirectory("output");
bool dirExists = Directory.Exists("output");
string[] files = Directory.GetFiles("output", "*.txt");
string[] dirs  = Directory.GetDirectories(".");
Directory.Delete("output", recursive: true);

// Path helpers — always build paths with Path.Combine (cross-platform)
string full = Path.Combine("output", "sub", "file.txt");
string ext  = Path.GetExtension(full);       // .txt
string name = Path.GetFileNameWithoutExtension(full); // file
string dir  = Path.GetDirectoryName(full);
string temp = Path.GetTempFileName();
```

## JSON Serialization (System.Text.Json)

The modern, built-in, high-performance JSON library.

```csharp
using System.Text.Json;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

// Serialize (object -> JSON)
var product = new Product { Id = 1, Name = "Keyboard", Price = 49.99m };
string json = JsonSerializer.Serialize(product);

// Pretty-print
var options = new JsonSerializerOptions { WriteIndented = true };
string pretty = JsonSerializer.Serialize(product, options);

// Deserialize (JSON -> object)
Product? back = JsonSerializer.Deserialize<Product>(json);

// Collections
var list = new List<Product> { product };
string listJson = JsonSerializer.Serialize(list);
var products = JsonSerializer.Deserialize<List<Product>>(listJson);
```

### Customizing JSON

```csharp
using System.Text.Json.Serialization;

public class User
{
    [JsonPropertyName("user_name")]
    public string Name { get; set; } = "";

    [JsonIgnore]
    public string Password { get; set; } = "";
}

var opts = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
```

### Async JSON to/from files & streams

```csharp
await using FileStream fs = File.Create("product.json");
await JsonSerializer.SerializeAsync(fs, product);

await using FileStream rs = File.OpenRead("product.json");
Product? loaded = await JsonSerializer.DeserializeAsync<Product>(rs);
```

## Other Serialization Formats

- **XML**: `System.Xml.Serialization.XmlSerializer`.
- **Binary**: `BinaryFormatter` is **obsolete and insecure** — do not use it.
- **Newtonsoft.Json (Json.NET)**: popular third-party library; still common in older projects.

## Key Takeaways

- Use `File.ReadAllText`/`WriteAllText` for small files; **streams** for large data.
- Always build paths with `Path.Combine` for cross-platform safety.
- Prefer the async I/O APIs to avoid blocking threads.
- Use `System.Text.Json` for fast, built-in JSON; avoid the obsolete `BinaryFormatter`.

➡️ Next: [Module 16 — Advanced C#](16-advanced-topics.md)
