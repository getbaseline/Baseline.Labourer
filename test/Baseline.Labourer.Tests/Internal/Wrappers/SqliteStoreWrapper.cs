using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Store.Sqlite;
using Microsoft.Data.Sqlite;

namespace Baseline.Labourer.Tests.Internal.Wrappers;

public class SqliteStoreWrapper : StoreWrapper
{
    private readonly SqliteConnection _connection;

    public SqliteStoreWrapper(Guid uniqueTestId) : base(uniqueTestId)
    {
        var connectionString = $"Data Source={uniqueTestId};";

        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        Store = new SqliteStore(connectionString);
    }

    public override async ValueTask BootstrapAsync()
    {
        await Store.BootstrapAsync();
    }

    public override ValueTask DisposeAsync()
    {
        _connection.Dispose();

        return ValueTask.CompletedTask;
    }

    public override ValueTask<IReadOnlyCollection<ServerInstance>> GetRegisteredServersAsync()
    {
        var result = new List<ServerInstance>();

        var registeredServersQuery = new SqliteCommand(
            @"
                SELECT hostname, key
                FROM bl_lb_servers
            ",
            _connection
        );
        using var reader = registeredServersQuery.ExecuteReader();
        while (reader.Read())
        {
            result.Add(
                new ServerInstance { Hostname = reader.GetString(0), Key = reader.GetString(1) }
            );
        }

        return ValueTask.FromResult((IReadOnlyCollection<ServerInstance>)result);
    }

    public override ValueTask<IReadOnlyCollection<Worker>> GetRegisteredWorkersAsync()
    {
        var result = new List<Worker>();

        var registeredWorkersQuery = new SqliteCommand(
            @"
                SELECT id, server_id
                FROM bl_lb_workers
            ",
            _connection
        );
        using var reader = registeredWorkersQuery.ExecuteReader();
        while (reader.Read())
        {
            result.Add(
                new Worker { Id = reader.GetString(0), ServerInstanceId = reader.GetString(1) }
            );
        }

        return ValueTask.FromResult((IReadOnlyCollection<Worker>)result);
    }
}
