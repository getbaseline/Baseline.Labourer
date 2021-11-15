using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Exceptions;
using Baseline.Labourer.Internal.Utils;
using Baseline.Labourer.Store.Memory.Internal;

namespace Baseline.Labourer.Store.Memory
{
    /// <summary>
    /// Provides a memory based <see cref="IResourceLocker"/> implementation. As with the other components of the
    /// memory store, this is pretty much useless in a production scenario and should mainly be used for development
    /// or for testing.
    /// </summary>
    public class MemoryResourceLocker : IResourceLocker
    {
        private readonly MemoryStore _memoryStore;

        public MemoryResourceLocker(MemoryStore memoryStore)
        {
            _memoryStore = memoryStore;
        }

        /// <inheritdoc />
        public async Task<IAsyncDisposable> LockResourceAsync(
            string resource, 
            TimeSpan @for, 
            CancellationToken cancellationToken
        )
        {
            var lockId = StringGenerationUtils.GenerateUniqueRandomString();

            using (await _memoryStore.AcquireLockAsync())
            {
                if (_memoryStore.Locks.ContainsKey(resource) && _memoryStore.Locks[resource].Any(@lock => @lock.Locked))
                {
                    throw new ResourceLockedException();
                }
                
                if (!_memoryStore.Locks.ContainsKey(resource))
                {
                    _memoryStore.Locks.Add(resource, new List<MemoryLock>());
                }

                _memoryStore.Locks[resource].Add(new MemoryLock
                {
                    Id = lockId,
                    Until = DateTime.UtcNow.Add(@for)
                });
            }

            return new AsyncComposableDisposable(async () =>
            {
                using (await _memoryStore.AcquireLockAsync())
                {
                    var lockToModify = _memoryStore.Locks[resource].First(@lock => @lock.Id == lockId);
                    lockToModify.Released = DateTime.UtcNow;
                }
            });
        }
    }
}