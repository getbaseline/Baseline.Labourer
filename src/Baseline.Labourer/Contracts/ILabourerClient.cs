using System.Threading.Tasks;

namespace Baseline.Labourer;

/// <summary>
/// A contract that provides the consumer level API for all Baseline.Labourer job management functions
/// (such as dispatching jobs and registering recurrent ones).
/// </summary>
public interface ILabourerClient
{
    /// <summary>
    /// Creates or updates a scheduled job (i.e. one that is in the future and can optionally recur).
    /// </summary>
    /// <param name="nameOrId">The name or id of the scheduled job.</param>
    /// <param name="cronExpression">
    /// A cron expression used to define when the scheduled job will run and if it will repeat.
    /// </param>
    Task<string> CreateOrUpdateScheduledJobAsync<TJob>(string nameOrId, string cronExpression)
        where TJob : IJob;

    /// <summary>
    /// Creates or updates a scheduled job (i.e. one that is in the future and can optionally recur).
    /// </summary>
    /// <param name="nameOrId">The name or id of the scheduled job.</param>
    /// <param name="cronExpression">
    /// A cron expression used to define when the scheduled job will run and if it will repeat.
    /// </param>
    /// <param name="jobParameters">
    /// The parameters for the job, serialized to be stored and then deserialized to become the parameter for
    /// the HandleAsync method of the job.
    /// </param>
    Task<string> CreateOrUpdateScheduledJobAsync<TParams, TJob>(
        string nameOrId,
        string cronExpression,
        TParams jobParameters
    )
        where TJob : IJob<TParams>
        where TParams : class;

    /// <summary>
    /// Deletes a scheduled job.
    /// </summary>
    /// <param name="nameOrId">The name or id of the scheduled job.</param>
    Task DeleteScheduledJobAsync(string nameOrId);

    /// <summary>
    /// Dispatches a job without parameters to run immediately and returns the created id of the job.
    /// </summary>
    Task<string> DispatchJobAsync<TJob>() where TJob : IJob;

    /// <summary>
    /// Dispatches a job with parameters to run immediately and returns the created id of the job.
    /// </summary>
    /// <param name="jobParameters">
    /// The parameters for the job, serialized to be stored and then deserialized to become the parameter for
    /// the HandleAsync method of the job.
    /// </param>
    Task<string> DispatchJobAsync<TParams, TJob>(TParams jobParameters)
        where TJob : IJob<TParams>
        where TParams : class;
}
