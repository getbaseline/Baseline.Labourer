using System;
using System.Threading.Tasks;

namespace Baseline.Labourer.Store.Memory.Internal
{
    public class AsyncComposableDisposable : IAsyncDisposable
    {
        private readonly Func<ValueTask> _action;

        public AsyncComposableDisposable(Func<ValueTask> action)
        {
            _action = action;
        }
        
        public async ValueTask DisposeAsync()
        {
            await _action();
        }
    }
}