using System;
using System.Threading.Tasks;
using Baseline.Labourer.Queue.Sqlite;
using Microsoft.Data.Sqlite;

namespace Baseline.Labourer.Tests.Scenarios.Internal.Wrappers;

public class SqliteQueueWrapper : QueueWrapper
{
    private readonly SqliteConnection _connection;

    public SqliteQueueWrapper(Guid uniqueTestId) : base(uniqueTestId)
    {
        var connectionString = $"Data Source={uniqueTestId};";

        _connection = new SqliteConnection(connectionString);
        Queue = new SqliteQueue(connectionString);
    }

    public override async ValueTask BootstrapAsync()
    {
        await Queue.BootstrapAsync();
    }

    public override ValueTask DisposeAsync()
    {
        _connection.Dispose();

        return ValueTask.CompletedTask;
    }
}
