using System;

namespace Baseline.Labourer.Internal;

/// <summary>
/// An <see cref="IDisposable"/> implementation that can perform a specific action when dispose is called.
/// </summary>
public class ComposableDisposable : IDisposable
{
    private readonly Action _action;

    public ComposableDisposable(Action action)
    {
        _action = action;
    }

    public void Dispose()
    {
        _action();
    }
}
