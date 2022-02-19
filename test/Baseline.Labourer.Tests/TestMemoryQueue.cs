using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using Baseline.Labourer.Internal;
using FluentAssertions;

namespace Baseline.Labourer.Tests
{
    public class TestMemoryQueue : MemoryQueue
    {
        public TestMemoryQueue(IDateTimeProvider dateTimeProvider) : base(new MemoryQueueDataContainer(), dateTimeProvider)
        {
        }
        
        public void AssertMessageDispatched(Expression<Func<MemoryQueuedJob, bool>> predicate)
        {
            _dataContainer.Queue.Should().ContainSingle(predicate);
        }

        public void AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(string jobId, uint retryCount, TimeSpan delay)
        {
            var jobDefinitions = _dataContainer.RemovedQueue.Select(j => new
            {
                VisibilityDelay = j.PreviousVisibilityDelay,
                JobDefinition = JsonSerializer.Deserialize<DispatchedJobDefinition>(j.SerializedDefinition)!
            });

            jobDefinitions.Should().ContainSingle(
                j => j.JobDefinition.Id == jobId && 
                     j.JobDefinition.Retries == retryCount &&
                     j.VisibilityDelay == delay
            );
        }

        public void MakeAllMessagesVisible()
        {
            _dataContainer.Queue.ForEach(q => q.VisibilityDelay = null);
        }
    }
}