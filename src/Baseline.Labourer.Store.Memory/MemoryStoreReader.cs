using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;

namespace Baseline.Labourer
{
    /// <summary>
    /// Store reader implementation for the memory store. 
    /// </summary>
    public class MemoryStoreReader : IStoreReader
    {
        private readonly MemoryBackingStore _memoryBackingStore;

        public MemoryStoreReader(MemoryBackingStore memoryBackingStore)
        {
            _memoryBackingStore = memoryBackingStore;
        }

        /// <inheritdoc />
        public async ValueTask<List<ScheduledJobDefinition>> GetScheduledJobsDueToRunBeforeDateAsync(
            DateTime before, 
            CancellationToken cancellationToken
        )
        {
            using var _ = await _memoryBackingStore.AcquireStoreLockAsync();

            return _memoryBackingStore.ScheduledJobs
                .Values
                .Where(job => job.NextRunDate <= before)
                .ToList();
        }
    }
}