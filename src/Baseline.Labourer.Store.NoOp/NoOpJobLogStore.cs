using System;
using Baseline.Labourer.Contracts;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Store.NoOp
{
    /// <summary>
    /// NoOpJobLogStore is an implementation of <see cref="IJobLogStore"/> that does literally nothing.
    /// </summary>
    public class NoOpJobLogStore : IJobLogStore
    {
        /// <inheritdoc />
        public void LogEntryForJob(string jobId, LogLevel logLevel, string message, Exception? exception)
        {
        }
    }
}