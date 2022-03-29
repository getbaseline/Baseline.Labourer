using Microsoft.Data.Sqlite;

namespace Baseline.Labourer.Internal;

/// <summary>
/// Internal base class used to allow different projects to bootstrap SQLite.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseSqliteBootstrapper<T> : BaseSqliteInteractor
{
    protected BaseSqliteBootstrapper(string connectionString) : base(connectionString) { }

    /// <summary>
    /// Bootstraps SQLite, running the required migrations.
    /// </summary>
    public async ValueTask BootstrapAsync()
    {
        using var connection = NewConnection();
        var transaction = connection.BeginTransaction();

        CreateMigrationsTableIfNotExists(connection, transaction);
        await MigrateAsync(connection, transaction);

        transaction.Commit();
    }

    private void CreateMigrationsTableIfNotExists(
        SqliteConnection connection,
        SqliteTransaction transaction
    )
    {
        var createCommand = new SqliteCommand(
            "CREATE TABLE IF NOT EXISTS bl_lb_version_history (migration TEXT NOT NULL);",
            connection,
            transaction
        );

        createCommand.ExecuteNonQuery();
    }

    private static async ValueTask MigrateAsync(
        SqliteConnection connection,
        SqliteTransaction transaction
    )
    {
        var assembly = typeof(T).Assembly;
        var availableMigrations = assembly
            .GetManifestResourceNames()
            .Select(r => new { Name = r, Contents = assembly.GetManifestResourceStream(r) })
            .OrderBy(r => r.Name);

        foreach (var migration in availableMigrations)
        {
            if (MigrationAlreadyRan(migration.Name, connection, transaction))
            {
                continue;
            }

            await MigrateFileAsync(migration.Name, migration.Contents!, connection, transaction);
        }
    }

    private static bool MigrationAlreadyRan(
        string name,
        SqliteConnection connection,
        SqliteTransaction transaction
    )
    {
        var alreadyRanCommand = new SqliteCommand(
            "SELECT COUNT(1) FROM bl_lb_version_history WHERE migration = @Migration",
            connection,
            transaction
        );

        alreadyRanCommand.Parameters.Add(new SqliteParameter("@Migration", name));

        return (long)alreadyRanCommand.ExecuteScalar()! > 0;
    }

    private static async Task MigrateFileAsync(
        string name,
        Stream contents,
        SqliteConnection connection,
        SqliteTransaction transaction
    )
    {
        var migrationFileContents = await new StreamReader(contents).ReadToEndAsync();

        var migrationCommand = new SqliteCommand(migrationFileContents, connection, transaction);
        await migrationCommand.ExecuteNonQueryAsync();

        var versionCommand = new SqliteCommand(
            "INSERT INTO bl_lb_version_history (migration) VALUES (@Name)",
            connection,
            transaction
        );

        versionCommand.Parameters.Add(new SqliteParameter("@Name", name));

        versionCommand.ExecuteNonQuery();
    }
}
