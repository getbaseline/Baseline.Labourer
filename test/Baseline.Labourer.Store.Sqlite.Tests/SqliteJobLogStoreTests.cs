using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Baseline.Labourer.Store.Sqlite.Tests;

public class SqliteJobLogStoreTests : BaseSqliteTest
{
    private readonly string _jobId = Guid.NewGuid().ToString();
    private readonly SqliteJobLogStore _jobLogStore;

    public SqliteJobLogStoreTests()
    {
        _jobLogStore = new SqliteJobLogStore(ConnectionString);
    }

    [Fact]
    public async Task It_Successfully_Logs_The_Job_Log_Message()
    {
        // Arrange.
        var exception = new ArgumentException("foo", "bar");

        // Act.
        _jobLogStore.LogEntryForJob(_jobId, LogLevel.Critical, "Foo bar.", exception);

        // Assert.
        var logEntryRetrievalCommand = new SqliteCommand(
            $"SELECT id, job_id, log_level, message, exception FROM bl_lb_job_logs WHERE job_id = '{_jobId}'",
            Connection
        ).ExecuteReader();

        logEntryRetrievalCommand.Read();

        logEntryRetrievalCommand.GetString(1).Should().Be(_jobId);
        logEntryRetrievalCommand.GetString(2).Should().Be(LogLevel.Critical.ToString());
        logEntryRetrievalCommand.GetString(3).Should().Be("Foo bar.");
        logEntryRetrievalCommand.GetString(4).Should().Contain("foo");
    }
}
