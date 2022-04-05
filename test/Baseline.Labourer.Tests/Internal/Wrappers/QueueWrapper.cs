using System;
using System.Threading.Tasks;

namespace Baseline.Labourer.Tests.Internal.Wrappers;

public abstract class QueueWrapper
{
    protected readonly Guid UniqueTestId;

    public IQueue Queue { get; protected set; }

    protected QueueWrapper(Guid uniqueTestId)
    {
        UniqueTestId = uniqueTestId;
    }

    public virtual async ValueTask BootstrapAsync()
    {
        await Queue.BootstrapAsync();
    }

    public virtual ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
