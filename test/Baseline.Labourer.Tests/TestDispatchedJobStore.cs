using System;
using System.Linq;
using Baseline.Labourer.Store.Memory;
using FluentAssertions;

namespace Baseline.Labourer.Tests
{
    public class TestDispatchedJobStore : MemoryJobStore
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
        
        public DispatchedJobDefinition AssertJobWithTypesStored(Type jobType, Type parametersType)
        {
            var jobDefinition = DispatchedJobs.FirstOrDefault(
                j => j.Type == jobType.AssemblyQualifiedName &&
                     j.ParametersType == parametersType.AssemblyQualifiedName
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

        public void AssertJobHasRetryCount(string jobId, int retryCount)
        {
            DispatchedJobs.First(j => j.Id == jobId).Retries.Should().Be(retryCount);
        }
    }
}