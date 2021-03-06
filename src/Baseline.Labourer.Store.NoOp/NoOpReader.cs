using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;

namespace Baseline.Labourer.Store.NoOp;

/// <summary>
/// NoOpReader is an <see cref="IStoreReader"/> implementation that literally does nothing.
/// </summary>
public class NoOpReader : IStoreReader
{
    /// <inheritdoc />
    public ValueTask<List<ScheduledJobDefinition>> GetScheduledJobsDueToRunBeforeDateAsync(
        DateTime before
    )
    {
        return new ValueTask<List<ScheduledJobDefinition>>(new List<ScheduledJobDefinition>());
    }
}
