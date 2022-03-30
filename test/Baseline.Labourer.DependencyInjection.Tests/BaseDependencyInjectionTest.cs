using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Baseline.Labourer.DependencyInjection.Tests;

public class BaseDependencyInjectionTest : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ILoggerFactory _loggerFactory;
    private IServiceProvider _serviceProvider;

    protected ILabourerClient Client => Resolve<ILabourerClient>();
    protected LabourerServer Server => Resolve<LabourerServer>();

    protected BaseDependencyInjectionTest(ITestOutputHelper testOutputHelper)
    {
        _loggerFactory = LoggerFactory.Create(
            logging =>
            {
                logging.AddXUnit(testOutputHelper);
            }
        );
    }

    protected void ConfigureServices(Action<IServiceProvider, LabourerBuilder> builder)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton(_loggerFactory);
        serviceCollection.AddBaselineLabourer(
            (sp, b) =>
            {
                builder(sp, b);
                b.ConfigureServer(
                    serverBuilder =>
                        serverBuilder.UseThisShutdownTokenSource(_cancellationTokenSource)
                );
            }
        );

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    protected T Resolve<T>() => _serviceProvider.GetService<T>();

    protected void RunServer()
    {
        Task.Run(async () => await Server.RunServerAsync());
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
    }
}
