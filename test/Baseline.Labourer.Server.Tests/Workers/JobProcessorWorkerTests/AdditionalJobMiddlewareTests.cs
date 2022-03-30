using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Labourer.Tests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests.Workers.JobProcessorWorkerTests;

public class AdditionalJobMiddlewareTests : ServerTest
{
    public class TestJobMiddleware : JobMiddleware
    {
        public static bool JobCompleted;
        public static bool JobFailed;
        public static bool JobStarted;
        public static bool JobFailedAndExceededRetries;

        public override ValueTask JobCompletedAsync(JobContext jobContext)
        {
            JobCompleted = true;
            return new ValueTask();
        }

        public override ValueTask<MiddlewareContinuation> JobFailedAsync(
            JobContext jobContext,
            Exception? exception
        )
        {
            JobFailed = true;
            return new ValueTask<MiddlewareContinuation>(MiddlewareContinuation.Continue);
        }

        public override ValueTask JobFailedAndExceededRetriesAsync(
            JobContext jobContext,
            Exception? exception
        )
        {
            JobFailedAndExceededRetries = true;
            return new ValueTask();
        }

        public override ValueTask JobStartedAsync(JobContext jobContext)
        {
            JobStarted = true;
            return new ValueTask();
        }
    }

    public class SimpleQueuedJob : IJob
    {
        public ValueTask HandleAsync()
        {
            return new ValueTask();
        }
    }

    public class JobThatWillFail : IJob
    {
        public ValueTask HandleAsync()
        {
            throw new NotImplementedException();
        }
    }

    public AdditionalJobMiddlewareTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    { }

    [Fact]
    public async Task It_Dispatches_User_Defined_JobStarted_And_JobCompleted_Job_Middlewares()
    {
        // Arrange.
        RunWorker(typeof(TestJobMiddleware));

        // Act.
        await Client.DispatchJobAsync<SimpleQueuedJob>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                TestJobMiddleware.JobStarted.Should().BeTrue();
                TestJobMiddleware.JobCompleted.Should().BeTrue();
            }
        );
    }

    [Fact]
    public async Task It_Dispatches_User_Defined_JobFailure_Job_Middlewares()
    {
        // Arrange.
        RunWorker(typeof(TestJobMiddleware));

        // Act.
        await Client.DispatchJobAsync<JobThatWillFail>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                TestJobMiddleware.JobFailed.Should().BeTrue();
                TestJobMiddleware.JobFailedAndExceededRetries.Should().BeTrue();
            }
        );
    }

    public class NoContinueMiddleware : JobMiddleware
    {
        public static bool Ran;

        public override ValueTask<MiddlewareContinuation> JobFailedAsync(
            JobContext jobContext,
            Exception? exception
        )
        {
            Ran = true;
            return new ValueTask<MiddlewareContinuation>(MiddlewareContinuation.Abort);
        }
    }

    public class AfterContinueMiddleware : JobMiddleware
    {
        public static bool Ran;

        public override ValueTask<MiddlewareContinuation> JobFailedAsync(
            JobContext jobContext,
            Exception? exception
        )
        {
            Ran = true;
            return new ValueTask<MiddlewareContinuation>(MiddlewareContinuation.Continue);
        }
    }

    [Fact]
    public async Task It_Does_Not_Run_Any_Further_Middleware_If_A_Middleware_Says_Not_To_On_JobFailure()
    {
        // Arrange.
        RunWorker(typeof(NoContinueMiddleware), typeof(AfterContinueMiddleware));

        // Act.
        await Client.DispatchJobAsync<JobThatWillFail>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                NoContinueMiddleware.Ran.Should().BeTrue();
                AfterContinueMiddleware.Ran.Should().BeFalse();
            }
        );
    }

    public class NoContinueOnErrorMiddleware : JobMiddleware
    {
        public static bool Ran;

        public override bool ContinueExecutingMiddlewaresOnFailure { get; } = false;

        public override ValueTask<MiddlewareContinuation> JobFailedAsync(
            JobContext jobContext,
            Exception? exception
        )
        {
            Ran = true;
            throw new ArgumentException("Failure here.");
        }
    }

    [Fact]
    public async Task It_Does_Not_Run_Any_Further_Middleware_If_A_Middleware_Fails_And_It_Says_Not_To_Continue_On_Failure()
    {
        // Arrange.
        RunWorker(typeof(NoContinueOnErrorMiddleware), typeof(AfterContinueMiddleware));

        // Act.
        await Client.DispatchJobAsync<JobThatWillFail>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                NoContinueOnErrorMiddleware.Ran.Should().BeTrue();
                AfterContinueMiddleware.Ran.Should().BeFalse();
            }
        );
    }

    private void RunWorker(params Type[] jobMiddlewares)
    {
        Task.Run(
            async () =>
                await new LabourerServer(
                    GenerateServerConfiguration(
                        s => s.DispatchedJobMiddlewares = jobMiddlewares.ToList()
                    )
                ).RunServerAsync()
        );
    }
}
