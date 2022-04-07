using System;
using System.Threading.Tasks;
using Baseline.Labourer.Server;
using Baseline.Labourer.Tests.Configurations;
using Baseline.Labourer.Tests.Internal;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Tests;

public class JobMiddlewares : BaseTest
{
    public JobMiddlewares(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <summary>
    /// Tests that all relevant middleware methods are executed throughout the different execution statuses
    /// (i.e. started, completed, failed) of a job.
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task RunsTheRelevantMiddlewareMethodsThroughoutTheExecutionStatusesOfAJob(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        RunsTheRelevantMiddlewareMethodsThroughoutTheExecutionStatusesOfAJobMiddleware.Reset();

        await BootstrapAsync(
            queueProvider,
            storeProvider,
            server =>
            {
                server.DefaultRetryConfiguration = RetryConfiguration.None;
                server.DispatchedJobMiddlewares = new[]
                {
                    typeof(RunsTheRelevantMiddlewareMethodsThroughoutTheExecutionStatusesOfAJobMiddleware)
                };
            }
        );

        // Act.
        await Client.DispatchJobAsync<SuccessJob>();
        await Client.DispatchJobAsync<FailureJob>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                RunsTheRelevantMiddlewareMethodsThroughoutTheExecutionStatusesOfAJobMiddleware.Started
                    .Should()
                    .BeTrue();

                RunsTheRelevantMiddlewareMethodsThroughoutTheExecutionStatusesOfAJobMiddleware.Completed
                    .Should()
                    .BeTrue();

                RunsTheRelevantMiddlewareMethodsThroughoutTheExecutionStatusesOfAJobMiddleware.Failed
                    .Should()
                    .BeTrue();

                RunsTheRelevantMiddlewareMethodsThroughoutTheExecutionStatusesOfAJobMiddleware.CatastrophicallyFailed
                    .Should()
                    .BeTrue();
            }
        );
    }

    /// <summary>
    /// Tests that when a failure middleware says to abort any further middleware executions, it's listened to!
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToStopContinuingExecution(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToStopContinuingExecution_FirstMiddleware.Executed =
            false;

        DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToStopContinuingExecution_SecondMiddleware.Executed =
            false;

        await BootstrapAsync(
            queueProvider,
            storeProvider,
            server =>
            {
                server.DispatchedJobMiddlewares = new[]
                {
                    typeof(DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToStopContinuingExecution_FirstMiddleware),
                    typeof(DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToStopContinuingExecution_SecondMiddleware)
                };
            }
        );

        // Act.
        await Client.DispatchJobAsync<FailureJob>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToStopContinuingExecution_FirstMiddleware.Executed
                    .Should()
                    .BeTrue();
            }
        );

        await AssertionUtils.EnsureAsync(
            () =>
            {
                DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToStopContinuingExecution_SecondMiddleware.Executed
                    .Should()
                    .BeFalse();
            }
        );
    }

    /// <summary>
    /// Tests that the configuration property saying to abort any further middleware executions when a middleware throws
    /// an unhandled exception is listened to.
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToAbortOnFailure(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToAbortOnFailure_FirstMiddleware.Executed =
            false;

        DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToAbortOnFailure_SecondMiddleware.Executed =
            false;

        await BootstrapAsync(
            queueProvider,
            storeProvider,
            server =>
            {
                server.DispatchedJobMiddlewares = new[]
                {
                    typeof(DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToAbortOnFailure_FirstMiddleware),
                    typeof(DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToAbortOnFailure_SecondMiddleware),
                };
            }
        );

        // Act.
        await Client.DispatchJobAsync<SuccessJob>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToAbortOnFailure_FirstMiddleware.Executed
                    .Should()
                    .BeTrue();
            }
        );

        await AssertionUtils.EnsureAsync(
            () =>
            {
                DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToAbortOnFailure_SecondMiddleware.Executed
                    .Should()
                    .BeFalse();
            }
        );
    }

    #region Test Dependencies
    public class SuccessJob : IJob
    {
        public ValueTask HandleAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    public class FailureJob : IJob
    {
        public ValueTask HandleAsync()
        {
            throw new System.NotImplementedException();
        }
    }

    public class RunsTheRelevantMiddlewareMethodsThroughoutTheExecutionStatusesOfAJobMiddleware
        : JobMiddleware
    {
        public static bool Started;
        public static bool Completed;
        public static bool Failed;
        public static bool CatastrophicallyFailed;

        public override ValueTask JobStartedAsync(JobContext jobContext)
        {
            Started = true;
            return ValueTask.CompletedTask;
        }

        public override ValueTask JobCompletedAsync(JobContext jobContext)
        {
            Completed = true;
            return ValueTask.CompletedTask;
        }

        public override ValueTask<MiddlewareContinuation> JobFailedAsync(
            JobContext jobContext,
            Exception? exception
        )
        {
            Failed = true;
            return ValueTask.FromResult(MiddlewareContinuation.Continue);
        }

        public override ValueTask JobFailedAndExceededRetriesAsync(
            JobContext jobContext,
            Exception? exception
        )
        {
            CatastrophicallyFailed = true;
            return ValueTask.CompletedTask;
        }

        public static void Reset()
        {
            Started = false;
            Completed = false;
            Failed = false;
            CatastrophicallyFailed = false;
        }
    }

    public class DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToStopContinuingExecution_FirstMiddleware
        : JobMiddleware
    {
        public static bool Executed;

        public override ValueTask<MiddlewareContinuation> JobFailedAsync(
            JobContext jobContext,
            Exception? exception
        )
        {
            Executed = true;
            return ValueTask.FromResult(MiddlewareContinuation.Abort);
        }
    }

    public class DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToStopContinuingExecution_SecondMiddleware
        : JobMiddleware
    {
        public static bool Executed;

        public override ValueTask<MiddlewareContinuation> JobFailedAsync(
            JobContext jobContext,
            Exception? exception
        )
        {
            Executed = true;
            return ValueTask.FromResult(MiddlewareContinuation.Continue);
        }
    }

    public class DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToAbortOnFailure_FirstMiddleware
        : JobMiddleware
    {
        public static bool Executed;

        public override bool ContinueExecutingMiddlewaresOnFailure => false;

        public override ValueTask JobCompletedAsync(JobContext jobContext)
        {
            Executed = true;
            throw new Exception();
        }
    }

    public class DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToAbortOnFailure_SecondMiddleware
        : JobMiddleware
    {
        public static bool Executed;

        public override ValueTask JobCompletedAsync(JobContext jobContext)
        {
            Executed = true;
            return ValueTask.CompletedTask;
        }
    }

    #endregion
}
