using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;

namespace Baseline.Labourer.Tests.Scenarios.Internal.Wrappers;

public abstract class StoreWrapper
{
    protected readonly Guid UniqueTestId;

    public IStore Store { get; protected set; }

    protected StoreWrapper(Guid uniqueTestId)
    {
        UniqueTestId = uniqueTestId;
    }

    public virtual async ValueTask BootstrapAsync()
    {
        await Store.BootstrapAsync();
    }

    public virtual ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public abstract ValueTask<IReadOnlyCollection<ServerInstance>> GetRegisteredServersAsync();

    public abstract ValueTask<IReadOnlyCollection<Worker>> GetRegisteredWorkersAsync();
}
