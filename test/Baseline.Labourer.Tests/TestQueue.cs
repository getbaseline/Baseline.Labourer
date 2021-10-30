using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Utils;

namespace Baseline.Labourer.Tests
{
    public class TestQueue : IQueue
    {
        public List<QueuedJob> _queue = new List<QueuedJob>();
        
        public async Task EnqueueAsync<T>(QueuedMessageType queuedMessageType, T messageToQueue, CancellationToken cancellationToken)
        {
            _queue.Add(new QueuedJob
            {
                Type = queuedMessageType,
                SerializedDefinition = await SerializationUtils.SerializeToStringAsync(messageToQueue, cancellationToken)
            });
        }

        public Task<QueuedJob?> DequeueAsync(CancellationToken cancellationToken)
        {
            if (!_queue.Any())
            {
                return null;
            }

            var q = _queue.First();
            _queue = _queue.Skip(1).ToList();
            return Task.FromResult(q);
        }

        public void AssertMessageDispatched(QueuedJob message)
        {
            if (_queue.All(j => !j.Equals(message)))
            {
                throw new Exception("No dispatched messages found that match message provided.");
            }
        }
    }
}