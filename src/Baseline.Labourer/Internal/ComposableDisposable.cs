using System;

namespace Baseline.Labourer.Internal;

/// <summary>
/// An <see cref="IDisposable"/> implementation that can perform a specific action when dispose is called. Allows the
/// using statement to be co-opted into running a deferred statement. An example of when this might be used is when
/// locking a resource and releasing it when the current scope is finished.
/// </summary>
public class ComposableDisposable : IDisposable
{
    private readonly Action _action;

    public ComposableDisposable(Action action)
    {
        _action = action;
    }

    /// <summary>
    /// Disposes of the disposable, calling the delegate passed into the constructor.
    /// </summary>
    public void Dispose()
    {
        _action();
    }
}
