using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Attributes;
using System.Reflection;

namespace PowerBillingUsage.Infrastructure.Helpers;

public class CacheInvalidationHelper : ICacheInvalidationHelper, ISingletonDependency
{
    private readonly Dictionary<Type, List<Type>> _relatedTypes;

    public CacheInvalidationHelper(IEnumerable<Assembly> assembliesToScan)
    {
        _relatedTypes = [];

        // Scan all provided assemblies for ReadModel types
        var readModelTypes = assembliesToScan
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IReadModel<>)) && t.IsClass)
            .Distinct();

        foreach (var type in readModelTypes)
        {
            var attributes = type.GetCustomAttributes<RelatedToEntityAttribute>(false);
            foreach (var attribute in attributes)
            {
                if (!_relatedTypes.ContainsKey(attribute.EntityType))
                    _relatedTypes[attribute.EntityType] = [];

                _relatedTypes[attribute.EntityType].Add(type);
            }
        }
    }

    public List<Type> GetRelatedTypes(Type entityType)
    {
        return _relatedTypes.TryGetValue(entityType, out List<Type>? value) ? value : [];
    }
}