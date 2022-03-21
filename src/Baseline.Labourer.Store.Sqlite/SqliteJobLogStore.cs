using System;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer;

/// <summary>
/// SQLite backed job log store.
/// </summary>
public class SqliteJobLogStore : IJobLogStore
{
    /// <inheritdoc />
    public void LogEntryForJob(string jobId, LogLevel logLevel, string message, Exception? exception)
    {
        throw new NotImplementedException();
    }
}