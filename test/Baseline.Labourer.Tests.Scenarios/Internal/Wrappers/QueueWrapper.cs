using System;
using System.Threading.Tasks;

namespace Baseline.Labourer.Tests.Scenarios.Internal;

public abstract class QueueWrapper
{
    protected readonly Guid UniqueTestId;

    public IQueue Queue { get; protected set; }

    protected QueueWrapper(Guid uniqueTestId)
    {
        UniqueTestId = uniqueTestId;
    }

    protected abstract ValueTask BootstrapAsync();

    protected abstract ValueTask DisposeAsync();
}
