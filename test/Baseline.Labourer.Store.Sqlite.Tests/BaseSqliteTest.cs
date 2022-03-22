using System;
using Microsoft.Data.Sqlite;

namespace Baseline.Labourer.Store.Sqlite.Tests;

public abstract class BaseSqliteTest : IDisposable
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
    
    public void Dispose()
    {
        Connection.Dispose();
    }
}