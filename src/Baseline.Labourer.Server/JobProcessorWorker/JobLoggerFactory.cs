using Baseline.Labourer.Contracts;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.JobProcessorWorker
{
    /// <summary>
    /// JobLoggerFactory is an internal logger factory used to create <see cref="JobLogger"/> instances that optionally
    /// wrap a logger provided by a consumer of the library.
    /// </summary>
    public class JobLoggerFactory : ILoggerFactory
    {
        private readonly string _jobId;
        private readonly ILoggerFactory _wrappedLoggerFactory;
        private readonly IJobLogStore _jobLogStore;

        public JobLoggerFactory(JobContext jobContext)
        {
            _jobId = jobContext.JobDefinition.Id;
            _wrappedLoggerFactory = jobContext.WorkerContext.ServerContext.LoggerFactory;
            _jobLogStore = jobContext.WorkerContext.ServerContext.JobLogStore;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _wrappedLoggerFactory?.Dispose();
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return new JobLogger(
                _jobId,
                _wrappedLoggerFactory.CreateLogger(categoryName),
                _jobLogStore
            );
        }

        /// <inheritdoc />
        public void AddProvider(ILoggerProvider provider)
        {
            _wrappedLoggerFactory.AddProvider(provider);
        }
    }
}