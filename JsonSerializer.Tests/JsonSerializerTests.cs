using JsonSerializer.Core.Exception;
namespace JsonSerializer.Tests;

public class Address
{
    public string City { get; set; } = "";
}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
    public Address? Home { get; set; }
    public Person? Friend { get; set; }
}

public enum Status
{
    Active,
    Inactive
}

public class JsonSerializerTests
{
    [Fact]
    public void Serialize_Primitives_ProducesCorrectJson()
    {
        Assert.Equal("42", JsonSerializer.Core.JsonSerializer.Serialize(42));
        Assert.Equal("true", JsonSerializer.Core.JsonSerializer.Serialize(true));
        Assert.Equal("null", JsonSerializer.Core.JsonSerializer.Serialize(null));
        Assert.Equal("\"hi\"", JsonSerializer.Core.JsonSerializer.Serialize("hi"));
    }

    [Fact]
    public void Serialize_NestedObject_NestsCorrectly()
    {
        var p = new Person { Id = 1, Name = "A", Home = new Address { City = "Dhaka" } };
        var json = JsonSerializer.Core.JsonSerializer.Serialize(p);
        Assert.Contains("\"City\": \"Dhaka\"", json);
    }

    [Fact]
    public void Serialize_CircularReference_ThrowsSerializationException()
    {
        var a = new Person { Name = "A" };
        var b = new Person { Name = "B" };
        a.Friend = b;
        b.Friend = a;

        Assert.Throws<JsonSerializationException>(() => JsonSerializer.Core.JsonSerializer.Serialize(a));
    }

    [Fact]
    public void Deserialize_RoundTrip_PreservesValues()
    {
        var p = new Person { Id = 5, Name = "Round", IsActive = true, Home = new Address { City = "Dhaka" } };
        var json = JsonSerializer.Core.JsonSerializer.Serialize(p);
        var back = JsonSerializer.Core.JsonSerializer.Deserialize<Person>(json);

        Assert.Equal(p.Id, back!.Id);
        Assert.Equal(p.Name, back.Name);
        Assert.Equal(p.Home!.City, back.Home!.City);
    }

    [Fact]
    public void Deserialize_QuotedKeywordLikeString_StaysAsString()
    {
        var json = "{\"Name\": \"null\"}";
        var back = JsonSerializer.Core.JsonSerializer.Deserialize<Person>(json);
        Assert.Equal("null", back!.Name);
    }

    [Fact]
    public void Deserialize_MalformedJson_ThrowsParseException()
    {
        var badJson = "{\"Name\": \"unterminated";
        Assert.Throws<JsonParseException>(() =>
            JsonSerializer.Core.JsonSerializer.Deserialize<Person>(badJson));
    }

    [Fact]
    public void Deserialize_TypeMismatch_ThrowsClearError()
    {
        var json = "{\"Id\": \"not-a-number\"}";
        var ex = Assert.Throws<JsonParseException>(() =>
            JsonSerializer.Core.JsonSerializer.Deserialize<Person>(json));

        Assert.Contains("Int32", ex.Message); // target type name
        Assert.Contains("not-a-number", ex.Message); // the actual bad value
    }

    [Fact]
    public void Deserialize_Collections_WorksCorrectly()
    {
        var json = JsonSerializer.Core.JsonSerializer.Serialize(new List<int> { 1, 2, 3 });
        var back = JsonSerializer.Core.JsonSerializer.Deserialize<List<int>>(json);
        Assert.Equal(new List<int> { 1, 2, 3 }, back);
    }

    [Fact]
    public void Deserialize_Enum_RoundTrips()
    {
        var json = JsonSerializer.Core.JsonSerializer.Serialize(Status.Active);
        var back = JsonSerializer.Core.JsonSerializer.Deserialize<Status>(json);
        Assert.Equal(Status.Active, back);
    }
}