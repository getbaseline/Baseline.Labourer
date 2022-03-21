using System;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer;

/// <summary>
/// Defines what job log stores must implement. This is separate from the rest of the transactional write stores for two reasons: <br /><br />
/// 
/// 1. Adhering to Microsoft's logging framework requires loggers to be implemented synchronously.<br />
/// 2. This does not need to be transactional: there are no foreign keys or constraints on any log storage, so at worst the log messages are just orphaned.
/// </summary>
public interface IJobLogStore
{
    /// <summary>
    /// Creates and saves a log entry against a job specified by the job id parameter.
    /// </summary>
    /// <param name="jobId">The id of the job that should have a log entry created against it.</param>
    /// <param name="logLevel">The logging level, i.e. the severity of the log.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">An optional exception, if there was one present.</param>
    void LogEntryForJob(
        string jobId,
        LogLevel logLevel,
        string message,
        Exception? exception
    );
}