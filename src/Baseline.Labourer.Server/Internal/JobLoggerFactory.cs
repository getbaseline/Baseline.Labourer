﻿using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.Internal
{
    /// <summary>
    /// JobLoggerFactory is an internal logger factory used to create <see cref="JobLogger"/> instances that optionally
    /// wrap a logger provided by a consumer of the library.
    /// </summary>
    public class JobLoggerFactory : ILoggerFactory
    {
        private readonly string _jobId;
        private readonly ILoggerFactory _wrappedLoggerFactory;
        private readonly IDispatchedJobStore _dispatchedJobStore;
        
        public JobLoggerFactory(
            string jobId,
            ILoggerFactory wrappedLoggerFactory,
            IDispatchedJobStore dispatchedJobStore
        )
        {
            _jobId = jobId;
            _wrappedLoggerFactory = wrappedLoggerFactory;
            _dispatchedJobStore = dispatchedJobStore;
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
                _wrappedLoggerFactory?.CreateLogger(categoryName),
                _dispatchedJobStore
            );
        }

        /// <inheritdoc />
        public void AddProvider(ILoggerProvider provider)
        {
            _wrappedLoggerFactory?.AddProvider(provider);
        }
    }
}