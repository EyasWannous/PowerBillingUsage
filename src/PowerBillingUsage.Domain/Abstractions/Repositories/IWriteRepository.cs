﻿using System.Linq.Expressions;

namespace PowerBillingUsage.Domain.Abstractions.Repositories;

public interface IWriteRepository<Entity, EntityId> : IDisposable
    where Entity : class, IEntity<EntityId>
    where EntityId : IEntityId
{
    Task DeleteAsync(EntityId id, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<Entity> InsertAsync(Entity bill, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Entity> UpdateAsync(Entity bill, CancellationToken cancellationToken = default);
}
