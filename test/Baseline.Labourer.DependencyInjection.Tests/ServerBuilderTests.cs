using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Labourer.Queue.Memory;
using Baseline.Labourer.Server;
using Baseline.Labourer.Store.Memory;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.DependencyInjection.Tests;

public class ServerBuilderTests : BaseDependencyInjectionTest
{
    public ServerBuilderTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    public class ServerBuilderJob : IJob
    {
        public ValueTask HandleAsync()
        {
            return new ValueTask();
        }
    }

    public class ServerBuilderMiddleware : JobMiddleware
    {
        public static bool Success;

        public override ValueTask JobStartedAsync(JobContext jobContext)
        {
            var sc = jobContext.WorkerContext.ServerContext;

            Success =
                sc.JobProcessingWorkersToRun == 5
                && sc.ScheduledJobProcessorInterval == TimeSpan.FromSeconds(30)
                && sc.DefaultRetryConfiguration.Retries == 5
                && sc.DefaultRetryConfiguration.Delays.All(d => d == TimeSpan.Zero)
                && sc.AdditionalDispatchedJobMiddlewares.First() == typeof(ServerBuilderMiddleware)
                && sc.JobRetryConfigurations[typeof(ServerBuilderJob)].Retries
                    == RetryConfiguration.Default.Retries
                && sc.JobRetryConfigurations[typeof(ServerBuilderJob)].Delays.All(
                    d => d == TimeSpan.FromSeconds((30))
                );

            return new ValueTask();
        }
    }

    [Fact]
    public async Task It_Can_Configure_The_Server_Options_Successfully()
    {
        // Arrange.
        ConfigureServices(
            (_, builder) =>
            {
                builder.UseMemoryQueue();
                builder.UseMemoryStore();
                builder.ConfigureServer(
                    serverBuilder =>
                    {
                        serverBuilder.RunThisManyJobProcessingWorkers(5);
                        serverBuilder.WaitThisLongBetweenCheckingForScheduledJobs(
                            TimeSpan.FromSeconds(30)
                        );
                        serverBuilder.SetRetryConfigurationForAllJobsToBe(
                            new RetryConfiguration(5, TimeSpan.Zero)
                        );
                        serverBuilder.AddRetryConfigurationForJobType<ServerBuilderJob>(
                            RetryConfiguration.Default
                        );
                        serverBuilder.AddDispatchedJobMiddlewareOfType<ServerBuilderMiddleware>();
                    }
                );
            }
        );

        RunServer();

        // Act.
        await Client.DispatchJobAsync<ServerBuilderJob>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                ServerBuilderMiddleware.Success.Should().BeTrue();
            }
        );
    }
}
