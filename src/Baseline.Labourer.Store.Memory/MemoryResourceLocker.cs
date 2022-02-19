using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;

namespace Baseline.Labourer.Store.Memory
{
    /// <summary>
    /// Provides a memory based <see cref="IResourceLocker"/> implementation. As with the other components of the
    /// memory store, this is pretty much useless in a production scenario and should mainly be used for development
    /// or for testing.
    /// </summary>
    public class MemoryResourceLocker : IResourceLocker
    {
        private readonly MemoryBackingStore _memoryBackingStore;
        private readonly IDateTimeProvider _dateTimeProvider;

        public MemoryResourceLocker(MemoryBackingStore memoryBackingStore) : this(memoryBackingStore, new DateTimeProvider())
        {
        }

        protected MemoryResourceLocker(MemoryBackingStore memoryBackingStore, IDateTimeProvider dateTimeProvider)
        {
            _memoryBackingStore = memoryBackingStore;
            _dateTimeProvider = dateTimeProvider;
        }

        /// <inheritdoc />
        public async Task<IAsyncDisposable> LockResourceAsync(
            string resource, 
            TimeSpan @for, 
            CancellationToken cancellationToken
        )
        {
            var lockId = StringGenerationUtils.GenerateUniqueRandomString();

            using (await _memoryBackingStore.AcquireStoreLockAsync())
            {
                if (
                    _memoryBackingStore.Locks.ContainsKey(resource) && 
                    _memoryBackingStore.Locks[resource].Any(
                        @lock => @lock.Released == null && @lock.Until >= _dateTimeProvider.UtcNow()
                    )
                )
                {
                    throw new ResourceLockedException(resource);
                }
                
                if (!_memoryBackingStore.Locks.ContainsKey(resource))
                {
                    _memoryBackingStore.Locks.Add(resource, new List<MemoryLock>());
                }

                _memoryBackingStore.Locks[resource].Add(new MemoryLock
                {
                    Id = lockId,
                    Until = DateTime.UtcNow.Add(@for)
                });
            }

            return new AsyncComposableDisposable(async () =>
            {
                using (await _memoryBackingStore.AcquireStoreLockAsync())
                {
                    var lockToModify = _memoryBackingStore.Locks[resource].First(@lock => @lock.Id == lockId);
                    lockToModify.Released = DateTime.UtcNow;
                }
            });
        }
    }
}