﻿using System;
using System.Collections.Generic;
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

            public override ValueTask JobFailedAsync(JobContext jobContext, Exception? exception, CancellationToken cancellationToken)
            {
                JobFailed = true;
                return new ValueTask();
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
            Task.Run(
                async () => await new JobProcessorWorker
                        .JobProcessorWorker(
                            GenerateServerContextAsync(
                                s => s.AdditionalDispatchedJobMiddlewares = new List<IJobMiddleware>
                                {
                                    new TestJobMiddleware()
                                }
                            )
                        )
                        .RunAsync()
            );
        }

        [Fact]
        public async Task It_Dispatches_User_Defined_JobStarted_And_JobCompleted_Job_Middlewares()
        {
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
        public async Task It_Dispatches_User_Defines_JobFailure_Job_Middlewares()
        {
            // Act.
            await Client.DispatchJobAsync<JobThatWillFail>();
            
            // Assert.
            await AssertionUtils.RetryAsync(() =>
            {
                TestJobMiddleware.JobFailed.Should().BeTrue();
                TestJobMiddleware.JobFailedAndExceededRetries.Should().BeTrue();
            });
        }
    }
}