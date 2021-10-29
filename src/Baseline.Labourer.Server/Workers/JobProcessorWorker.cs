using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Server
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
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var processingTasks = Enumerable
                .Range(1, _baselineServerConfiguration.JobProcessorTasksToRun)
                .Select(async _ => await RunSingleAsync(cancellationToken));

            await Task.WhenAll(processingTasks);
        }

        private async Task RunSingleAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var dequeuedMessage = await _queue.DequeueAsync(cancellationToken);
                
                // Deserialize the jobs, then execute the jobs in a wrapped context logging when required and updating
                // the job store along the way.
            }
        }
    }
}