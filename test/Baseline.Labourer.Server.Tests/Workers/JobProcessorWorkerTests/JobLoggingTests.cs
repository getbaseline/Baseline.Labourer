using Baseline.Labourer.Tests;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests.Workers.JobProcessorWorkerTests;

public class JobLoggingTests : ServerTest
{
    protected class TestLogger : ILogger, IDisposable
    {
        public static List<string> LoggedMessages { get; } = new List<string>();

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter
        )
        {
            LoggedMessages.Add(formatter(state, exception));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public void Dispose()
        {
        }

        public static bool HasLoggedMessage(string message)
        {
            return LoggedMessages.Contains(message);
        }
    }
    protected class TestLoggerFactory : ILoggerFactory
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger();
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }
    }

    public JobLoggingTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        Task.Run(
            async () => await new JobProcessorWorker.JobProcessorWorker(await GenerateServerContextAsync()).RunAsync()
        );
    }

    public class LoggerTestJob : IJob
    {
        private readonly ILogger<LoggerTestJob> _logger;

        public LoggerTestJob(ILogger<LoggerTestJob> logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Message one.");

            await Task.Delay(1000, cancellationToken);

            _logger.LogInformation("Message two.");
        }
    }

    [Fact]
    public async Task It_Logs_Job_Messages_To_The_Wrapped_Logger_And_The_Server_Store()
    {
        // Arrange.
        var serverContext = await GenerateServerContextAsync();
        serverContext.LoggerFactory = new TestLoggerFactory();

        // Act.
        var jobId = await Client.DispatchJobAsync<LoggerTestJob>();

        // Assert.
        await AssertionUtils.RetryAsync(() =>
        {
            TestLogger.HasLoggedMessage("Message one.");
            TestStore.AssertMessageForJobLogged(jobId, "Message one.");

            TestLogger.HasLoggedMessage("Message two.");
            TestStore.AssertMessageForJobLogged(jobId, "Message two.");
        });
    }
}
