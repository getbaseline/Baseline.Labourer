using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Baseline.Labourer
{
    public class BaselineLabourerHostedService : IHostedService
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly LabourerServer _labourerServer;

        public BaselineLabourerHostedService(IServiceProvider serviceProvider)
        {
            _labourerServer = serviceProvider.GetService<LabourerServer>()!;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _labourerServer.RunServerAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}