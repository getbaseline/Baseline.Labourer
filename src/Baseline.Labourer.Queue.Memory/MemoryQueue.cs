using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Contracts;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Internal.Utils;

namespace Baseline.Labourer.Queue.Memory;

/// <summary>
/// <see cref="MemoryQueue"/> is an <see cref="IQueue"/> implementation that persists and processes queue messages in memory.
/// </summary>
public class MemoryQueue : IQueue
{
    private readonly SemaphoreSlim _semaphore = new(1); // We don't want to de-queue messages when we're potentially adding some!
    private readonly IDateTimeProvider _dateTimeProvider;

    protected List<MemoryQueuedJob> Queue { get; } = new();
    protected List<MemoryQueuedJob> RemovedQueue { get; } = new();

    public MemoryQueue() : this(new DateTimeProvider()) { }

    public MemoryQueue(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc />
    public bool SupportsLongPolling => false;

    /// <inheritdoc />
    public ValueTask BootstrapAsync()
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public async Task EnqueueAsync<T>(T messageToQueue, TimeSpan? visibilityDelay)
    {
        try
        {
            await _semaphore.WaitAsync();

            Queue.Add(
                new MemoryQueuedJob
                {
                    MessageId = StringGenerationUtils.GenerateUniqueRandomString(),
                    SerializedDefinition = await SerializationUtils.SerializeToStringAsync(
                        messageToQueue
                    ),
                    VisibilityDelay = visibilityDelay,
                    EnqueuedAt = _dateTimeProvider.UtcNow()
                }
            );
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask<QueuedJob?> DequeueAsync()
    {
        try
        {
            // This semaphore will prevent other queues from snatching up our messages!
            await _semaphore.WaitAsync();

            var firstMessage = Queue.FirstOrDefault(
                q =>
                    q.EnqueuedAt.Add(q.VisibilityDelay ?? TimeSpan.Zero)
                    <= _dateTimeProvider.UtcNow()
            );

            if (firstMessage == null)
            {
                return null;
            }

            firstMessage.PreviousVisibilityDelay = firstMessage.VisibilityDelay;
            firstMessage.VisibilityDelay = (
                _dateTimeProvider.UtcNow() - firstMessage.EnqueuedAt
            ).Add(TimeSpan.FromSeconds(30));

            return firstMessage;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask DeleteMessageAsync(string messageId)
    {
        try
        {
            await _semaphore.WaitAsync();

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
