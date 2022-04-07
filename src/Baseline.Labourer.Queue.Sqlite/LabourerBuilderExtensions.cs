using Baseline.Labourer.DependencyInjection;

namespace Baseline.Labourer.Queue.Sqlite;

/// <summary>
/// Extension methods related to the SQLite queue for inherits of the <see cref="LabourerBuilder"/> class.
/// </summary>
public static class LabourerBuilderExtensions
{
    /// <summary>
    /// Configures the <see cref="LabourerBuilder"/> instance to use the SQLite queue provider.
    /// </summary>
    /// <param name="builder">The builder to configure to use the SQLite queue provider.</param>
    /// <param name="connectionString">The SQLite connection string to use.</param>
    /// <returns></returns>
    public static LabourerBuilder UseSqliteQueue(
        this LabourerBuilder builder,
        string connectionString
    )
    {
        builder.Queue = new SqliteQueue(connectionString);
        return builder;
    }
}
