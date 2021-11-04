using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Store.Memory
{
    public class MemoryServerStore : IServerStore
    {
        private readonly SemaphoreSlim _serverSemaphore = new SemaphoreSlim(1); 
        protected readonly List<ServerInstance> Servers = new List<ServerInstance>();
        protected readonly Dictionary<string, List<Worker>> ServerWorkers = new Dictionary<string, List<Worker>>();
        protected readonly Dictionary<string, List<DateTime>> ServerHeartbeats = new Dictionary<string, List<DateTime>>();
        
        public async Task<ServerInstance> CreateServerAsync(ServerInstance serverInstance, CancellationToken cancellationToken)
        {
            try
            {
                await _serverSemaphore.WaitAsync(cancellationToken);
                Servers.Add(serverInstance);
            }
            finally
            {
                _serverSemaphore.Release();
            }

            return serverInstance;
        }

        public async Task CreateServerHeartbeat(string serverId, CancellationToken cancellationToken)
        {
            try
            {
                await _serverSemaphore.WaitAsync(cancellationToken);

                if (!ServerHeartbeats.ContainsKey(serverId))
                {
                    ServerHeartbeats[serverId] = new List<DateTime>();
                }
                
                ServerHeartbeats[serverId].Add(DateTime.UtcNow);
            }
            finally
            {
                _serverSemaphore.Release();
            }
        }

        public async Task<Worker> CreateWorkerAsync(Worker worker, CancellationToken cancellationToken)
        {
            try
            {
                await _serverSemaphore.WaitAsync(cancellationToken);

                if (!ServerWorkers.ContainsKey(worker.ServerInstanceId))
                {
                    ServerWorkers[worker.ServerInstanceId] = new List<Worker>();
                }

                ServerWorkers[worker.ServerInstanceId].Add(worker);
            }
            finally
            {
                _serverSemaphore.Release();
            }

            return worker;
        }
    }
}