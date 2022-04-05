using System;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Tests.Configurations;
using Baseline.Labourer.Tests.Internal;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Tests;

public class JobLogging : BaseTest
{
    public JobLogging(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <summary>
    /// Tests that messages do get logged to a provided logging factory.
    /// </summary>
    /// <param name="queueProvider"></param>
    /// <param name="storeProvider"></param>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task LogsRelevantMessagesToTheProvidedLoggingFactory(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        LogsRelevantMessagesToTheProvidedLoggingFactoryJobLogger.LoggedMessage = false;

        await BootstrapAsync(
            queueProvider,
            storeProvider,
            server =>
            {
                server.LoggerFactory = () =>
                    new LogsRelevantMessagesToTheProvidedLoggingFactoryJobLoggerFactory();
            }
        );

        // Act.
        await Client.DispatchJobAsync<LogsRelevantMessagesToTheProvidedLoggingFactoryJob>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                LogsRelevantMessagesToTheProvidedLoggingFactoryJobLogger.LoggedMessage
                    .Should()
                    .BeTrue();
            }
        );
    }

    #region Test Dependencies
    public class LogsRelevantMessagesToTheProvidedLoggingFactoryJobLoggerFactory : ILoggerFactory
    {
        public void Dispose() { }

        public ILogger CreateLogger(string categoryName)
        {
            return new LogsRelevantMessagesToTheProvidedLoggingFactoryJobLogger();
        }

        public void AddProvider(ILoggerProvider provider) { }
    }

    public class LogsRelevantMessagesToTheProvidedLoggingFactoryJobLogger : ILogger
    {
        public static bool LoggedMessage;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            LoggedMessage = true;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new ComposableDisposable(() => { });
        }
    }

    public class LogsRelevantMessagesToTheProvidedLoggingFactoryJob : IJob
    {
        public ValueTask HandleAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
    #endregion
}
