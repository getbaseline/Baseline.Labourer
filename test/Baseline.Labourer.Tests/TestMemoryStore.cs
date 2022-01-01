using System;
using System.Linq;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Store.Memory;
using FluentAssertions;

namespace Baseline.Labourer.Tests
{
    public class TestMemoryStore : MemoryStore
    {
        public void AssertJobHasFinishedAtValueWithin5SecondsOf(string jobId, DateTime closeToValue)
        {
            var job = DispatchedJobs.FirstOrDefault(j => j.Id == jobId);
            job.Should().NotBeNull();
            job.FinishedAt!.Should().BeCloseTo(closeToValue, TimeSpan.FromSeconds(5));
        }

        public void AssertStatusForJobIs(string jobId, JobStatus status)
        {
            var job = DispatchedJobs.FirstOrDefault(j => j.Id == jobId);
            job.Should().NotBeNull();
            job!.Status.Should().Be(status);
        }

        public DispatchedJobDefinition AssertJobWithTypesStored(Type jobType, Type parametersType = null)
        {
            var jobDefinition = DispatchedJobs.FirstOrDefault(
                j => j.Type == jobType.AssemblyQualifiedName &&
                     (parametersType == null || j.ParametersType == parametersType.AssemblyQualifiedName)
            );

            if (jobDefinition == null)
            {
                throw new Exception(
                    $"No jobs found with job type of {jobType.AssemblyQualifiedName} and parameter type of {parametersType.AssemblyQualifiedName}."
                );
            }

            return jobDefinition;
        }

        public void AssertMessageForJobLogged(string jobId, string message)
        {
            LogEntries.Should().Contain(l => l.JobId == jobId && l.Message == message);
        }

        public void AssertJobHasRetryCount(string jobId, uint retryCount)
        {
            DispatchedJobs.Should().ContainSingle(j => j.Id == jobId && j.Retries == retryCount);
        }

        public string AssertHasRegisteredAServer()
        {
            Servers.Should().HaveCountGreaterOrEqualTo(1);
            return Servers.First().Id;
        }

        public void AssertHasRegisteredWorkersForServer(string serverId, int? count = null)
        {
            ServerWorkers.ContainsKey(serverId).Should().BeTrue();

            if (count != null)
            {
                ServerWorkers[serverId].Count.Should().Be(count);
            }
            else
            {
                ServerWorkers[serverId].Count.Should().BeGreaterThanOrEqualTo(1);
            }
        }

        public void AssertHeartbeatRegisteredForServer(string server, int count = 1)
        {
            ServerHeartbeats.ContainsKey(server).Should().BeTrue();
            ServerHeartbeats[server].Count.Should().Be(count);
        }

        public void AssertScheduledJobCreated(string id, string cronExpression = null)
        {
            ScheduledJobs.Should().ContainKey(id);

            if (cronExpression != null)
            {
                ScheduledJobs[id].CronExpression.Should().Be(cronExpression);
            }
        }

        public void AssertNextRunDateForScheduledJobIsCloseTo(string id, DateTime nextRunDate)
        {
            ScheduledJobs[id].NextRunDate.Should().BeCloseTo(nextRunDate, TimeSpan.FromSeconds(1));
        }
    }
}