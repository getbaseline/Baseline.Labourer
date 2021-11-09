using Baseline.Labourer.Store.Memory;
using FluentAssertions;

namespace Baseline.Labourer.Tests;

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

    public void AssertJobHasRetryCount(string jobId, int retryCount)
    {
        DispatchedJobs.First(j => j.Id == jobId).Retries.Should().Be(retryCount);
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

    public void AssertHeartbeatRegisteredForServer(string server)
    {
        ServerHeartbeats.ContainsKey(server).Should().BeTrue();
        ServerHeartbeats[server].Count.Should().BeGreaterThanOrEqualTo(1);
    }
}
