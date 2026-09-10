using JsonSerializer.Core.Exception;
using System.Runtime.CompilerServices;
using JsonSerializer.Core.Reflection;

namespace JsonSerializer.Core;
using System.Globalization;
using System.Collections;
public static class JsonSerializer
{
    private static string SerializePrimitive(Type type, object value)
    {
        if (type == typeof(string))
        {
            var val = EscapeString((string)value);
            return $"\"{val}\"";
        }

        if (type.IsEnum)
        {
            return $"\"{value}\"";
        }

        if (type == typeof(DateTime))
        {
            return $"\"{((DateTime)value):O}\"";
        }

        if (type == typeof(Guid))
        {
            return $"\"{value}\"";
        }

        if (type == typeof(int))
        {
            return ((int)value).ToString(CultureInfo.InvariantCulture);
        }

        if (type == typeof(long))
        {
            return ((long)value).ToString(CultureInfo.InvariantCulture);
        }

        if (type == typeof(float))
        {
            return ((float)value).ToString(CultureInfo.InvariantCulture);
        }

        if (type == typeof(double))
        {
            return ((double)value).ToString(CultureInfo.InvariantCulture);
        }

        if (type == typeof(decimal))
        {
            return ((decimal)value).ToString(CultureInfo.InvariantCulture);
        }

        if (type == typeof(bool))
        {
            return (bool)value ? "true" : "false";
        }

        throw new JsonSerializationException($"No serialization rule defined for primitive type '{type.Name}'.");

    }
    private static bool IsPrimitive(Type type)
    {
        return type == typeof(string)
               || type == typeof(int)
               || type == typeof(long)
               || type == typeof(float)
               || type == typeof(double)
               || type == typeof(decimal)
               || type == typeof(bool)
               || type.IsEnum
               || type == typeof(DateTime)
               || type == typeof(Guid);
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
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

        return Serialize(value, visited);
    }
    private static string Serialize(object? value, HashSet<object> visited)
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
        
        if (value is IDictionary dictionary)
        {
            if (!visited.Add(value))
            {
                throw new JsonSerializationException("Circular reference detected.");
            }

            var list = new List<string>();

            foreach (DictionaryEntry o in dictionary)
            {
                var key = EscapeString(o.Key?.ToString() ?? "");
                var val = Serialize(o.Value, visited);

                list.Add($"\"{key}\": {val}");
            }

            visited.Remove(value);

            return $"{{{string.Join(", ", list)}}}";
        }

        if (value is IEnumerable enumerable)
        {
            if (!visited.Add(value))
            {
                throw new JsonSerializationException("Circular reference detected.");
            }

            var list = new List<string>();

            foreach (var o in enumerable)
                list.Add(Serialize(o, visited));

            visited.Remove(value);

            return $"[{string.Join(", ", list)}]";
        }

        // Object
        if (!visited.Add(value))
        {
            throw new JsonSerializationException("Circular reference detected.");
        }

        var properties = TypeMetadataCache.GetProperties(type);
        var propertyList = new List<string>();

        foreach (var property in properties)
        {
            var name = property.Name;
            var val = property.GetValue(value);

            var v = Serialize(val, visited);

            propertyList.Add($"\"{name}\": {v}");
        }

        visited.Remove(value);

        return $"{{{string.Join(", ", propertyList)}}}";
    }

    private static string UnEscapeString(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        if (text.Length >= 2 && text[0] == '"' && text[^1] == '"')
        {
            text = text[1..^1];
        }

        var news = text.Replace("\\n", "\n");
        news = news.Replace("\\r", "\r");
        news = news.Replace("\\t", "\t");
        news = news.Replace("\\\"", "\"");
        news = news.Replace("\\\\", "\\");

        return news;
    }
    // "{\"Name\": \"Tamim\", \"HomeAddress\": {\"City\": \"Dhaka\", \"ZipCode\": 1207}}";
    private static List<string> Tokenizer(string json)
    {
        var tokens = new List<string>();

        int i = 0;

        while (i < json.Length)
        {
            char c = json[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            // JSON symbols
            if (c == '{' || c == '}' || c == ':' || c == ',' || c == '[' || c == ']')
            {
                tokens.Add(c.ToString());
                i++;
            }// String
            else if (c == '"')
            {
                int start = ++i;

                while (i < json.Length && json[i] != '"')
                {
                    if (json[i] == '\\')
                        i++;

                    i++;
                }

                if (i >= json.Length)
                {
                    throw new JsonParseException("Unterminated JSON string.");
                }

                string extracted = json.Substring(start, i - start);

                tokens.Add($"\"{extracted}\"");

                i++;
            } // Number / true / false / null
            else
            {
                int start = i;

                while (i < json.Length && !"{}:,[] \t\r\n".Contains(json[i]))
                {
                    i++;
                }

                tokens.Add( json.Substring(start, i - start));
            }
        }
        return tokens;
    }

    public static object? Parser( List<string> tokens, ref int index, Type targetType)
    {
        if (index >= tokens.Count)
            return null;

        var realType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        var currentVal = tokens[index];

        if (currentVal.Length >= 2 && currentVal[0] == '"' && currentVal[^1] == '"')
        {
            index++;
            var stringValue = UnEscapeString(currentVal);

            if (realType.IsEnum)
            {
                if (!Enum.TryParse(realType, stringValue, ignoreCase: true, out var enumResult))
                {
                    throw new JsonParseException($"'{stringValue}' is not a valid value for enum '{realType.Name}'.");
                }
                return enumResult;
            }

            if (realType == typeof(DateTime))
            {
                if (!DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind,
                        out var dateResult))
                {
                    throw new JsonParseException($"'{stringValue}' is not a valid DateTime.");
                }
                return dateResult;
            }

            if (realType == typeof(Guid))
            {
                if (!Guid.TryParse(stringValue, out var guidResult))
                {
                    throw new JsonParseException($"'{stringValue}' is not a valid Guid.");
                }
                return guidResult;
            }

            if (realType != typeof(string))
            {
                throw new JsonParseException( $"Cannot convert JSON string \"{stringValue}\" to type '{realType.Name}'.");
            }

            return stringValue;
        }

        if (currentVal == "null")
        {
            index++;
            return null;
        }

        // object

        if (currentVal == "{")
        {
            var instance = Activator.CreateInstance(targetType);

            index++; // skip {

            while (index < tokens.Count && tokens[index] != "}")
            {
                // Property name
                string key = UnEscapeString(tokens[index++]);

                // Skip :
                if (index < tokens.Count && tokens[index] == ":")
                {
                    index++;
                }

                // Find property
                var property = TypeMetadataCache.FindProperty(targetType, key);

                if (property != null && property.CanWrite)
                {
                    // Recursive parsing
                    var parsedValue = Parser( tokens, ref index, property.PropertyType);

                    property.SetValue( instance, parsedValue);
                }
                else
                {
                    Parser( tokens, ref index, typeof(object));
                }

                // Skip comma
                if (index < tokens.Count && tokens[index] == ",")
                {
                    index++;
                }
            }

            // Skip }
            if (index < tokens.Count &&
                tokens[index] == "}")
            {
                index++;
            }

            return instance;
        }

        // arr // coll

        if (currentVal == "[")
        {
            index++; // skip [

            var elementType = targetType.IsArray  ? targetType.GetElementType()! : targetType.GetGenericArguments()[0];

            var items = new List<object?>();

            while (index < tokens.Count && tokens[index] != "]")
            {
                var item = Parser( tokens, ref index, elementType );

                items.Add(item);

                if (index < tokens.Count && tokens[index] == ",")
                {
                    index++;
                }
            }

            // skip ]
            if (index < tokens.Count && tokens[index] == "]")
            {
                index++;
            }

            // Array
            if (targetType.IsArray)
            {
                var array = Array.CreateInstance(elementType, items.Count);

                for (int i = 0; i < items.Count; i++)
                {
                    array.SetValue(items[i], i);
                }

                return array;
            }

            // List<T>
            var listType = typeof(List<>).MakeGenericType(elementType);

            var list = (IList)Activator.CreateInstance(listType)!;

            foreach (var item in items)
            {
                list.Add(item);
            }

            return list;
        }


        // prim
        index++;

        if (realType == typeof(string))
            return currentVal;

        if (realType == typeof(bool))
        {
            if (currentVal != "true" && currentVal != "false")
                throw new JsonParseException($"Expected 'true' or 'false' but got '{currentVal}'.");
            return currentVal == "true";
        }

        if (realType == typeof(int))
        {
            if (!int.TryParse(currentVal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
                throw new JsonParseException($"Expected an int but got '{currentVal}'.");
            return r;
        }

        if (realType == typeof(long))
        {
            if (!long.TryParse(currentVal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
                throw new JsonParseException($"Expected a long but got '{currentVal}'.");
            return r;
        }

        if (realType == typeof(float))
        {
            if (!float.TryParse(currentVal, NumberStyles.Float, CultureInfo.InvariantCulture, out var r))
                throw new JsonParseException($"Expected a float but got '{currentVal}'.");
            return r;
        }

        if (realType == typeof(double))
        {
            if (!double.TryParse(currentVal, NumberStyles.Float, CultureInfo.InvariantCulture, out var r))
                throw new JsonParseException($"Expected a double but got '{currentVal}'.");
            return r;
        }

        if (realType == typeof(decimal))
        {
            if (!decimal.TryParse(currentVal, NumberStyles.Float, CultureInfo.InvariantCulture, out var r))
                throw new JsonParseException($"Expected a decimal but got '{currentVal}'.");
            return r;
        }

        try
        {
            return Convert.ChangeType(currentVal, realType, CultureInfo.InvariantCulture);
        }
        catch (System.Exception)
        {
            throw new JsonParseException($"Cannot convert '{currentVal}' to type '{realType.Name}'.");
        }
    }
    // {"Id": 1, "Name": "John", "IsActive": true}
    public static T? Deserialize<T>(string json)
    {
        var tokens = Tokenizer(json);
        int index = 0;
        var result = Parser(tokens, ref index, typeof(T));
        return (T?)result;
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();

        public new bool Equals(object? x, object? y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}


