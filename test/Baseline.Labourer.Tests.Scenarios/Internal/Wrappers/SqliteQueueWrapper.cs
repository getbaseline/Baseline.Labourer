using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Baseline.Labourer.Tests.Scenarios.Internal;

public class SqliteQueueWrapper : QueueWrapper
{
    private readonly SqliteConnection _connection;

    public SqliteQueueWrapper(Guid uniqueTestId) : base(uniqueTestId)
    {
        var connectionString = "Data Source={_uniqueTestId};";

        _connection = new SqliteConnection(connectionString);
        Queue = new SqliteQueue(connectionString);
    }

    protected override async ValueTask BootstrapAsync()
    {
        await Queue.BootstrapAsync();
    }

    protected override ValueTask DisposeAsync()
    {
        _connection.Dispose();
        File.Delete(UniqueTestId.ToString());

        return ValueTask.CompletedTask;
    }
}
