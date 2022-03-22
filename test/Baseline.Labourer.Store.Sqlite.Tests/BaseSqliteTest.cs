using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Baseline.Labourer.Store.Sqlite.Tests;

public abstract class BaseSqliteTest : IAsyncLifetime
{
    private readonly Guid _databaseId = Guid.NewGuid();
    protected readonly string ConnectionString;
    protected readonly SqliteConnection Connection;

    protected BaseSqliteTest()
    {
        ConnectionString = $"Data Source={_databaseId};Mode=Memory;Cache=Shared";
        Connection = new SqliteConnection(ConnectionString);
        Connection.Open();
    }

    public Task DisposeAsync()
    {
        Connection.Dispose();
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await new SqliteStore(ConnectionString).BootstrapAsync();
    }
}
