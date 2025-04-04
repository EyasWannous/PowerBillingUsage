using Microsoft.EntityFrameworkCore.ChangeTracking;
using PowerBillingUsage.Domain.Abstractions;

namespace PowerBillingUsage.Infrastructure.EntityFramework.EntityConfigurations;

public static class ValueComparers
{
    // Define a value comparer for IEntityId to compare only the Guid value
    public static ValueComparer<T> GetValueComparer<T>() where T : IEntityId =>
        new(
            (id1, id2) => id1 != null && id2 != null && id1.Value == id2.Value,
            id => id.Value.GetHashCode()
        );
}
