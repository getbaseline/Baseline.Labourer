using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
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

        protected readonly TestQueue TestQueue = new TestQueue();

        protected readonly TestMemoryStore TestStore = new TestMemoryStore();

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

            Client = new LabourerClient(
                new BaselineLabourerConfiguration
                {
                    LoggerFactory = () => TestLoggerFactory
                },
                new MemoryResourceLocker(TestStore),
                new MemoryStoreWriterTransactionManager(TestStore),
                TestQueue
            );
        }

        public ServerContext GenerateServerContextAsync(Action<ServerContext>? configuror = null)
        {
            var serverInstance = new ServerInstance
            {
                Hostname = Dns.GetHostName(),
                Key = StringGenerationUtils.GenerateUniqueRandomString()
            };
            TestStore.Servers.Add(serverInstance);

            ServerId = serverInstance.Id;

            var serverContext = new ServerContext
            {
                Activator = new DefaultActivator(),
                JobLogStore = new MemoryJobLogStore(TestStore),
                StoreReader = new MemoryStoreReader(TestStore),
                ResourceLocker = new MemoryResourceLocker(TestStore),
                Queue = TestQueue,
                ServerInstance = serverInstance,
                StoreWriterTransactionManager = new MemoryStoreWriterTransactionManager(TestStore),
                ShutdownTokenSource = _cancellationTokenSource,
                LoggerFactory = TestLoggerFactory,
                ScheduledJobProcessorInterval = TimeSpan.FromMilliseconds(500),
                DefaultRetryConfiguration = new RetryConfiguration(3, TimeSpan.Zero)
            };

            configuror?.Invoke(serverContext);

            return serverContext;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}