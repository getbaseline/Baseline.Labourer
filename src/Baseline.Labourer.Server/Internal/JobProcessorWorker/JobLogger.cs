using System;
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
    private readonly IJobLogStore _jobLogStore;

    public JobLogger(string jobId, ILogger wrappedLogger, IJobLogStore jobLogStore)
    {
        _jobId = jobId;
        _wrappedLogger = wrappedLogger;
        _jobLogStore = jobLogStore;
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
        try
        {
            if (_wrappedLogger.IsEnabled(logLevel))
            {
                _wrappedLogger.Log(logLevel, eventId, state, exception, formatter);
            }

            _jobLogStore.LogEntryForJob(_jobId, logLevel, formatter(state, exception), exception);
        }
        catch (Exception)
        {
            // Wrap all exceptions cos' the last thing you want to happen is your logger breaks your application!
        }
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
