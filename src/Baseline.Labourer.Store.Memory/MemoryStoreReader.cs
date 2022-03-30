using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;

namespace Baseline.Labourer;

/// <summary>
/// Store reader implementation for the memory store.
/// </summary>
public class MemoryStoreReader : IStoreReader
{
    private readonly MemoryStoreDataContainer _memoryStoreDataContainer;

    public MemoryStoreReader(MemoryStoreDataContainer memoryStoreDataContainer)
    {
        _memoryStoreDataContainer = memoryStoreDataContainer;
    }

    /// <inheritdoc />
    public async ValueTask<List<ScheduledJobDefinition>> GetScheduledJobsDueToRunBeforeDateAsync(
        DateTime before
    )
    {
        using var _ = await _memoryStoreDataContainer.AcquireStoreLockAsync();

        return _memoryStoreDataContainer.ScheduledJobs.Values
            .Where(job => job.NextRunDate <= before)
            .ToList();
    }
}
