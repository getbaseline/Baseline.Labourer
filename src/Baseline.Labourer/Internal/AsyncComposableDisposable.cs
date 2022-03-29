using System;
using System.Threading.Tasks;

namespace Baseline.Labourer.Internal;

/// <summary>
/// AsyncComposableDisposable is an <see cref="IAsyncDisposable" /> interface that allows the await using statement to
/// be co-opted into running a deferred statement. An example of when this might be used is when locking a resource and
/// releasing it when the current scope is finished.
/// </summary>
public class AsyncComposableDisposable : IAsyncDisposable
{
    private readonly Func<ValueTask> _action;

    public AsyncComposableDisposable(Func<ValueTask> action)
    {
        _action = action;
    }

    /// <summary>
    /// Disposes of the disposable, running whatever cleanup task was provided in the constructor.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _action();
    }
}
