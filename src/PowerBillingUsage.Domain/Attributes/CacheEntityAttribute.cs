using System.Reflection;

namespace PowerBillingUsage.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class CacheEntityAttribute : Attribute
{
    public string EntityName { get; }

    public CacheEntityAttribute(string entityName)
    {
        EntityName = entityName;
    }

    public CacheEntityAttribute(Type entityType)
    {
        EntityName = entityType.FullName ?? throw new ArgumentNullException(nameof(entityType));
    }

    public CacheEntityAttribute(Type entityType, string fallbackName)
    {
        var attribute = entityType.GetCustomAttribute<CacheEntityAttribute>();

        if (attribute?.EntityName is null)
            EntityName = fallbackName;
        else
            EntityName = attribute!.EntityName;
    }
}
