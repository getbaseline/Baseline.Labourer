using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Contracts;
using Baseline.Labourer.Internal.Models;

namespace Baseline.Labourer.Internal
{
    public class JobDispatcher
    {
        private readonly IStoreWriterTransactionManager _transactionManager;
        private readonly IQueue _queue;

        public JobDispatcher(IStoreWriterTransactionManager transactionManager, IQueue queue)
        {
            _transactionManager = transactionManager;
            _queue = queue;
        }

        public async Task<string> DispatchJobAsync(
            DispatchedJobDefinition jobDefinition, 
            CancellationToken cancellationToken
        )
        {
            await using var writer = _transactionManager.BeginTransaction();

            await writer.CreateDispatchedJobDefinitionAsync(jobDefinition, cancellationToken);
            await _queue.EnqueueAsync(jobDefinition, cancellationToken);

            return jobDefinition.Id;
        }
    }
}