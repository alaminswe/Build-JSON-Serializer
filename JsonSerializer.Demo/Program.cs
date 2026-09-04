using System.Text.Json.Serialization;

var numbers = new List<int> { 1, 2, 3 };
var names = new[] { "Alice", "Bob" };
var dict = new Dictionary<string, object>
{
    { "count", 3 },
    { "items", numbers }
};

Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(numbers));
Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(names));
Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(dict));

//Collections start
// var user = new User
// {
//     Id = 1,
//     Name = "John",
//     Skills = new List<string>
//     {
//         "C#",
//         "ASP.NET",
//         "SQL"
//     }
// };
// var addresses = new List<Address>
// {
//     new Address
//     {
//         City = "Dhaka",
//         Country = "Bangladesh"
//     },
//     new Address
//     {
//         City = "Dinajpur",
//         Country = "Bangladesh"
//     }
// };
// Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(user));
// Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(addresses));

// public class Address
// {
//     public string City { get; set; } = "";
//     public string Country { get; set; } = "";
// }
// public class User
// {
//     public int Id { get; set; }
//     public string Name { get; set; } = "";
//     public List<string> Skills { get; set; } = new();
// }
//Collections end

//covering Nested Serialization - Start

// var user = new User
// {
//     Id = 1,
//     Name = "John\"Doe",
//     IsActive = true,
//     HomeAddress = new Address { City = "Dhaka", Country = "Bangladesh" }
// };

// Console.WriteLine(JsonSerializer.Core.JsonSerializer.Serialize(user));

//covering Nested Serialization - End
//only primitive type -- start
// var json1 = JsonSerializer.Core.JsonSerializer.Serialize(null);
// var json2 = JsonSerializer.Core.JsonSerializer.Serialize(42);
// var json3 = JsonSerializer.Core.JsonSerializer.Serialize(10.5);
// var json4 = JsonSerializer.Core.JsonSerializer.Serialize(true);
// var json5 = JsonSerializer.Core.JsonSerializer.Serialize(false);
// var json6 = JsonSerializer.Core.JsonSerializer.Serialize("Hello");
// var json7 = JsonSerializer.Core.JsonSerializer.Serialize("He said \"Hi\"");
// Console.WriteLine(json1);
// Console.WriteLine(json2);
// Console.WriteLine(json3);
// Console.WriteLine(json4);
// Console.WriteLine(json5);
// Console.WriteLine(json6);
// Console.WriteLine(json7);

//only primitive type -- end


// public class Address
// {
//     public string City { get; set; } = "";
//     public string Country { get; set; } = "";
// }

// public class User
// {
//     public int Id { get; set; }
//     public string Name { get; set; } = "";
//     public bool IsActive { get; set; }
//     public Address? HomeAddress { get; set; }
// }
