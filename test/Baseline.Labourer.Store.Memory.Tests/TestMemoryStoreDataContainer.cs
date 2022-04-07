using System;
using System.Linq;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Models;
using FluentAssertions;

namespace Baseline.Labourer.Store.Memory.Tests;

public class TestMemoryStoreDataContainer : MemoryStoreDataContainer
{
    public void AssertJobHasFinishedAtValueWithin5SecondsOf(string jobId, DateTime closeToValue)
    {
        var job = DispatchedJobs.FirstOrDefault(j => j.Id == jobId);
        job.Should().NotBeNull();
        job!.FinishedAt!.Should().BeCloseTo(closeToValue, TimeSpan.FromSeconds(5));
    }

    public void AssertStatusForJobIs(string jobId, JobStatus status)
    {
        var job = DispatchedJobs.FirstOrDefault(j => j.Id == jobId);
        job.Should().NotBeNull();
        job!.Status.Should().Be(status);
    }

    public DispatchedJobDefinition AssertJobWithTypesStored(
        Type jobType,
        Type? parametersType = null
    )
    {
        var jobDefinition = DispatchedJobs.FirstOrDefault(
            j =>
                j.Type == jobType.AssemblyQualifiedName
                && (
                    parametersType == null
                    || j.ParametersType == parametersType.AssemblyQualifiedName
                )
        );

        if (jobDefinition == null)
        {
            throw new Exception(
                $"No jobs found with job type of {jobType.AssemblyQualifiedName} and parameter type of {parametersType?.AssemblyQualifiedName}."
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

    public void AssertScheduledJobDoesNotExist(string id)
    {
        ScheduledJobs.Should().NotContainKey(id);
    }

    public void AssertScheduledJobExists(
        string id,
        string? cronExpression = null,
        string? type = null,
        string? parametersType = null,
        string? serializedParameters = null
    )
    {
        ScheduledJobs.Should().ContainKey(id);

        if (cronExpression != null)
        {
            ScheduledJobs[id].CronExpression.Should().Be(cronExpression);
        }

        if (type != null)
        {
            ScheduledJobs[id].Type.Should().Be(type);
        }

        if (parametersType != null)
        {
            ScheduledJobs[id].ParametersType.Should().Be(parametersType);
        }

        if (serializedParameters != null)
        {
            ScheduledJobs[id].SerializedParameters.Should().Be(serializedParameters);
        }
    }

    public void AssertNextRunDateForScheduledJobIsCloseTo(string id, DateTime nextRunDate)
    {
        ScheduledJobs[id].NextRunDate.Should().BeCloseTo(nextRunDate, TimeSpan.FromSeconds(1));
    }
}
