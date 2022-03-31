using System;
using System.Threading.Tasks;

namespace Baseline.Labourer.Tests.Scenarios.Internal;

public abstract class StoreWrapper
{
    protected readonly Guid UniqueTestId;

    public IStore Store { get; }

    protected StoreWrapper(Guid uniqueTestId)
    {
        UniqueTestId = uniqueTestId;
    }

    protected abstract Task DisposeAsync();
}
