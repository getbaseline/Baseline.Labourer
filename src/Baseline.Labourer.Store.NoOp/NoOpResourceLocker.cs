using System;
using System.Threading.Tasks;

namespace Baseline.Labourer.Store.NoOp;

/// <summary>
/// NoOpResourceLocker is an <see cref="IResourceLocker"/> implementation that literally does nothing.
/// </summary>
public class NoOpResourceLocker : IResourceLocker
{
    private class NoOpAsyncDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }

    /// <inheritdoc />
    public Task<IAsyncDisposable> LockResourceAsync(string resource, TimeSpan @for)
    {
        return Task.FromResult((IAsyncDisposable)new NoOpAsyncDisposable());
    }
}
