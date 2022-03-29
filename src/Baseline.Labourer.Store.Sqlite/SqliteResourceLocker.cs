using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Microsoft.Data.Sqlite;

namespace Baseline.Labourer;

/// <summary>
/// SqliteResourceLocker locks resources by entering rows in a SQLite database.
/// </summary>
public class SqliteResourceLocker : BaseSqliteInteractor, IResourceLocker
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public SqliteResourceLocker(IDateTimeProvider dateTimeProvider, string connectionString)
        : base(connectionString)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc />
    public Task<IAsyncDisposable> LockResourceAsync(
        string resource,
        TimeSpan @for,
        CancellationToken cancellationToken
    )
    {
        using var connection = NewConnection();
        using var transaction = connection.BeginTransaction();

        if (LockAlreadyExists(connection, transaction, resource))
        {
            throw new ResourceLockedException(resource);
        }

        var lockId = CreateLock(connection, transaction, resource, @for);

        transaction.Commit();

        return Task.FromResult(
            (IAsyncDisposable)new AsyncComposableDisposable(() => ReleaseLock(lockId))
        );
    }

    private bool LockAlreadyExists(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string resource
    )
    {
        var lockAlreadyExistsCommand = new SqliteCommand(
            @"
                SELECT 
                    COUNT(1) 
                FROM 
                    bl_lb_locks 
                WHERE 
                    resource = @Resource AND 
                    released_at IS NULL AND 
                    until >= @Now
            ",
            connection,
            transaction
        );

        lockAlreadyExistsCommand.Parameters.Add(new SqliteParameter("@Resource", resource));
        lockAlreadyExistsCommand.Parameters.Add(
            new SqliteParameter("@Now", _dateTimeProvider.UtcNow())
        );

        return (long)lockAlreadyExistsCommand.ExecuteScalar()! > 0;
    }

    private long CreateLock(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string resource,
        TimeSpan @for
    )
    {
        var createLockCommand = new SqliteCommand(
            @"
                INSERT INTO bl_lb_locks (resource, until, created_at, updated_at)
                VALUES (@Resource, @Until, @CreatedAt, @UpdatedAt);
            ",
            connection,
            transaction
        );
        createLockCommand.Parameters.Add(new SqliteParameter("@Resource", resource));
        createLockCommand.Parameters.Add(
            new SqliteParameter("@Until", _dateTimeProvider.UtcNow().Add(@for))
        );
        createLockCommand.Parameters.Add(
            new SqliteParameter("@CreatedAt", _dateTimeProvider.UtcNow())
        );
        createLockCommand.Parameters.Add(
            new SqliteParameter("@UpdatedAt", _dateTimeProvider.UtcNow())
        );
        createLockCommand.ExecuteNonQuery();

        var lastIdCommand = new SqliteCommand(
            @"SELECT last_insert_rowid()",
            connection,
            transaction
        );
        return (long)lastIdCommand.ExecuteScalar()!;
    }

    private ValueTask ReleaseLock(long lockId)
    {
        using var connection = NewConnection();

        var releaseLockCommand = new SqliteCommand(
            @"UPDATE bl_lb_locks SET released_at = @Now WHERE id = @LockId",
            connection
        );
        releaseLockCommand.Parameters.Add(new SqliteParameter("@LockId", lockId));
        releaseLockCommand.Parameters.Add(new SqliteParameter("@Now", _dateTimeProvider.UtcNow()));
        releaseLockCommand.ExecuteNonQuery();

        return ValueTask.CompletedTask;
    }
}
