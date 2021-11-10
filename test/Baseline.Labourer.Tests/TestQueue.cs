using Baseline.Labourer.Queue.Memory;
using FluentAssertions;

namespace Baseline.Labourer.Tests
{
    public class TestQueue : MemoryQueue
    {
        public void AssertMessageDispatched(QueuedJob message)
        {
            Queue.Should().ContainSingle(j => j.Equals(message));
        }
    }
}