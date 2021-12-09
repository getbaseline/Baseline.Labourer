using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Models;

namespace Baseline.Labourer.Store.Memory
{
    /// <summary>
    /// Store reader implementation for the memory store. 
    /// </summary>
    public class MemoryStoreReader : IStoreReader
    {
        private readonly MemoryStore _memoryStore;

        public MemoryStoreReader(MemoryStore memoryStore)
        {
            _memoryStore = memoryStore;
        }

        /// <inheritdoc />
        public async ValueTask<List<ScheduledJobDefinition>> GetScheduledJobsDueToRunBeforeDateAsync(
            DateTime before, 
            CancellationToken cancellationToken
        )
        {
            using var _ = await _memoryStore.AcquireLockAsync();

            return _memoryStore.ScheduledJobs
                .Where(job => job.NextRunDate <= before)
                .ToList();
        }
    }
}