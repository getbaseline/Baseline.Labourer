using System;
using System.Net;
using System.Threading;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Internal.Utils;
using Baseline.Labourer.Store.Memory;
using Baseline.Labourer.Tests;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests
{
    public class ServerTest : IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        protected readonly TestMemoryResourceLocker TestResourceLocker;

        protected readonly TestMemoryQueue TestMemoryQueue;

        protected readonly TestMemoryBackingStore TestBackingStore = new TestMemoryBackingStore();

        protected readonly TestDateTimeProvider TestDateTimeProvider = new TestDateTimeProvider();

        protected readonly ILoggerFactory TestLoggerFactory;

        protected string ServerId;

        public LabourerClient Client { get; }

        public ServerTest(ITestOutputHelper testOutputHelper)
        {
            TestLoggerFactory = LoggerFactory.Create(logger =>
            {
                logger
                    .AddXUnit(testOutputHelper)
                    .SetMinimumLevel(LogLevel.Debug);
            });

            TestMemoryQueue = new TestMemoryQueue(TestDateTimeProvider);
            TestResourceLocker = new TestMemoryResourceLocker(TestBackingStore, TestDateTimeProvider);

            Client = new LabourerClient(
                new BaselineLabourerConfiguration
                {
                    LoggerFactory = () => TestLoggerFactory
                },
                TestResourceLocker,
                new MemoryStoreWriterTransactionManager(TestBackingStore),
                TestMemoryQueue
            );
        }

        public BaselineServerConfiguration GenerateServerConfiguration(Action<BaselineServerConfiguration>? configuror = null)
        {
            var configuration = new BaselineServerConfiguration
            {
                Activator = new DefaultActivator(),
                Store = new TestMemoryStore(TestBackingStore, TestDateTimeProvider),
                Queue = TestMemoryQueue,
                ShutdownTokenSource = _cancellationTokenSource,
                LoggerFactory = () => TestLoggerFactory,
                ScheduledJobProcessorInterval = TimeSpan.FromMilliseconds(500),
                DefaultRetryConfiguration = new RetryConfiguration(3, TimeSpan.Zero)
            };
            
            configuror?.Invoke(configuration);

            return configuration;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}