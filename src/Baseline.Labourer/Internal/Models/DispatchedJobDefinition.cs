using System;
using Baseline.Labourer.Internal.Utils;

namespace Baseline.Labourer.Internal.Models
{
    /// <summary>
    /// A model that represents a dispatched job. Differs to other job models in that this is the only job definition
    /// that is ran by the worker.
    /// </summary>
    public class DispatchedJobDefinition : JobDefinition
    {
        /// <summary>
        /// Gets or sets the identifier of the dispatched job.
        /// </summary>
        public string Id { get; set; } = StringGenerationUtils.GenerateUniqueRandomString();

        /// <summary>
        /// Gets or sets the amount of retries the job has had executed.
        /// </summary>
        public int Retries { get; set; }

        /// <summary>
        /// Gets or sets the status of the dispatched job.
        /// </summary>
        public JobStatus Status { get; set; }

        /// <summary>
        /// Gets or sets when the job finished or null if it hasn't yet.
        /// </summary>
        public DateTime? FinishedAt { get; set; }

        public DispatchedJobDefinition()
        {
        }

        public DispatchedJobDefinition(JobDefinition jobDefinition)
        {
            Type = jobDefinition.Type;
            HasParameters = jobDefinition.HasParameters;
            ParametersType = jobDefinition.ParametersType;
            SerializedParameters = jobDefinition.SerializedParameters;
        }
    }
}