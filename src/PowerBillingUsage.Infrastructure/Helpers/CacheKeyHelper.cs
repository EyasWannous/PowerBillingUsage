using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Attributes;
using System.Reflection;

namespace PowerBillingUsage.Infrastructure.Helpers;
public class CacheKeyHelper<Entity> : ICacheKeyHelper<Entity>, IScopedDependency
{
    public string EntityName { get; }

    public CacheKeyHelper()
    {
        var attribute = typeof(Entity).GetCustomAttribute<CacheEntityAttribute>();
        if (attribute is null)
            throw new ArgumentNullException(nameof(attribute));

        EntityName = attribute.EntityName;
    }
}
