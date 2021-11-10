using System;

namespace Baseline.Labourer
{
    /// <summary>
    /// JobDefinition is a base class that represents part of a job and its parameters defined by a consumer of the
    /// library in a normalized format.
    /// </summary>
    public abstract class JobDefinition
    {
        /// <summary>
        /// Gets or sets the fully qualified type of the job itself.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets whether the job has parameters or not.
        /// </summary>
        public bool HasParameters { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified type of the job's parameters.
        /// </summary>
        public string ParametersType { get; set; }

        /// <summary>
        /// Gets or sets the serialized JSON representation of the job.
        /// </summary>
        public string SerializedParameters { get; set; }

        /// <summary>
        /// Gets or sets when the job was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when the job was updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}