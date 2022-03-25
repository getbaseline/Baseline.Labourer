using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Microsoft.Data.Sqlite;

namespace Baseline.Labourer;

/// <summary>
/// An <see cref="IStore"/> implementation that persists its data in a SQLite database.
/// </summary>
public class SqliteStore : BaseSqliteInteractor, IStore
{
    /// <inheritdoc />
    public IJobLogStore JobLogStore { get; }

    /// <inheritdoc />
    public IResourceLocker ResourceLocker { get; }

    /// <inheritdoc />
    public IStoreReader Reader { get; }

    /// <inheritdoc />
    public IStoreWriterTransactionManager WriterTransactionManager { get; }

    public SqliteStore(IDateTimeProvider dateTimeProvider, string connectionString)
        : base(connectionString)
    {
        JobLogStore = new SqliteJobLogStore(connectionString);
        ResourceLocker = new SqliteResourceLocker(dateTimeProvider, connectionString);
        Reader = new SqliteReader(connectionString);
        WriterTransactionManager = new SqliteStoreWriterTransactionManager(
            dateTimeProvider,
            connectionString
        );
    }

    /// <inheritdoc />
    public async ValueTask BootstrapAsync()
    {
        await using var connection = NewConnection();
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
        var assembly = typeof(SqliteStore).Assembly;
        var availableMigrations = assembly
            .GetManifestResourceNames()
            .Select(r => new { Name = r, Contents = assembly.GetManifestResourceStream(r) });

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
