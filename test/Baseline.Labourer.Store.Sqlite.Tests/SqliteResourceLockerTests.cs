using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Tests;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Baseline.Labourer.Store.Sqlite.Tests;

public class SqliteResourceLockerTests : BaseSqliteTest
{
    private readonly string _resourceId = Guid.NewGuid().ToString();
    private readonly TestDateTimeProvider _testDateTimeProvider = new TestDateTimeProvider();
    private readonly SqliteResourceLocker _resourceLocker;

    public SqliteResourceLockerTests()
    {
        _resourceLocker = new SqliteResourceLocker(_testDateTimeProvider, ConnectionString);
    }

    [Fact]
    public async Task It_Throws_An_Exception_If_A_Lock_Already_Exists_For_The_Resource()
    {
        // Arrange.
        CreateLock(DateTime.UtcNow.AddDays(10));

        // Act.
        var func = async () =>
            await _resourceLocker.LockResourceAsync(
                _resourceId,
                TimeSpan.FromHours(1),
                CancellationToken.None
            );

        // Assert.
        await func.Should().ThrowExactlyAsync<ResourceLockedException>();
    }

    [Fact]
    public async Task It_Does_Not_Throw_An_Exception_If_A_Lock_Exists_For_A_Resource_But_It_Has_Expired()
    {
        // Arrange.
        CreateLock(DateTime.UtcNow.AddDays(-1));

        // Act.
        var func = async () =>
            await _resourceLocker.LockResourceAsync(
                _resourceId,
                TimeSpan.FromHours(1),
                CancellationToken.None
            );

        // Assert.
        await func.Should().NotThrowAsync();
    }

    [Fact]
    public async Task It_Does_Not_Throw_An_Exception_If_A_Lock_Exists_For_A_Resource_But_It_Has_Been_Released()
    {
        // Arrange.
        CreateLock(DateTime.UtcNow.AddDays(10), true);

        // Act.
        var func = async () =>
            await _resourceLocker.LockResourceAsync(
                _resourceId,
                TimeSpan.FromHours(1),
                CancellationToken.None
            );

        // Assert.
        await func.Should().NotThrowAsync();
    }

    [Fact]
    public async Task It_Creates_The_Lock_It_Is_Supposed_To()
    {
        // Arrange.
        _testDateTimeProvider.SetUtcNow(DateTime.Today);

        // Act.
        await _resourceLocker.LockResourceAsync(
            _resourceId,
            TimeSpan.FromMinutes(1),
            CancellationToken.None
        );

        // Assert.
        var countCommand = new SqliteCommand(
            "SELECT COUNT(1) FROM bl_lb_locks WHERE resource = @Resource AND until = @Until",
            Connection
        );
        countCommand.Parameters.Add(new SqliteParameter("@Resource", _resourceId));
        countCommand.Parameters.Add(new SqliteParameter("@Until", DateTime.Today.AddMinutes(1)));

        ((long)countCommand.ExecuteScalar()!).Should().Be(1);
    }

    [Fact]
    public async Task It_Releases_The_Lock_When_The_AsyncDisposable_Is_Disposed()
    {
        // Arrange.
        _testDateTimeProvider.SetUtcNow(DateTime.Today);

        // Act.
        var result = await _resourceLocker.LockResourceAsync(
            _resourceId,
            TimeSpan.FromMinutes(1),
            CancellationToken.None
        );
        await result.DisposeAsync();

        // Assert.
        var countCommand = new SqliteCommand(
            "SELECT released_at FROM bl_lb_locks WHERE resource = @Resource",
            Connection
        );
        countCommand.Parameters.Add(new SqliteParameter("@Resource", _resourceId));

        countCommand.ExecuteScalar()!.Should().NotBeOfType<DBNull>();
    }

    private void CreateLock(DateTime until, bool released = false)
    {
        var createLockCommand = new SqliteCommand(
            @"
                INSERT INTO bl_lb_locks (resource, until, released_at, created_at, updated_at)
                VALUES (@Resource, @Until, @ReleasedAt, @CreatedAt, @UpdatedAt);
            ",
            Connection
        );
        createLockCommand.Parameters.Add(new SqliteParameter("@Resource", _resourceId));
        createLockCommand.Parameters.Add(new SqliteParameter("@Until", until));
        createLockCommand.Parameters.Add(
            new SqliteParameter("@ReleasedAt", released ? DateTime.Now : DBNull.Value)
        );
        createLockCommand.Parameters.Add(new SqliteParameter("@CreatedAt", DateTime.UtcNow));
        createLockCommand.Parameters.Add(new SqliteParameter("@UpdatedAt", DateTime.UtcNow));

        createLockCommand.ExecuteNonQuery();
    }
}
