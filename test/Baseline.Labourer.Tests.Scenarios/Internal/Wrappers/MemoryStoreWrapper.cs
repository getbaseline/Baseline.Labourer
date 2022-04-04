using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Store.Memory;

namespace Baseline.Labourer.Tests.Scenarios.Internal.Wrappers;

public class MemoryStoreWrapper : StoreWrapper
{
    private readonly MemoryStoreDataContainer _memoryStoreDataContainer = new();

    public MemoryStoreWrapper(Guid uniqueTestId) : base(uniqueTestId)
    {
        Store = new MemoryStore(_memoryStoreDataContainer);
    }

    public override ValueTask<IReadOnlyCollection<ServerInstance>> GetRegisteredServersAsync()
    {
        return ValueTask.FromResult(
            (IReadOnlyCollection<ServerInstance>)_memoryStoreDataContainer.Servers
        );
    }

    public override ValueTask<IReadOnlyCollection<Worker>> GetRegisteredWorkersAsync()
    {
        return ValueTask.FromResult(
            (IReadOnlyCollection<Worker>)_memoryStoreDataContainer.ServerWorkers
                .SelectMany(x => x.Value)
                .ToList()
        );
    }
}
