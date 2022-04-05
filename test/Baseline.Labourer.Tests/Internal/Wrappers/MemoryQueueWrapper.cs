using System;
using Baseline.Labourer.Queue.Memory;

namespace Baseline.Labourer.Tests.Internal.Wrappers;

public class MemoryQueueWrapper : QueueWrapper
{
    public MemoryQueueWrapper(Guid uniqueTestId) : base(uniqueTestId)
    {
        Queue = new MemoryQueue();
    }
}
