using System.Threading.Tasks;
using Baseline.Labourer.Queue.Sqlite;
using Baseline.Labourer.Store.Sqlite;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.DependencyInjection.Tests;

public class SqliteTests : BaseDependencyInjectionTest
{
    public class SqliteJob : IJob
    {
        public static bool Ran = false;

        public ValueTask HandleAsync()
        {
            Ran = true;
            return ValueTask.CompletedTask;
        }
    }

    public SqliteTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [Fact]
    public async Task It_Can_Create_And_Run_Using_The_Sqlite_Queue_And_Store()
    {
        // Arrange.
        var connectionString = $"Data Source=InMemory;Mode=Memory;Cache=Shared";

        var connection = new SqliteConnection(connectionString);
        connection.Open();

        ConfigureServices(
            (_, builder) =>
            {
                builder.UseSqliteStore(connectionString);
                builder.UseSqliteQueue(connectionString);
            }
        );

        RunServer();

        // Act.
        await Task.Delay(1000); // This sucks but need to wait for it to bootstrap the relevant tables.
        var jobId = await Client.DispatchJobAsync<SqliteJob>();

        // Assert.
        jobId.Should().NotBeEmpty();
        await AssertionUtils.RetryAsync(
            () =>
            {
                SqliteJob.Ran.Should().BeTrue();
            }
        );
    }
}
