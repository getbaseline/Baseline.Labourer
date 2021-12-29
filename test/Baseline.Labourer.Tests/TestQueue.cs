using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Queue.Memory;
using FluentAssertions;

namespace Baseline.Labourer.Tests
{
    public class TestQueue : MemoryQueue
    {
        public void AssertMessageDispatched(Expression<Func<MemoryQueuedJob, bool>> predicate)
        {
            Queue.Should().ContainSingle(predicate);
        }

        public void AssertJobDispatchedWithIdRetryCountAndDelay(string jobId, uint retryCount, TimeSpan delay)
        {
            var jobDefinitions = RemovedQueue.Select(j => new
            {
                VisibilityDelay = j.VisibilityDelay,
                JobDefinition = JsonSerializer.Deserialize<DispatchedJobDefinition>(j.SerializedDefinition)
            });

            jobDefinitions.Should().ContainSingle(
                j => j.JobDefinition.Id == jobId && 
                     j.JobDefinition.Retries == retryCount &&
                     j.VisibilityDelay == delay
            );
        }
    }
}