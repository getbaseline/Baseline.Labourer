using System;

namespace Baseline.Labourer.Server
{
    /// <summary>
    /// Configures how retries should be attempted.
    /// </summary>
    public class RetryConfiguration
    {
        /// <summary>
        /// Gets the number of retries that should be attempted.
        /// </summary>
        public uint Retries { get; set; }

        public RetryConfiguration(uint retries)
        {
            Retries = retries;
        }

        /// <summary>
        /// Gets the default retry configuration.
        /// </summary>
        public static RetryConfiguration Default => new RetryConfiguration(3);
    }
}