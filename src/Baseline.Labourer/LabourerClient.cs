﻿using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Utils;

namespace Baseline.Labourer
{
    /// <summary>
    /// LabourerClient is the default (and ideally only) implementation of the <see cref="ILabourerClient"/> interface.
    /// </summary>
    public class LabourerClient : ILabourerClient
    {
        private readonly BaselineLabourerConfiguration _configuration;
        private readonly IDispatchedJobStore _dispatchedJobStore;
        private readonly IQueue _queue;

        public LabourerClient(
            BaselineLabourerConfiguration configuration, 
            IDispatchedJobStore dispatchedJobStore, 
            IQueue queue
        )
        {
            _configuration = configuration;
            _dispatchedJobStore = dispatchedJobStore;
            _queue = queue;
        }

        /// <inheritdoc />
        public async Task DispatchJobAsync<TParams, TJob>(
            TParams jobParameters, 
            CancellationToken cancellationToken = default
        ) where TJob : IJob<TParams>
        {
            var jobDefinition = new DispatchedJobDefinition
            {
                Type = typeof(TJob).AssemblyQualifiedName,
                ParametersType = typeof(TParams).AssemblyQualifiedName,
                SerializedParameters = await SerializationUtils.SerializeToStringAsync(jobParameters, cancellationToken)
            };
            
            await _dispatchedJobStore.SaveDispatchedJobDefinitionAsync(jobDefinition, cancellationToken);
            await _queue.EnqueueAsync(QueuedMessageType.UserEnqueuedJob, jobDefinition, cancellationToken);
        }
    }
}