using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Contracts;
using Baseline.Labourer.Internal.Contracts;
using Baseline.Labourer.Internal.Utils;
using NCrontab;

namespace Baseline.Labourer.Internal.Models
{
    /// <summary>
    /// Represents a job that is scheduled to run in the future and potentially recurring basis.
    /// </summary>
    public class ScheduledJobDefinition : JobDefinition
    {
        /// <summary>
        /// Gets or sets the id of the scheduled job.
        /// </summary>
        public string Id { get; } = $"scheduled-job:{StringGenerationUtils.GenerateUniqueRandomString()}";

        /// <summary>
        /// Gets or sets the cron expression used to define when the job will be executed and/or reoccur.
        /// </summary>
        public string CronExpression { get; set; }
        
        /// <summary>
        /// Gets or sets when the job last completed.
        /// </summary>
        public DateTime? LastCompletedDate { get; set; }
        
        /// <summary>
        /// Gets or sets when the job last ran.
        /// </summary>
        public DateTime? LastRunDate { get; set; }
        
        /// <summary>
        /// Gets or sets the next run date of the scheduled job.
        /// </summary>
        public DateTime NextRunDate { get; set; }

        /// <summary>
        /// Locks this recurring job in the relevant store for the timespan specified.
        /// </summary>
        /// <param name="resourceLocker">A resource locker implementation, used to actually lock this job.</param>
        /// <param name="for">The time the resource should be locked for, assuming the resource is not manually released.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async ValueTask<IAsyncDisposable> LockJobAsync(
            IResourceLocker resourceLocker,
            TimeSpan @for,
            CancellationToken cancellationToken
        )
        {
            return await resourceLocker.LockResourceAsync(Id, @for, cancellationToken);
        }

        /// <summary>
        /// Updates the last run date of the scheduled job within the store.
        /// </summary>
        /// <param name="writer">A transactional writer used to update the store.</param>
        /// <param name="dateTimeProvider">A date time provider used to retrieve the current time.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task UpdateLastRunDateAsync(
            ITransactionalStoreWriter writer, 
            IDateTimeProvider dateTimeProvider,
            CancellationToken cancellationToken
        )
        {
            LastRunDate = dateTimeProvider.UtcNow();

            await writer.UpdateScheduledJobLastRunDateAsync(Id, LastRunDate, cancellationToken);
        }

        /// <summary>
        /// Updates the next run date of the scheduled job within the store.
        /// </summary>
        /// <param name="writer">A transactional writer used to update the store.</param>
        /// <param name="dateTimeProvider">A date time provider used to retrieve the current time.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task UpdateNextRunDateAsync(
            ITransactionalStoreWriter writer,
            IDateTimeProvider dateTimeProvider,
            CancellationToken cancellationToken
        )
        {
            NextRunDate = CrontabSchedule
                .Parse(CronExpression, new CrontabSchedule.ParseOptions { IncludingSeconds = true })
                .GetNextOccurrence(dateTimeProvider.UtcNow());

            await writer.UpdateScheduledJobNextRunDateAsync(Id, NextRunDate, cancellationToken);
        }
    }
}