using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace PowerBillingUsage.Core.Enums;

public abstract class SmartEnum<TEnum, TValue> where TEnum : SmartEnum<TEnum, TValue> where TValue : notnull
{
    private static readonly ConcurrentDictionary<TValue, TEnum> _items = new();

    public TValue Value { get; }
    public string Name { get; }

    protected SmartEnum(TValue value, string name)
    {
        Value = value;
        Name = name;
    }

    static SmartEnum()
    {
        // Automatically register all enum values
        foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetValue(null) is not TEnum item)
                continue;

            Register(item);
        }
    }

    public static TEnum FromValue(TValue value) => _items[value];

    public static bool TryFromValue(TValue value, [MaybeNullWhen(false)] out TEnum result)
    {
        return _items.TryGetValue(key: value, value: out result);
    }

    public static IEnumerable<TEnum> GetAll() => _items.Values;

    protected static TEnum Register(TEnum item) => _items.GetOrAdd(item.Value, item);

    public override string ToString() => Name;
}
