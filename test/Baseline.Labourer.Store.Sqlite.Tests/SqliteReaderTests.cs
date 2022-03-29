using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Baseline.Labourer.Store.Sqlite.Tests;

public class SqliteReaderTests : BaseSqliteTest
{
    private readonly SqliteReader _sqliteReader;

    public SqliteReaderTests()
    {
        _sqliteReader = new SqliteReader(ConnectionString);
    }

    [Fact]
    public async Task It_Retrieves_Scheduled_Jobs_Correctly()
    {
        // Arrange.
        var beforeDate = DateTime.UtcNow;

        CreateScheduledJob(name: "before", nextRun: DateTime.UtcNow.AddDays(-7));
        CreateScheduledJob(name: "same", nextRun: beforeDate);
        CreateScheduledJob(name: "after", nextRun: DateTime.UtcNow.AddDays(7));

        // Act.
        var result = await _sqliteReader.GetScheduledJobsDueToRunBeforeDateAsync(
            beforeDate
        );

        // Assert.
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().ContainSingle(r => r.Name == "before");
        result.Should().ContainSingle(r => r.Name == "same");
        result.Should().NotContain(r => r.Name == "after");
    }
}
