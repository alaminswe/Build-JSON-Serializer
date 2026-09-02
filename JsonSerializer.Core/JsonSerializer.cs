namespace JsonSerializer.Core;
using System.Globalization;
public static class JsonSerializer
{

    private static bool IsPrimitive(Type type)
    {
        return type == typeof(string)
               || type == typeof(int)
               || type == typeof(long)
               || type == typeof(float)
               || type == typeof(double)
               || type == typeof(decimal)
               || type == typeof(bool);
    }
    private static string EscapeString(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var news = text.Replace("\\", "\\\\");
        news = news.Replace("\"", "\\\"");
        news = news.Replace("\n", "\\n");
        news = news.Replace("\r", "\\r");
        news = news.Replace("\t", "\\t");

        return news;
    }
    public static string Serialize(object? value)
    {
        if (value == null)
        {
            return "null";
        }

        Type type = value.GetType();

        if (IsPrimitive(type))
        {
            return SerializePrimitive(type, value);
        }

        var properties = type.GetProperties();
        var list = new List<string>();
        foreach (var property in properties)
        {
            var name = property.Name;
            var val = property.GetValue(value);
            
            var v = Serialize(val);
            list.Add($"\"{name}\": {v}");
        }

        return $"{{{string.Join(", ", list)}}}";
    }

    public static T? Deserialize<T>(string json)
    {
        throw new NotImplementedException();
    }

    private static string SerializePrimitive(Type type, object value)
    {
        if (type == typeof(string))
        {
            var val = EscapeString(value.ToString());
            return $"\"{val}\"";
        }


        if (type == typeof(int) || type == typeof(long))
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        if (type == typeof(bool))
        {
            return value.ToString().ToLower();
        }

        return "0";
    }
}