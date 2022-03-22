using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;

namespace Baseline.Labourer;

/// <summary>
/// NoOpReader is an <see cref="IStoreReader"/> implementation that literally does nothing.
/// </summary>
public class NoOpReader : IStoreReader
{
    /// <inheritdoc />
    public ValueTask<List<ScheduledJobDefinition>> GetScheduledJobsDueToRunBeforeDateAsync(
        DateTime before,
        CancellationToken cancellationToken
    )
    {
        return new ValueTask<List<ScheduledJobDefinition>>(new List<ScheduledJobDefinition>());
    }
}
