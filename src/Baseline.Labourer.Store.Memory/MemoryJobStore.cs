using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Store.Memory
{
    public class MemoryJobStore : IDispatchedJobStore
    {
        private readonly List<DispatchedJobDefinition> _dispatchedJobs = new List<DispatchedJobDefinition>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        /// <inheritdoc />
        public async Task<DispatchedJobDefinition> SaveDispatchedJobDefinitionAsync(
            DispatchedJobDefinition definition, 
            CancellationToken cancellationToken
        )
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);

                _dispatchedJobs.Add(definition);
            }
            finally
            {
                _semaphore.Release();
            }
            
            return definition;
        }
    }
}