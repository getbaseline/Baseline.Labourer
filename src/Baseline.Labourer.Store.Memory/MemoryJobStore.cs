using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Store.Memory
{
    public class MemoryJobStore : IDispatchedJobStore
    {
        protected readonly List<DispatchedJobDefinition> DispatchedJobs = new List<DispatchedJobDefinition>();
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

                DispatchedJobs.Add(definition);
            }
            finally
            {
                _semaphore.Release();
            }
            
            return definition;
        }

        /// <inheritdoc />
        public async Task UpdateJobAsync(
            string jobId, 
            DispatchedJobDefinition jobDefinition, 
            CancellationToken cancellationToken
        )
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);

                var relevantDispatchedJob = DispatchedJobs.FirstOrDefault(j => j.Id == jobId);
                if (relevantDispatchedJob == null)
                {
                    return;
                }

                relevantDispatchedJob.Status = jobDefinition.Status;
                relevantDispatchedJob.FinishedAt = jobDefinition.FinishedAt;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}