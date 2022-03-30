using System;
using System.Threading.Tasks;
using Baseline.Labourer.Tests;
using FluentAssertions;
using Xunit;

namespace Baseline.Labourer.Queue.Memory.Tests;

public class MemoryQueueTests
{
    private readonly TestDateTimeProvider _dateTimeProvider = new TestDateTimeProvider();
    private readonly TestMemoryQueue _memoryQueue;

    public MemoryQueueTests()
    {
        _memoryQueue = new TestMemoryQueue(_dateTimeProvider);
    }

    [Fact]
    public async Task It_Enqueues_A_Simple_Message_Correctly()
    {
        // Arrange.
        var message = new { A = "b" };

        // Act.
        await _memoryQueue.EnqueueAsync(message, null);

        // Assert.
        _memoryQueue.AssertMessageDispatched(j => j.SerializedDefinition == "{\"A\":\"b\"}");
    }

    [Fact]
    public async Task It_Enqueues_A_Message_With_A_Visibility_Timeout_Correctly_And_Does_Not_Retrieve_It_Until_The_Timeout_Is_Ellapsed()
    {
        // Arrange.
        var message = new { A = "b" };

        // Act.
        await _memoryQueue.EnqueueAsync(message, TimeSpan.FromMinutes(1));

        // Assert.
        _memoryQueue.AssertMessageDispatched(
            j =>
                j.SerializedDefinition == "{\"A\":\"b\"}"
                && j.VisibilityDelay == TimeSpan.FromMinutes(1)
        );

        _dateTimeProvider.SetUtcNow(DateTime.UtcNow.AddMinutes(2));

        var dequeuedMessage = (MemoryQueuedJob)(await _memoryQueue.DequeueAsync())!;

        dequeuedMessage.SerializedDefinition.Should().Be("{\"A\":\"b\"}");
        dequeuedMessage.PreviousVisibilityDelay.Should().Be(TimeSpan.FromMinutes(1));
        dequeuedMessage.VisibilityDelay
            .Should()
            .Be(
                (_dateTimeProvider.UtcNow() - dequeuedMessage.EnqueuedAt).Add(
                    TimeSpan.FromSeconds(30)
                )
            );
    }
}
