using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;

namespace Baseline.Labourer;

/// <summary>
/// SQLite backed reader.
/// </summary>
public class SqliteReader : IStoreReader
{
    /// <inheritdoc />
    public ValueTask<List<ScheduledJobDefinition>> GetScheduledJobsDueToRunBeforeDateAsync(DateTime before,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}