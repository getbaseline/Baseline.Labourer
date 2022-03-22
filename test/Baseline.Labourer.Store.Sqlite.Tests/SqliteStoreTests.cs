using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Baseline.Labourer.Store.Sqlite.Tests;

public class SqliteStoreTests : BaseSqliteTest
{
    [Fact]
    public async Task It_Creates_The_Relevant_Tables_And_Migrates_The_Migrations()
    {
        // Arrange.
        var store = new SqliteStore(ConnectionString);

        // Act.
        await store.BootstrapAsync();
        
        // Assert.
        var entriesInVersionTable = (long)new SqliteCommand("SELECT count(1) FROM bl_lb_version_history", Connection)
            .ExecuteScalar()!;
        
        entriesInVersionTable.Should().Be(1);
    }
        
    [Fact]
    public async Task It_Does_Not_Run_The_Same_Migrations_Twice()
    {
        // Arrange.
        var store = new SqliteStore(ConnectionString);

        // Act.
        await store.BootstrapAsync();
        await store.BootstrapAsync();
        
        // Assert.
        var entriesInVersionTable = (long)new SqliteCommand("SELECT count(1) FROM bl_lb_version_history", Connection)
            .ExecuteScalar()!;
        
        entriesInVersionTable.Should().Be(1);
    }
}