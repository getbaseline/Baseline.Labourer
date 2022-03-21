using System;
using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer;

/// <summary>
/// SqliteResourceLocker locks resources by entering rows in a SQLite database.
/// </summary>
public class SqliteResourceLocker : IResourceLocker
{
    /// <inheritdoc />
    public Task<IAsyncDisposable> LockResourceAsync(string resource, TimeSpan @for, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}