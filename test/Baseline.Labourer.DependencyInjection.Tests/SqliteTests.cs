using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Tests;
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

        public ValueTask HandleAsync(CancellationToken cancellationToken)
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
        var connectionString = $"Data Source={Guid.NewGuid()};Mode=Memory;Cache=Shared";

        var connection = new SqliteConnection(connectionString);
        connection.Open();

        ConfigureServices(
            (_, builder) =>
            {
                builder.UseSqliteStore(connectionString);
                builder.UseSqliteQueue(connectionString);

                builder.ConfigureServer(s => s.RunThisManyJobProcessingWorkers(1));
            }
        );

        RunServer();

        // Act.
        await Task.Delay(3000);
        var jobId = await Client.DispatchJobAsync<SqliteJob>();

        // Assert.
        jobId.Should().NotBeEmpty();
        await AssertionUtils.RetryAsync(
            () =>
            {
                SqliteJob.Ran.Should().BeTrue();
            },
            100000
        );
    }
}
