using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.DependencyInjection.Tests;

public class NoOpTests : BaseDependencyInjectionTest
{
    public class NoOpJob : IJob
    {
        public ValueTask HandleAsync(CancellationToken cancellationToken)
        {
            return new ValueTask();
        }
    }

    public NoOpTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [Fact]
    public async Task It_Can_Create_And_Run_Baseline_By_Using_The_NoOp_Queue_And_Store()
    {
        // Arrange.
        ConfigureServices(
            (_, builder) =>
            {
                builder.UseNoOpStore();
                builder.UseNoOpQueue();
            }
        );

        // Act.
        Func<Task> sut = async () => await Client.DispatchJobAsync<NoOpJob>();

        // Assert.
        await sut.Should().NotThrowAsync();
    }
}
