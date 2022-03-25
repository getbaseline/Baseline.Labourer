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
}
