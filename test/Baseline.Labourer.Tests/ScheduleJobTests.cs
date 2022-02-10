﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Exceptions;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Internal.Utils;
using Baseline.Labourer.Store.Memory;
using FluentAssertions;
using Xunit;

namespace Baseline.Labourer.Tests
{
    public class ScheduleJobTests : ClientTest
    {
        [Fact]
        public async Task It_Successfully_Creates_The_Scheduled_Job_Record()
        {
            // Arrange.
            var cronExpression = "* * * * *";
            
            // Act.
            var scheduledJobId = await Client.CreateOrUpdateScheduledJobAsync<BasicJob>("created-job", cronExpression);

            // Assert.
            scheduledJobId.Should().Be("scheduled-job:created-job");
            TestBackingStore.AssertScheduledJobExists(scheduledJobId, cronExpression);
        }

        public class TestScheduledJob : IJob
        {
            public ValueTask HandleAsync(CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }
        }

        public class TestScheduledParameters
        {
            public string Name { get; } = "foo";
        }
        
        public class TestScheduledJobWithParameters : IJob<TestScheduledParameters>
        {
            public Task HandleAsync(TestScheduledParameters parameters, CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }
        }

        [Fact]
        public async Task It_Can_Update_A_Scheduled_Job_Record()
        {
            // Arrange.
            var scheduledJobId = await Client.CreateOrUpdateScheduledJobAsync<TestScheduledJob>(
                "update-scheduled-job", 
                "* * * * *"
            );
            
            // Act.
            await Client.CreateOrUpdateScheduledJobAsync<TestScheduledParameters, TestScheduledJobWithParameters>(
                scheduledJobId,
                "0 * * * *",
                new TestScheduledParameters()
            );

            // Assert.
            TestBackingStore.AssertScheduledJobExists(
                scheduledJobId,
                "0 * * * *",
                typeof(TestScheduledJobWithParameters).AssemblyQualifiedName,
                typeof(TestScheduledParameters).AssemblyQualifiedName,
                "{\"Name\":\"foo\"}"
            );
        }

        [Fact]
        public async Task It_Cannot_Update_A_Scheduled_Job_If_There_Is_Already_An_Active_Lock_Established()
        {
            // Arrange.
            var scheduledJobId = await Client.CreateOrUpdateScheduledJobAsync<BasicJob>(
                "update-with-lock", 
                "* * * * *",
                CancellationToken.None
            );
            
            TestBackingStore.Locks[scheduledJobId].Add(new MemoryLock
            {
                Id = StringGenerationUtils.GenerateUniqueRandomString(),
                Until = DateTime.Today.AddDays(1)
            });
            
            // Act.
            Func<Task> func = async () => await Client.CreateOrUpdateScheduledJobAsync<BasicJob>(
                scheduledJobId, 
                "* * * * *",
                CancellationToken.None
            );

            // Assert.
            await func.Should().ThrowExactlyAsync<ResourceLockedException>();
        }

        [Fact]
        public async Task It_Can_Delete_A_Scheduled_Job()
        {
            // Arrange.
            var scheduledJobId = await Client.CreateOrUpdateScheduledJobAsync<TestScheduledJob>(
                "update-scheduled-job", 
                "* * * * *"
            );
            
            // Act.
            await Client.DeleteScheduledJobAsync(scheduledJobId);

            // Assert.
            TestBackingStore.AssertScheduledJobDoesNotExist(scheduledJobId);
        }

        [Fact]
        public async Task It_Cannot_Delete_A_Scheduled_Job_If_There_Is_An_Active_Lock_Already_Established()
        {
            // Arrange.
            var scheduledJob = new ScheduledJobDefinition {Name = "active-lock-established"};
            TestBackingStore.ScheduledJobs.Add(scheduledJob.Id, scheduledJob);
            TestBackingStore.Locks.Add(
                scheduledJob.Id, 
                new List<MemoryLock>
                {
                    new MemoryLock
                    {
                        Id = StringGenerationUtils.GenerateUniqueRandomString(), 
                        Until = DateTime.Today.AddDays(1)
                    }
                }
            );
            
            // Act.
            Func<Task> func = async () => await Client.DeleteScheduledJobAsync(scheduledJob.Id, CancellationToken.None);

            // Assert.
            await func.Should().ThrowExactlyAsync<ResourceLockedException>();
        }

        [Fact]
        public async Task It_Can_Delete_A_Scheduled_Job_If_There_Is_An_Outdated_Lock()
        {
            // Arrange.
            var scheduledJob = new ScheduledJobDefinition {Name = "outdated-lock"};
            TestBackingStore.ScheduledJobs.Add(scheduledJob.Id, scheduledJob);
            TestBackingStore.Locks.Add(
                scheduledJob.Id, 
                new List<MemoryLock>
                {
                    new MemoryLock
                    {
                        Id = StringGenerationUtils.GenerateUniqueRandomString(), 
                        Until = DateTime.Today.AddDays(-1)
                    }
                }
            );
            
            // Act.
            Func<Task> func = async () => await Client.DeleteScheduledJobAsync(scheduledJob.Id, CancellationToken.None);

            // Assert.
            await func.Should().NotThrowAsync();
        }
    }
}