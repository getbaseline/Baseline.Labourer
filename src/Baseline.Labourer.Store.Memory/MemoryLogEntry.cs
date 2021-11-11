using System;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Store.Memory
{
    /// <summary>
    /// <see cref="MemoryLogEntry" /> represents a log entry that is stored in memory. As it is not truly persisted anywhere,
    /// this is the best way to persist all elements of the log entry without using a ridiculous tuple.
    /// </summary>
    public class MemoryLogEntry
    {
        /// <summary>
        /// Gets or sets the id of the job.
        /// </summary>
        public string JobId { get; set; }

        /// <summary>
        /// Gets or sets the level of the log.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets the message of the log.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the exception of the log (if applicable).
        /// </summary>
        public Exception Exception { get; set; }
    }
}