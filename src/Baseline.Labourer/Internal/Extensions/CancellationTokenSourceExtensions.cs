using System;
using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Internal.Extensions;

/// <summary>
/// Contains a number of extension methods related to <see cref="CancellationTokenSource"/> instances.
/// </summary>
public static class CancellationTokenSourceExtensions
{
    /// <summary>
    /// Waits for a specified amount of time before resolving or, if the cancellation token source is cancelled
    /// before then, throws a <see cref="TaskCanceledException"/> immediately. Useful when waiting before repeating
    /// tasks but ensures shutting down the server doesn't take the time of the longest task wait.
    /// </summary>
    /// <param name="source">The cancellation token source.</param>
    /// <param name="timeSpan">A timespan to wait for before returning.</param>
    public static async Task WaitForTimeOrCancellationAsync(
        this CancellationTokenSource source,
        TimeSpan timeSpan
    )
    {
        await Task.Delay(timeSpan, source.Token);
    }
}
