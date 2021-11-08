using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer
{
    /// <summary>
    /// A contract that provides the consumer level API for all Baseline.Labourer job management functions
    /// (such as dispatching jobs and registering recurrent ones).
    /// </summary>
    public interface ILabourerClient
    {
        /// <summary>
        /// Dispatches a job without parameters to run immediately and returns the created id of the job.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task<string> DispatchJobAsync<TJob>(CancellationToken cancellationToken) where TJob : IJob;

        /// <summary>
        /// Dispatches a job with parameters to run immediately and returns the created id of the job.
        /// </summary>
        /// <param name="jobParameters">
        /// The parameters for the job, serialized to be stored and then deserialized to become the parameter for
        /// the HandleAsync method of the job.
        /// </param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task<string> DispatchJobAsync<TParams, TJob>(
            TParams jobParameters,
            CancellationToken cancellationToken = default
        ) where TJob : IJob<TParams>;
    }
}