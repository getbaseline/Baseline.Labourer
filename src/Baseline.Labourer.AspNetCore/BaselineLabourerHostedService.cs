using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Baseline.Labourer
{
    /// <summary>
    /// A hosted service used to run the Baseline.Labourer server.
    /// </summary>
    public class BaselineLabourerHostedService : IHostedService
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly LabourerServer _labourerServer;

        public BaselineLabourerHostedService(IServiceProvider serviceProvider)
        {
            _labourerServer = serviceProvider.GetService<LabourerServer>()!;
        }

        /// <summary>
        /// Runs the server, booting all of the relevant workers to process jobs, dispatch scheduled jobs that need to
        /// run and more.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _labourerServer.RunServerAsync();
        }

        /// <summary>
        /// Cancels the cancellation token source used within the Baseline.Labourer server which should result in the
        /// graceful shutdown of all relevant services.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}