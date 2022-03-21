using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;

namespace Baseline.Labourer;

/// <summary>
/// <see cref="MemoryQueue"/> is an <see cref="IQueue"/> implementation that persists and processes queue messages in memory.
/// </summary>
public class MemoryQueue : IQueue
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1); // We don't want to de-queue messages when we're potentially adding some!
    private readonly IDateTimeProvider _dateTimeProvider;
        
    protected List<MemoryQueuedJob> Queue { get; } = new List<MemoryQueuedJob>();
    protected List<MemoryQueuedJob> RemovedQueue { get; } = new List<MemoryQueuedJob>();

    public MemoryQueue() : this(new DateTimeProvider())
    {
    }

    public MemoryQueue(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc />
    public ValueTask BootstrapAsync()
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public async Task EnqueueAsync<T>(
        T messageToQueue,
        TimeSpan? visibilityDelay,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);

            Queue.Add(new MemoryQueuedJob
            {
                MessageId = StringGenerationUtils.GenerateUniqueRandomString(),
                SerializedDefinition = await SerializationUtils.SerializeToStringAsync(
                    messageToQueue,
                    cancellationToken
                ),
                VisibilityDelay = visibilityDelay,
                EnqueuedAt = _dateTimeProvider.UtcNow()
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

                var firstMessage = Queue.FirstOrDefault(
                    q => q.EnqueuedAt.Add(q.VisibilityDelay ?? TimeSpan.Zero) <= _dateTimeProvider.UtcNow()
                );

                if (firstMessage == null)
                {
                    released = true;
                    _semaphore.Release();
                    await Task.Delay(1000, cancellationToken);
                    continue;
                }

                firstMessage.PreviousVisibilityDelay = firstMessage.VisibilityDelay;
                firstMessage.VisibilityDelay = (_dateTimeProvider.UtcNow() - firstMessage.EnqueuedAt).Add(TimeSpan.FromSeconds(30));

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

            var messagesToRemove = Queue.Where(qm => qm.MessageId == messageId).ToList();
                
            RemovedQueue.AddRange(messagesToRemove);
            Queue.RemoveAll(qm => messagesToRemove.Any(m => m.MessageId == qm.MessageId));
        }
        finally
        {
            _semaphore.Release();
        }
    }
}