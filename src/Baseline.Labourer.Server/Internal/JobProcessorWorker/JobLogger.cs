using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.Internal;

/// <summary>
/// JobLogger is an internal logger used to log messages and exceptions to the relevant job store whilst optionally
/// wrapping a consumer provided logger to log the messages in a way configured by them too.
/// </summary>
internal class JobLogger : ILogger
{
    private readonly string _jobId;
    private readonly ILogger _wrappedLogger;
    private readonly IStoreWriterTransactionManager _storeWriterTransactionManager;

    public JobLogger(
        string jobId,
        ILogger wrappedLogger,
        IStoreWriterTransactionManager storeWriterTransactionManager
    )
    {
        _jobId = jobId;
        _wrappedLogger = wrappedLogger;
        _storeWriterTransactionManager = storeWriterTransactionManager;
    }

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        Task.Run(
            async () =>
            {
                try
                {
                    if (_wrappedLogger.IsEnabled(logLevel))
                    {
                        _wrappedLogger.Log(logLevel, eventId, state, exception, formatter);
                    }

                    await using var transaction = _storeWriterTransactionManager.BeginTransaction();
                    await transaction.LogEntryForJobAsync(
                        _jobId,
                        logLevel,
                        formatter(state, exception),
                        exception
                    );
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    // Wrap all exceptions cos' the last thing you want to happen is your logger breaks your application!
                }
            }
        );
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state)
    {
        return new NoOpDisposable();
    }

    private class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
