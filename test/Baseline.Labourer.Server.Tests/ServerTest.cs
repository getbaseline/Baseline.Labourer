using System;
using System.Threading;
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

        protected readonly TestMemoryStoreDataContainer TestStoreDataContainer = new TestMemoryStoreDataContainer();

        protected readonly TestDateTimeProvider TestDateTimeProvider = new TestDateTimeProvider();

        protected readonly ILoggerFactory TestLoggerFactory;

        protected readonly TestMemoryStore TestMemoryStore;

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
            TestResourceLocker = new TestMemoryResourceLocker(TestStoreDataContainer, TestDateTimeProvider);
            TestMemoryStore = new TestMemoryStore(TestStoreDataContainer, TestDateTimeProvider);
            
            Client = new LabourerClient(
                new BaselineLabourerConfiguration
                {
                    LoggerFactory = () => TestLoggerFactory,
                    Queue = TestMemoryQueue,
                    Store = TestMemoryStore
                }
            );
        }

        public BaselineServerConfiguration GenerateServerConfiguration(Action<BaselineServerConfiguration>? configuror = null)
        {
            var configuration = new BaselineServerConfiguration
            {
                Activator = new DefaultActivator(),
                Store = TestMemoryStore,
                Queue = TestMemoryQueue,
                ShutdownTokenSource = _cancellationTokenSource,
                LoggerFactory = () => TestLoggerFactory,
                ScheduledJobProcessorInterval = TimeSpan.FromMilliseconds(500),
                DefaultRetryConfiguration = new RetryConfiguration(3, TimeSpan.Zero),
                JobProcessingWorkersToRun = 1,
                DateTimeProvider = TestDateTimeProvider
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