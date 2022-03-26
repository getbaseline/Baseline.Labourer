using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Tests;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Baseline.Labourer.Store.Sqlite.Tests;

public class SqliteTransactionalStoreWriterTests : BaseSqliteTest
{
    [Fact]
    public async Task It_Rolls_Back_Any_Changes_If_Commit_Not_Called()
    {
        
    }
    
    [Fact]
    public async Task It_Creates_A_Server()
    {
        // Arrange.
        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );

        // Act.
        await writer.CreateServerAsync(
            new ServerInstance { Hostname = "foo", Key = "bar" },
            CancellationToken.None
        );
        await writer.CommitAsync(CancellationToken.None);

        // Assert.
        var serverRetrievalCommand = new SqliteCommand("SELECT * FROM bl_lb_servers", Connection);
        var reader = serverRetrievalCommand.ExecuteReader();
        reader.Read();

        reader.GetString(0).Should().Be("foo-bar");
    }

    [Fact]
    public async Task It_Creates_A_Server_Heartbeat()
    {
        // Arrange.
        var serverId = CreateServer();
        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );

        // Act.
        await writer.CreateServerHeartbeatAsync(serverId, CancellationToken.None);
        await writer.CommitAsync(CancellationToken.None);

        // Assert.
        var serverHeartbeatRetrievalCommand = new SqliteCommand(
            $"SELECT COUNT(1) FROM bl_lb_server_heartbeats WHERE server_id = '{serverId}'",
            Connection
        );
        ((long)serverHeartbeatRetrievalCommand.ExecuteScalar()!).Should().Be(1);
    }

    [Fact]
    public async Task It_Creates_A_Worker()
    {
        // Arrange.
        var serverId = CreateServer();
        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );

        // Act.
        await writer.CreateWorkerAsync(
            new Worker { ServerInstanceId = serverId, Id = Guid.NewGuid().ToString() },
            CancellationToken.None
        );
        await writer.CommitAsync(CancellationToken.None);

        // Assert.
        var workerRetrievalCommand = new SqliteCommand(
            $"SELECT COUNT(1) FROM bl_lb_workers WHERE server_id = '{serverId}'",
            Connection
        );
        ((long)workerRetrievalCommand.ExecuteScalar()!).Should().Be(1);
    }

    [Fact]
    public async Task It_Creates_A_Dispatched_Job()
    {
        // Arrange.

        // Act.

        // Assert.
    }

    [Fact]
    public async Task It_Can_Create_And_Update_Dispatched_Jobs()
    {
        
    }

    private string CreateServer()
    {
        var id = Guid.NewGuid();

        var createServerCommand = new SqliteCommand(
            @"
                INSERT INTO bl_lb_servers (id, hostname, key, created_at)
                VALUES (@Id, @Hostname, @Key, @Now)
            ",
            Connection
        );
        createServerCommand.Parameters.Add(new SqliteParameter("@Id", $"hostname-{id}"));
        createServerCommand.Parameters.Add(new SqliteParameter("@Hostname", "hostname"));
        createServerCommand.Parameters.Add(new SqliteParameter("@Key", id));
        createServerCommand.Parameters.Add(new SqliteParameter("@Now", DateTime.UtcNow));
        createServerCommand.ExecuteNonQuery();

        return $"hostname-{id}";
    }
}
