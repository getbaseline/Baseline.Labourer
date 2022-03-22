using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Baseline.Labourer.Store.Sqlite.Tests;

public class SqliteStoreTests : IDisposable
{
    private readonly Guid _databaseId = Guid.NewGuid();
    private readonly string _connectionString;
    private readonly SqliteConnection _connection;

    public SqliteStoreTests()
    {
        _connectionString = $"Data Source={_databaseId};Mode=Memory;Cache=Shared";
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
        
    [Fact]
    public async Task It_Creates_The_Relevant_Tables_And_Migrates_The_Migrations()
    {
        // Arrange.
        var store = new SqliteStore(_connectionString);

        // Act.
        await store.BootstrapAsync();
        
        // Assert.
        var entriesInVersionTable = (long)new SqliteCommand("SELECT count(1) FROM bl_lb_version_history", _connection)
            .ExecuteScalar()!;
        
        entriesInVersionTable.Should().Be(1);
    }
        
    [Fact]
    public async Task It_Does_Not_Run_The_Same_Migrations_Twice()
    {
        // Arrange.
        var store = new SqliteStore(_connectionString);

        // Act.
        await store.BootstrapAsync();
        await store.BootstrapAsync();
        
        // Assert.
        var entriesInVersionTable = (long)new SqliteCommand("SELECT count(1) FROM bl_lb_version_history", _connection)
            .ExecuteScalar()!;
        
        entriesInVersionTable.Should().Be(1);
    }
}