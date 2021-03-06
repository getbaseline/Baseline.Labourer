using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Models;

namespace Baseline.Labourer;

/// <summary>
/// Allows the querying of the store without engaging in transactions.
/// </summary>
public interface IStoreReader
{
    /// <summary>
    /// Retrieves the scheduled jobs that are due to run before the date provided.
    /// </summary>
    /// <param name="before">The date that should be used to retrieve the scheduled jobs that need to run.</param>
    ValueTask<List<ScheduledJobDefinition>> GetScheduledJobsDueToRunBeforeDateAsync(
        DateTime before
    );
}
