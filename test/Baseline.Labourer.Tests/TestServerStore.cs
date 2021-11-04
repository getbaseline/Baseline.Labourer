using System.Linq;
using Baseline.Labourer.Store.Memory;
using FluentAssertions;

namespace Baseline.Labourer.Tests
{
    public class TestServerStore : MemoryServerStore
    {
        public string AssertHasRegisteredAServer()
        {
            Servers.Should().HaveCountGreaterOrEqualTo(1);
            return Servers.First().Id;
        }

        public void AssertHasRegisteredWorkersForServer(string serverId, int? count = null)
        {
            ServerWorkers.ContainsKey(serverId).Should().BeTrue();

            if (count != null)
            {
                ServerWorkers[serverId].Count.Should().Be(count);
            }
            else
            {
                ServerWorkers[serverId].Count.Should().BeGreaterThanOrEqualTo(1);
            }
        }

        public void AssertHeartbeatRegisteredForServer(string server)
        {
            ServerHeartbeats.ContainsKey(server).Should().BeTrue();
            ServerHeartbeats[server].Count.Should().BeGreaterThanOrEqualTo(1);
        }
    }
}