using System;
using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer
{
    /// <summary>
    /// A contract that defines what all dispatched job stores must implement. This is a "slimmed down" interface of
    /// what job stores implement applicable to only immediately dispatched jobs.
    /// </summary>
    public interface IDispatchedJobStore
    {
        /// <summary>
        /// Saves a dispatched job to the job store.
        /// </summary>
        /// <param name="definition">The definition object.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task<DispatchedJobDefinition> SaveDispatchedJobDefinitionAsync(
            DispatchedJobDefinition definition, 
            CancellationToken cancellationToken
        );

        /// <summary>
        /// Changes the state of the job within the job store.
        /// </summary>
        /// <param name="dispatchedJobId">The id of the job that needs its state changing.</param>
        /// <param name="definition">The new definition of the job.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task UpdateJobAsync(
            string dispatchedJobId, 
            DispatchedJobDefinition definition,
            CancellationToken cancellationToken
        );
    }
}