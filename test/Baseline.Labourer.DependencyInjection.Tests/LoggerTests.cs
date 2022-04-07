using System;
using System.Threading.Tasks;
using Baseline.Labourer.Queue.NoOp;
using Baseline.Labourer.Store.NoOp;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.DependencyInjection.Tests;

public class LoggerTests : BaseDependencyInjectionTest
{
    public LoggerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    public class TrackableLoggerFactory : ILoggerFactory
    {
        public void Dispose() { }

        public ILogger CreateLogger(string categoryName)
        {
            return new TrackableLogger();
        }

        public void AddProvider(ILoggerProvider provider) { }
    }

    public class TrackableLogger : ILogger
    {
        public static bool MessageLogged;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter
        )
        {
            MessageLogged = true;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }

    [Fact]
    public async Task It_Uses_A_Logger_Factory_Provided_By_The_Consumer()
    {
        // Arrange.
        ConfigureServices(
            (sp, builder) =>
            {
                builder.UseThisLoggerFactory(() => new TrackableLoggerFactory());
                builder.UseNoOpQueue();
                builder.UseNoOpStore();
            }
        );

        // Act.
        RunServer();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                TrackableLogger.MessageLogged.Should().BeTrue();
            }
        );
    }
}
