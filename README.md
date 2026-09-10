# MyJsonSerializer

A JSON serializer/deserializer built from scratch in C# using reflection — no `System.Text.Json`, no Newtonsoft.Json, no third-party JSON library.

## Project Structure

```
MyJsonSerializer.sln
├── JsonSerializer.Core     — the library (serialization, tokenizer, parser, deserialization)
├── JsonSerializer.Demo     — runnable console demo covering every feature
└── JsonSerializer.Tests    — xUnit test suite
```

## Usage

```csharp
using JsonSerializer.Core;

// Serialize
string json = JsonSerializer.Serialize(myObject);

// Deserialize
MyClass? obj = JsonSerializer.Deserialize<MyClass>(json);
```

## How It Works

**Serialization** walks an object's public properties via reflection (`PropertyInfo.GetValue`), recursing into nested objects, collections, and dictionaries, and building the JSON string directly.

**Deserialization** is a three-stage pipeline:
1. **Tokenizer** — scans the raw JSON text into a flat list of tokens (structural characters, quoted strings, and bare literals like numbers/`true`/`false`/`null`).
2. **Parser** — a recursive, type-directed descent parser that consumes tokens and, using the *target* C# type at each step (via reflection), builds the actual object graph directly — no intermediate untyped tree.
3. Property values are matched to JSON keys case-insensitively via `Type.GetProperty`.

## Supported Types

- Primitives: `string`, `int`, `long`, `float`, `double`, `decimal`, `bool`, `null`
- Any plain class, via reflection on public properties (no per-class code)
- Nested objects, at any depth
- Arrays, `List<T>`, general `IEnumerable`
- `Dictionary<string, object>` and other `IDictionary` implementations
- `DateTime`, `Guid`, `enum`, nullable value types (`int?`, etc.)

## Design Decisions

- **DateTime** → serialized as an ISO 8601 round-trip string (`"O"` format, e.g. `"2026-09-11T10:30:00.0000000Z"`), parsed back with `DateTimeStyles.RoundtripKind` to preserve `Kind`.
- **Guid** → plain string (`"3fa85f64-...".`)
- **Enum** → serialized as the member **name** (e.g. `"Active"`), not the underlying int — more readable in the JSON output. Deserialization is case-insensitive (`Enum.TryParse(..., ignoreCase: true)`).
- **Property matching** during deserialization is case-insensitive, so `"id"`, `"Id"`, and `"ID"` in JSON all bind to a C# `Id` property.
- **Unknown JSON fields** are silently skipped rather than throwing — the parser advances past them without needing a matching property. Extra/unexpected fields in real-world JSON are common; failing on them would be overly strict.
- **Numbers** are always formatted and parsed using `CultureInfo.InvariantCulture`, to avoid locale-dependent decimal separators (e.g. `,` vs `.`) producing invalid JSON or failed parses on non-US locales.
- **Quoted values that look like keywords** (e.g. the JSON string `"null"` or `"true"`) are correctly kept as strings, not misread as the `null`/`true` literals — the tokenizer preserves the surrounding quotes as a marker so the parser can tell a string apart from a bare keyword.

## Circular References

Detected during serialization using a reference-tracked "visited" set (`HashSet<object>` with a custom reference-equality comparer — not value equality, so two *different* objects that happen to be `.Equals()` are never confused with each other).

- An object is added to the set right before its properties/elements are serialized, and removed once serialization of that branch completes (backtracking), so **sibling** references to *different* instances are never falsely flagged.
- If an object is encountered while it is already in the visited set (i.e. it is its own ancestor in the current call chain), a `JsonSerializationException` is thrown instead of recursing forever.

```csharp
var alice = new Person { Name = "Alice" };
var bob = new Person { Name = "Bob" };
alice.Friend = bob;
bob.Friend = alice;             // circular!

JsonSerializer.Serialize(alice); // throws JsonSerializationException
```

## Error Handling

All custom errors derive from an abstract `JsonException`:

- **`JsonParseException`** — thrown by the tokenizer/parser for anything wrong with the *input JSON text*: unterminated strings, unexpected tokens, and type mismatches during deserialization (e.g. a JSON string where a number was expected). Messages include both the expected type and the offending raw value.
- **`JsonSerializationException`** — thrown only while *writing* JSON, currently for circular references.

Malformed input never silently produces a "valid-looking" object — every failure path raises one of the above with a specific message.

## Performance

Reflection metadata (`PropertyInfo[]` from `GetProperties()`, and a name → `PropertyInfo` lookup dictionary used during deserialization) is cached per `Type` in a `TypeMetadataCache`, backed by `ConcurrentDictionary`. Repeated `Type.GetProperties()` / `Type.GetProperty(name)` calls are the dominant cost in tight serialization loops, since each call does metadata work; caching turns repeat lookups into O(1) dictionary access.

**Benchmark** — serializing a simple 3-property object (`User { Id, Name, IsActive }`) in a loop:

```
Serialized 100000 times in 114 ms (0.00114 ms/op)
actual output (114ms/100k)

## Limitations

- `float.NaN` / `double.PositiveInfinity` / `NegativeInfinity` are not representable in JSON and are not specially handled — serializing them produces invalid JSON (`NaN`/`Infinity` tokens), matching how they'd print via `ToString()`.
- Only **public instance properties** are considered; fields are ignored (this matches the assignment's examples, which all use properties).
- A type must have a public parameterless constructor to be deserialized (`Activator.CreateInstance` is used); records or classes without one are not supported.
- Property setters that are missing (read-only properties) are skipped during deserialization — their JSON value is parsed (to keep the token stream in sync) but discarded.

## Running Tests

```bash
dotnet test
```

9 tests covering: primitive serialization, nested objects, circular reference detection, round-trip deserialization, quoted-keyword-vs-null handling, malformed JSON, type mismatches, collections, and enum round-tripping.

## Running the Demo

```bash
dotnet run --project JsonSerializer.Demo
```

Runs through all 17 demonstration sections (one per assignment requirement, plus edge cases and the benchmark) with labeled console output.