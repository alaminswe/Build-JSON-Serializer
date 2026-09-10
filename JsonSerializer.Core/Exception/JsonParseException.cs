namespace JsonSerializer.Core.Exception;

public abstract class JsonException : System.Exception
{
    protected JsonException(string message) : base(message){}
}

public class JsonParseException : JsonException
{
    public JsonParseException(string message) : base(message){}
}

public class JsonSerializationException : JsonException
{
    public JsonSerializationException(string message) : base(message){}
}