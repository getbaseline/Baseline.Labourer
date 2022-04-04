using System.Collections;
using System.Collections.Generic;
using Baseline.Labourer.Tests.Scenarios.Internal;

namespace Baseline.Labourer.Tests.Scenarios.Configurations;

public class RunOnAllProvidersConfiguration : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        return new List<object[]>
        {
            new object[] { QueueProvider.Memory, StoreProvider.Memory },
            new object[] { QueueProvider.SQLite, StoreProvider.SQLite },
        }.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
