using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Server.Contracts;
using Baseline.Labourer.Server.JobProcessorWorker;
using Baseline.Labourer.Tests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests.Workers.JobProcessorWorkerTests
{
    public class AdditionalJobMiddlewareTests : ServerTest
    {
        public class TestJobMiddleware : JobMiddleware
        {
            public static bool JobCompleted;
            public static bool JobFailed;
            public static bool JobStarted;
            public static bool JobFailedAndExceededRetries;
        
            public override ValueTask JobCompletedAsync(JobContext jobContext, CancellationToken cancellationToken)
            {
                JobCompleted = true;
                return new ValueTask();
            }

            public override ValueTask<MiddlewareContinuation> JobFailedAsync(JobContext jobContext, Exception? exception, CancellationToken cancellationToken)
            {
                JobFailed = true;
                return new ValueTask<MiddlewareContinuation>(MiddlewareContinuation.Continue);
            }

            public override ValueTask JobFailedAndExceededRetriesAsync(JobContext jobContext, Exception? exception,
                CancellationToken cancellationToken)
            {
                JobFailedAndExceededRetries = true;
                return new ValueTask();
            }

            public override ValueTask JobStartedAsync(JobContext jobContext, CancellationToken cancellationToken)
            {
                JobStarted = true;
                return new ValueTask();
            }
        }

        public class SimpleQueuedJob : IJob
        {
            public ValueTask HandleAsync(CancellationToken cancellationToken)
            {
                return new ValueTask();
            }
        }

        public class JobThatWillFail : IJob
        {
            public ValueTask HandleAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
        
        public AdditionalJobMiddlewareTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task It_Dispatches_User_Defined_JobStarted_And_JobCompleted_Job_Middlewares()
        {
            // Arrange.
            RunWorker(typeof(TestJobMiddleware));
            
            // Act.
            await Client.DispatchJobAsync<SimpleQueuedJob>();
            
            // Assert.
            await AssertionUtils.RetryAsync(() =>
            {
                TestJobMiddleware.JobStarted.Should().BeTrue();
                TestJobMiddleware.JobCompleted.Should().BeTrue();
            });
        }

        [Fact]
        public async Task It_Dispatches_User_Defined_JobFailure_Job_Middlewares()
        {
            // Arrange.
            RunWorker(typeof(TestJobMiddleware));
            
            // Act.
            await Client.DispatchJobAsync<JobThatWillFail>();
            
            // Assert.
            await AssertionUtils.RetryAsync(() =>
            {
                TestJobMiddleware.JobFailed.Should().BeTrue();
                TestJobMiddleware.JobFailedAndExceededRetries.Should().BeTrue();
            });
        }

        public class NoContinueMiddleware : JobMiddleware
        {
            public static bool Ran;
            
            public override ValueTask<MiddlewareContinuation> JobFailedAsync(
                JobContext jobContext, 
                Exception? exception, 
                CancellationToken cancellationToken
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
                Exception? exception, 
                CancellationToken cancellationToken
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
            await AssertionUtils.RetryAsync(() =>
            {
                NoContinueMiddleware.Ran.Should().BeTrue();
                AfterContinueMiddleware.Ran.Should().BeFalse();
            });
        }

        private void RunWorker(params Type[] jobMiddlewares)
        {
            Task.Run(
                async () => await new JobProcessorWorker
                    .JobProcessorWorker(
                        GenerateServerContextAsync(s => s.AdditionalDispatchedJobMiddlewares = jobMiddlewares.ToList())
                    )
                    .RunAsync()
            );
        }
    }
}