using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Utils;

namespace Baseline.Labourer.Queue.Memory
{
    public class MemoryQueue : IQueue
    {
        protected List<QueuedJob> Queue = new List<QueuedJob>();

        private readonly SemaphoreSlim
            _semaphore = new SemaphoreSlim(1); // We don't want to de-queue messages when we're potentially adding some!

        /// <inheritdoc />
        public async Task EnqueueAsync<T>(
            QueuedMessageType queuedMessageType,
            T messageToQueue,
            CancellationToken cancellationToken
        )
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                
                Queue.Add(new QueuedJob
                {
                    Type = queuedMessageType,
                    SerializedDefinition = await SerializationUtils.SerializeToStringAsync(messageToQueue, cancellationToken)
                });
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task<QueuedJob?> DequeueAsync(CancellationToken cancellationToken)
        {
            for (var i = 0; i < 30; i++)
            {
                try
                {
                    // This semaphore will prevent other queues from snatching up our messages!
                    await _semaphore.WaitAsync(cancellationToken);

                    if (!Queue.Any())
                    {
                        _semaphore.Release();
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }

                    var firstMessage = Queue.First();
                    Queue = Queue.Skip(1).ToList();

                    return firstMessage;
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return null;
        }
    }
}