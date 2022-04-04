using Baseline.Labourer.DependencyInjection;

namespace Baseline.Labourer.Store.Sqlite;

/// <summary>
/// Extension methods related to the <see cref="LabourerBuilder"/> class.
/// </summary>
public static class LabourerBuilderExtensions
{
    public static LabourerBuilder UseSqliteStore(
        this LabourerBuilder builder,
        string connectionString
    )
    {
        builder.Store = new SqliteStore(connectionString);
        return builder;
    }
}
