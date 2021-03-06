using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.Internal.JobProcessorWorker;

/// <summary>
/// JobLoggerFactory is an internal logger factory used to create <see cref="JobLogger"/> instances that optionally
/// wrap a logger provided by a consumer of the library.
/// </summary>
internal class JobLoggerFactory : ILoggerFactory
{
    private readonly string _jobId;
    private readonly ILoggerFactory _wrappedLoggerFactory;
    private readonly IStoreWriterTransactionManager _storeWriterTransactionManager;

    public JobLoggerFactory(JobContext jobContext)
    {
        _jobId = jobContext.JobDefinition.Id;
        _wrappedLoggerFactory = jobContext.WorkerContext.ServerContext.LoggerFactory;
        _storeWriterTransactionManager =
            jobContext.WorkerContext.ServerContext.Store.WriterTransactionManager;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _wrappedLoggerFactory.Dispose();
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        return new JobLogger(
            _jobId,
            _wrappedLoggerFactory.CreateLogger(categoryName),
            _storeWriterTransactionManager
        );
    }

    /// <inheritdoc />
    public void AddProvider(ILoggerProvider provider)
    {
        _wrappedLoggerFactory.AddProvider(provider);
    }
}
