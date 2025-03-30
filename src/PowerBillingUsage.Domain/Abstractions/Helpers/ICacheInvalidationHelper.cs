namespace PowerBillingUsage.Domain.Abstractions.Helpers;

public interface ICacheInvalidationHelper
{
    List<Type> GetRelatedTypes(Type entityType);
}
