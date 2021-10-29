using System;

namespace Baseline.Labourer
{
    /// <summary>
    /// A model that represents a dispatched job. Differs to other job models in that this is the only job definition
    /// that is ran by the worker.
    /// </summary>
    public class DispatchedJobDefinition : JobDefinition
    {
        /// <summary>
        /// Gets or sets when the job finished or null if it hasn't yet.
        /// </summary>
        public DateTime? FinishedAt { get; set; }
    }
}