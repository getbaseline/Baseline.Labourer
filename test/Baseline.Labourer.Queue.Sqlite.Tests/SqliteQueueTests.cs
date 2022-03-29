using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Store.Sqlite.Tests;
using Baseline.Labourer.Tests;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Baseline.Labourer.Queue.Sqlite.Tests;

public class SqliteQueueTests : BaseSqliteTest
{
    private readonly TestDateTimeProvider _testDateTimeProvider = new();
    private readonly SqliteQueue _queue;

    public SqliteQueueTests()
    {
        _queue = new SqliteQueue(_testDateTimeProvider, ConnectionString);
    }

    [Fact]
    public async Task It_Enqueues_A_Simple_Message_Correctly()
    {
        // Act.
        await _queue.EnqueueAsync(new { Name = "foo" }, null, CancellationToken.None);

        // Assert.
        var messagesThatMatch = (long)new SqliteCommand(
            "SELECT COUNT(1) FROM bl_lb_queue WHERE message LIKE '%foo%'",
            Connection
        ).ExecuteScalar()!;
        messagesThatMatch.Should().Be(1);
    }

    [Fact]
    public async Task It_Enqueues_A_Message_With_A_Visibility_Timeout_Correctly_And_Does_Not_Retrieve_It_Until_The_Timeout_Is_Ellapsed()
    {
        // Arrange.
        await _queue.EnqueueAsync(
            new { Name = "foo" },
            TimeSpan.FromMinutes(1),
            CancellationToken.None
        );

        // Act/Assert.
        (await _queue.DequeueAsync(CancellationToken.None))
            .Should()
            .BeNull();

        _testDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddMinutes(3));

        (await _queue.DequeueAsync(CancellationToken.None)).Should().NotBeNull();
    }

    [Fact]
    public async Task It_Hides_A_Message_After_Retrieving_It_And_The_Message_Is_Not_Retrieved_Until_The_Timeout_Is_Ellapsed()
    {
        // Arrange.
        await _queue.EnqueueAsync(new { Name = "foo" }, null, CancellationToken.None);

        // Act/Assert.
        (await _queue.DequeueAsync(CancellationToken.None))
            .Should()
            .NotBeNull();

        (await _queue.DequeueAsync(CancellationToken.None)).Should().BeNull();

        _testDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddMinutes(1));

        (await _queue.DequeueAsync(CancellationToken.None)).Should().NotBeNull();
    }

    [Fact]
    public async Task It_Deletes_A_Message()
    {
        // Arrange.
        await _queue.EnqueueAsync(new { Name = "foo" }, null, CancellationToken.None);
        var message = await _queue.DequeueAsync(CancellationToken.None);

        // Act.
        await _queue.DeleteMessageAsync(message.MessageId, CancellationToken.None);

        // Assert.
        var messagesThatMatch = (long)new SqliteCommand(
            $"SELECT COUNT(1) FROM bl_lb_queue WHERE id = {message.MessageId}",
            Connection
        ).ExecuteScalar()!;
        messagesThatMatch.Should().Be(0);
    }
}
