// MyJsonSerializer - Demo
using System.Diagnostics;

void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine($"== {title} ==");
}

// 1. Primitive Serialization

Section("1. Primitive Serialization");

Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(null));
Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(42));
Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(10.5));
Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(true));
Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(false));
Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize("Hello"));
Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize("He said \"Hi\""));

// 2. String Serialization + Escaping

Section("2. String Escaping");

var escaped = JsonSerializer.Core.JsonSerializer.Serialize("Hello \"John\"\nWelcome");
Console.WriteLine($"json = {escaped}");

// 3. Primitive Deserialization

Section("3. Primitive Deserialization");

var number = JsonSerializer.Core.JsonSerializer.Deserialize<int>("123");
var decimalNumber = JsonSerializer.Core.JsonSerializer.Deserialize<double>("12.012");
var boolean = JsonSerializer.Core.JsonSerializer.Deserialize<bool>("true");

Console.WriteLine(number);
Console.WriteLine(decimalNumber);
Console.WriteLine(boolean);

// 4. String Deserialization + Unescaping

Section("4. String Unescaping");

var rawJson = "\"He said \\\"hi\\\"\"";
var original = JsonSerializer.Core.JsonSerializer.Deserialize<string>(rawJson);
Console.WriteLine($"original = {original}");

// 5. Object Serialization

Section("5. Object Serialization");

var basicUser = new User
{
    Id = 1,
    Name = "John",
    IsActive = true
};

Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(basicUser));

// 6. Nested Object Serialization
Section("6. Nested Object Serialization");

var nestedUser = new User
{
    Id = 1,
    Name = "John\"Doe",
    IsActive = true,
    HomeAddress = new Address
    {
        City = "Dhaka",
        Country = "Bangladesh"
    }
};

Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(nestedUser));

// 7. Collection Serialization

Section("7. Collection Serialization");

var numbers = new List<int> { 1, 2, 3 };

var names = new[] { "Alice", "Bob" };

var addresses = new List<Address>
{
    new() { City = "Dhaka", Country = "Bangladesh" },
    new() { City = "Dinajpur", Country = "Bangladesh" }
};

Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(numbers));
Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(names));
Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(addresses));

// 8. Dictionary Serialization

Section("8. Dictionary Serialization");

var dict = new Dictionary<string, object>
{
    { "count", 3 },
    { "items", numbers }
};

Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(dict));

// 9. Object Deserialization

Section("9. Object Deserialization");

var basicJson = """{"Id":1,"Name":"John","IsActive":true}""";
var deserializedUser = JsonSerializer.Core.JsonSerializer.Deserialize<User>(basicJson);

Console.WriteLine($"Id       = {deserializedUser?.Id}");
Console.WriteLine($"Name     = {deserializedUser?.Name}");
Console.WriteLine($"IsActive = {deserializedUser?.IsActive}");

// 10. Nested Object Deserialization=

Section("10. Nested Object Deserialization");

var nestedJson = """
                 {
                     "Id": 1,
                     "Name": "Tamim",
                     "IsActive": true,
                     "HomeAddress":
                     {
                         "City": "Dhaka",
                         "Country": "Bangladesh"
                     }
                 }
                 """;

var nestedResult = JsonSerializer.Core.JsonSerializer.Deserialize<User>(nestedJson);

Console.WriteLine($"Id      = {nestedResult?.Id}");
Console.WriteLine($"Name    = {nestedResult?.Name}");
Console.WriteLine($"City    = {nestedResult?.HomeAddress?.City}");
Console.WriteLine($"Country = {nestedResult?.HomeAddress?.Country}");

// 11. Collection Deserialization

Section("11. Collection Deserialization");

var scoresJson = """{"Name":"Tamim","Scores":[80,90,95]}""";
var scoresUser = JsonSerializer.Core.JsonSerializer.Deserialize<User>(scoresJson);

Console.WriteLine($"Name = {scoresUser?.Name}");
foreach (var score in scoresUser?.Scores ?? new List<int>())
{
    Console.WriteLine($"Score = {score}");
}

// 12. Special Types: DateTime, Guid, Enum

Section("12. Special Types (DateTime, Guid, Enum)");

var evt = new Event
{
    Title = "Launch",
    Occurred = new DateTime(2026, 9, 11, 10, 30, 0, DateTimeKind.Utc),
    Id = Guid.NewGuid(),
    State = Status.Active
};

var eventJson = JsonSerializer.Core.JsonSerializer.Serialize(evt);
Console.WriteLine(eventJson);

var eventBack = JsonSerializer.Core.JsonSerializer.Deserialize<Event>(eventJson);
Console.WriteLine($"{eventBack?.Title}, {eventBack?.Occurred:O}, {eventBack?.Id}, {eventBack?.State}");

// 13. String "null"/"true" vs actual JSON null

Section("13. Quoted keyword-like strings vs actual null");

var note1 = JsonSerializer.Core.JsonSerializer.Deserialize<Note>("""{"Text":"null"}""");
Console.WriteLine($"Text = {note1?.Text}");

var note2 = JsonSerializer.Core.JsonSerializer.Deserialize<Note>("""{"Text":"true"}""");
Console.WriteLine($"Text = {note2?.Text}");

var note3 = JsonSerializer.Core.JsonSerializer.Deserialize<Note>("""{"Text":null}""");
Console.WriteLine($"Text = {note3?.Text ?? "actual null"}");

// 14. Malformed JSON (unterminated string)

Section("14. Malformed JSON Test");

try
{
    JsonSerializer.Core.JsonSerializer.Deserialize<Note>("""{"Text":"unterminated""");
    Console.WriteLine("BUG: no exception thrown!");
}
catch (Exception ex)
{
    Console.WriteLine($"Correctly threw: {ex.GetType().Name}: {ex.Message}");
}

// 15. Type Mismatch

Section("15. Type Mismatch Test");

try
{
    JsonSerializer.Core.JsonSerializer.Deserialize<User>("""{"Id":"not-a-number"}""");
    Console.WriteLine("BUG: no exception thrown!");
}
catch (Exception ex)
{
    Console.WriteLine($"Correctly threw: {ex.GetType().Name}: {ex.Message}");
}

// 16. Circular Reference

Section("16. Circular Reference Test");

var alice = new Person { Name = "Alice" };
var bob = new Person { Name = "Bob" };
alice.Friend = bob;
bob.Friend = alice; // circular!

try
{
    JsonSerializer.Core.JsonSerializer.Serialize(alice);
    Console.WriteLine("BUG: no exception thrown!");
}
catch (Exception ex)
{
    Console.WriteLine($"Correctly threw: {ex.GetType().Name}: {ex.Message}");
}

// 17. Performance Benchmark

Section("17. Performance Benchmark");

RunBenchmark();

void RunBenchmark()
{
    var user = new User { Id = 1, Name = "Benchmark", IsActive = true };
    const int iterations = 100_000;

    JsonSerializer.Core.JsonSerializer.Serialize(user);

    var sw = Stopwatch.StartNew();
    for (int i = 0; i < iterations; i++)
    {
        JsonSerializer.Core.JsonSerializer.Serialize(user);
    }
    sw.Stop();

    Console.WriteLine($"Serialized {iterations} times in {sw.ElapsedMilliseconds} ms " +
                      $"({sw.Elapsed.TotalMilliseconds / iterations:F5} ms/op)");
}

// Models

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
    public Address? HomeAddress { get; set; }
    public List<int> Scores { get; set; } = new();
}

public class Address
{
    public string City { get; set; } = "";
    public string Country { get; set; } = "";
}

public class Note
{
    public string? Text { get; set; }
}

public class Person
{
    public string Name { get; set; } = "";
    public Person? Friend { get; set; }
}

public enum Status
{
    Active,
    Inactive
}

public class Event
{
    public string Title { get; set; } = "";
    public DateTime Occurred { get; set; }
    public Guid Id { get; set; }
    public Status State { get; set; }
}