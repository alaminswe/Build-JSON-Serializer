using System.Collections.Concurrent;
using System.Reflection;

namespace JsonSerializer.Core.Reflection;

internal static class TypeMetadataCache
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertiesCache = new();
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> LookupCache = new();

    public static PropertyInfo[] GetProperties(Type type) =>
        PropertiesCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

    public static PropertyInfo? FindProperty(Type type, string name)
    {
        var lookup = LookupCache.GetOrAdd(type, BuildLookup);
        return lookup.TryGetValue(name, out var prop) ? prop : null;
    }

    private static Dictionary<string, PropertyInfo> BuildLookup(Type type)
    {
        var dict = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in GetProperties(type))
            dict[p.Name] = p;
        return dict;
    }
}