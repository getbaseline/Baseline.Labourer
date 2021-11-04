namespace Baseline.Labourer.Server.Internal
{
    /// <summary>
    /// JobContext provides context and dependencies around a job that is running/is to be ran.
    /// </summary>
    public class JobContext
    {
        /// <summary>
        /// Gets or sets the context of the worker that the job is running in.
        /// </summary>
        public WorkerContext WorkerContext { get; set; }
        
        /// <summary>
        /// Gets or sets the definition of the job that is being ran.
        /// </summary>
        public DispatchedJobDefinition JobDefinition { get; set; }
        
        /// <summary>
        /// Gets or sets the state changer for the job.
        /// </summary>
        public JobStateChanger JobStateChanger { get; set; }
    }
}