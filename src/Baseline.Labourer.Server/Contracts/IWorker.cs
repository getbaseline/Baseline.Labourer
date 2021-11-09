﻿namespace Baseline.Labourer.Server.Contracts;

/// <summary>
/// IWorker defines what all server workers must implement.
/// </summary>
public interface IWorker
{
    /// <summary>
    /// Runs the worker as a long running task.
    /// </summary>
    Task RunAsync(CancellationToken cancellationToken = default);
}
