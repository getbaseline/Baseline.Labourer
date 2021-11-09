using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Utils;

namespace Baseline.Labourer.Queue.Memory
{
    /// <summary>
    /// <see cref="MemoryQueue"/> is an <see cref="IQueue"/> implementation that persists and processes queue messages in memory.
    /// </summary>
    public class MemoryQueue : IQueue
    {
        protected List<QueuedJob> Queue = new List<QueuedJob>();

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1); // We don't want to de-queue messages when we're potentially adding some!

        /// <inheritdoc />
        public async Task EnqueueAsync<T>(T messageToQueue, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                
                Queue.Add(new QueuedJob
                {
                    MessageId = StringGenerationUtils.GenerateUniqueRandomString(),
                    SerializedDefinition = await SerializationUtils.SerializeToStringAsync(
                        messageToQueue, 
                        cancellationToken
                    )
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
                var released = false; 
                
                try
                {
                    // This semaphore will prevent other queues from snatching up our messages!
                    await _semaphore.WaitAsync(cancellationToken);

                    if (!Queue.Any())
                    {
                        released = true;
                        _semaphore.Release();
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }
                    
                    // TODO: Mark message as invisible and timeout.
                    var firstMessage = Queue.First();
                    Queue = Queue.Skip(1).ToList();

                    return firstMessage;
                }
                finally
                {
                    if (!released)
                    {
                        _semaphore.Release();
                    }
                }
            }

            return null;
        }

        /// <inheritdoc />
        public async Task DeleteMessageAsync(string messageId, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                Queue.RemoveAll(q => q.MessageId == messageId);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}