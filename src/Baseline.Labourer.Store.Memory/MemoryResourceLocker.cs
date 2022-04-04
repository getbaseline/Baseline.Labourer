using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Labourer.Exceptions;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Contracts;
using Baseline.Labourer.Internal.Utils;

namespace Baseline.Labourer.Store.Memory;

/// <summary>
/// Provides a memory based <see cref="IResourceLocker"/> implementation. As with the other components of the
/// memory store, this is pretty much useless in a production scenario and should mainly be used for development
/// or for testing.
/// </summary>
public class MemoryResourceLocker : IResourceLocker
{
    private readonly MemoryStoreDataContainer _memoryStoreDataContainer;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MemoryResourceLocker(MemoryStoreDataContainer memoryStoreDataContainer)
        : this(memoryStoreDataContainer, new DateTimeProvider()) { }

    public MemoryResourceLocker(
        MemoryStoreDataContainer memoryStoreDataContainer,
        IDateTimeProvider dateTimeProvider
    )
    {
        _memoryStoreDataContainer = memoryStoreDataContainer;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc />
    public async Task<IAsyncDisposable> LockResourceAsync(string resource, TimeSpan @for)
    {
        var lockId = StringGenerationUtils.GenerateUniqueRandomString();

        using (await _memoryStoreDataContainer.AcquireStoreLockAsync())
        {
            if (
                _memoryStoreDataContainer.Locks.ContainsKey(resource)
                && _memoryStoreDataContainer.Locks[resource].Any(
                    @lock => @lock.Released == null && @lock.Until >= _dateTimeProvider.UtcNow()
                )
            )
            {
                throw new ResourceLockedException(resource);
            }

            if (!_memoryStoreDataContainer.Locks.ContainsKey(resource))
            {
                _memoryStoreDataContainer.Locks.Add(resource, new List<MemoryLock>());
            }

            _memoryStoreDataContainer.Locks[resource].Add(
                new MemoryLock { Id = lockId, Until = DateTime.UtcNow.Add(@for) }
            );
        }

        return new AsyncComposableDisposable(
            async () =>
            {
                using (await _memoryStoreDataContainer.AcquireStoreLockAsync())
                {
                    var lockToModify = _memoryStoreDataContainer.Locks[resource].First(
                        @lock => @lock.Id == lockId
                    );
                    lockToModify.Released = DateTime.UtcNow;
                }
            }
        );
    }
}
