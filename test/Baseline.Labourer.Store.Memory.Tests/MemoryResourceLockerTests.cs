using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Tests;
using FluentAssertions;
using Xunit;

namespace Baseline.Labourer.Store.Memory.Tests;

public class MemoryResourceLockerTests
{
    private readonly TestMemoryStoreDataContainer _memoryStoreDataContainer =
        new TestMemoryStoreDataContainer();
    private readonly TestDateTimeProvider _dateTimeProvider = new TestDateTimeProvider();
    private readonly TestMemoryResourceLocker _memoryResourceLocker;

    public MemoryResourceLockerTests()
    {
        _memoryResourceLocker = new TestMemoryResourceLocker(
            _memoryStoreDataContainer,
            _dateTimeProvider
        );
    }

    [Fact]
    public async Task It_Removes_The_Lock_When_The_Disposable_Goes_Out_Of_Scope()
    {
        // Act.
        await using (
            var _ = await _memoryResourceLocker.LockResourceAsync(
                "abc",
                TimeSpan.FromSeconds(100),
                CancellationToken.None
            )
        )
        {
            _memoryStoreDataContainer.Locks["abc"].Should().ContainSingle(l => l.Released == null);
        }

        // Assert.
        _memoryStoreDataContainer.Locks["abc"].Should().Contain(l => l.Released != null);
    }

    [Fact]
    public async Task It_Throws_An_Exception_If_The_Resource_Is_Already_Locked()
    {
        // Arrange.
        _memoryStoreDataContainer.Locks["abc"] = new[]
        {
            new MemoryLock { Until = DateTime.UtcNow.AddDays(1) }
        }.ToList();

        // Act.
        Func<Task> sut = async () =>
            await _memoryResourceLocker.LockResourceAsync(
                "abc",
                TimeSpan.FromSeconds(1),
                CancellationToken.None
            );

        // Assert.
        await sut.Should().ThrowExactlyAsync<ResourceLockedException>();
    }

    [Fact]
    public async Task It_Does_Not_Throw_An_Exception_If_The_Lock_Has_Expired()
    {
        // Arrange.
        _memoryStoreDataContainer.Locks["abc"] = new[]
        {
            new MemoryLock { Until = DateTime.UtcNow.AddDays(1) }
        }.ToList();

        _dateTimeProvider.SetUtcNow(DateTime.UtcNow.AddMonths(1));

        // Act.
        Func<Task> sut = async () =>
            await _memoryResourceLocker.LockResourceAsync(
                "abc",
                TimeSpan.FromSeconds(1),
                CancellationToken.None
            );

        // Assert.
        await sut.Should().NotThrowAsync<ResourceLockedException>();
    }

    [Fact]
    public async Task It_Does_Not_Throw_An_Exception_If_The_Lock_Has_Been_Released()
    {
        // Arrange.
        _memoryStoreDataContainer.Locks["abc"] = new[]
        {
            new MemoryLock
            {
                Until = DateTime.UtcNow.AddDays(1),
                Released = DateTime.UtcNow.AddDays(1).AddMinutes(10)
            }
        }.ToList();

        // Act.
        Func<Task> sut = async () =>
            await _memoryResourceLocker.LockResourceAsync(
                "abc",
                TimeSpan.FromSeconds(1),
                CancellationToken.None
            );

        // Assert.
        await sut.Should().NotThrowAsync<ResourceLockedException>();
    }
}
