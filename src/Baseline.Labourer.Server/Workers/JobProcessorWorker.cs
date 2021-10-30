using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Utils;

namespace Baseline.Labourer.Server.Workers
{
    /// <summary>
    /// A worker that processes jobs that need to be ran.
    /// </summary>
    public class JobProcessorWorker
    {
        private readonly BaselineServerConfiguration _baselineServerConfiguration;
        private readonly IDispatchedJobStore _dispatchedJobStore;
        private readonly IQueue _queue;

        public JobProcessorWorker(
            BaselineServerConfiguration baselineServerConfiguration,
            IDispatchedJobStore dispatchedJobStore, 
            IQueue queue
        )
        {
            _baselineServerConfiguration = baselineServerConfiguration;
            _dispatchedJobStore = dispatchedJobStore;
            _queue = queue;
        }

        /// <summary>
        /// Runs the worker, creating a number of parallel worker instances that process the jobs.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task RunAsync()
        {
            var processingTasks = Enumerable
                .Range(1, _baselineServerConfiguration.JobProcessorTasksToRun)
                .Select(async _ => await RunSingleAsync());

            await Task.WhenAll(processingTasks);
        }

        private async Task RunSingleAsync()
        {
            while (true)
            {
                if (_baselineServerConfiguration.ShutdownTokenSource.IsCancellationRequested)
                {
                    return;
                }
                
                var dequeuedMessage = await _queue.DequeueAsync(CancellationToken.None);

                if (dequeuedMessage == null)
                {
                    continue;
                }

                if (dequeuedMessage.Type is QueuedMessageType.UserEnqueuedJob)
                {
                    await ProcessMessageAsJobAsync(dequeuedMessage);
                }
            }
        }

        private async Task ProcessMessageAsJobAsync(
            QueuedJob dequeuedMessage, 
            CancellationToken cancellationToken = default
        )
        {
            var deserialisedJob = await SerializationUtils.DeserializeFromStringAsync<DispatchedJobDefinition>(
                dequeuedMessage.SerializedDefinition,
                cancellationToken
            );

            var parametersType = Type.GetType(deserialisedJob.ParametersType);
            var jobType = Type.GetType(deserialisedJob.Type);

            var deserializedParameters = await SerializationUtils.DeserializeFromStringAsync(
                deserialisedJob.SerializedParameters, 
                parametersType,
                cancellationToken
            );
            var jobInstance = Activator.CreateInstance(jobType);

            var task = jobType
                .GetMethod("HandleAsync")!
                .Invoke(jobInstance, new[] { deserializedParameters, CancellationToken.None }) as Task;

            await task;
        }
    }
}