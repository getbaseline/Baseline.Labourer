﻿namespace Baseline.Labourer.Store.Memory;

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
