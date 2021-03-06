using System.Threading.Tasks;
using Baseline.Labourer.Queue.Memory;
using Baseline.Labourer.Store.Memory;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.DependencyInjection.Tests;

public class MemoryTests : BaseDependencyInjectionTest
{
    public class Job : IJob
    {
        public static bool Ran;

        public Job() { }

        public ValueTask HandleAsync()
        {
            Ran = true;
            return new ValueTask();
        }
    }

    public MemoryTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [Fact]
    public async Task It_Can_Create_And_Run_Baseline_By_Using_The_NoOp_Queue_And_Store()
    {
        // Arrange.
        ConfigureServices(
            (sp, builder) =>
            {
                builder.UseLoggerFactoryResolvedFromContainer(sp);
                builder.UseMemoryStore();
                builder.UseMemoryQueue();
            }
        );

        RunServer();

        // Act.
        await Client.DispatchJobAsync<Job>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                Job.Ran.Should().BeTrue();
            }
        );
    }
}
