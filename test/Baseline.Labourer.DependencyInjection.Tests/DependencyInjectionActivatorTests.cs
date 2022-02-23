using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Baseline.Labourer.DependencyInjection.Tests
{
    public class DependencyInjectionActivatorTests
    {
        public class Dependency
        {
            public static bool Ran;

            public void DoSomething()
            {
                Ran = true;
            }
        }

        public class HasDependencyInjected
        {
            private readonly Dependency _dependency;

            public HasDependencyInjected(Dependency dependency)
            {
                _dependency = dependency;
            }
            
            public void Run()
            {
                _dependency.DoSomething();
            }
        }
        
        [Fact]
        public void It_Successfully_Resolves_Jobs_And_Things_With_Dependencies_From_The_Container()
        {
            // Arrange.
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<Dependency>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var activator = new DependencyInjectionActivator(serviceProvider);
            
            // Act.
            (activator.ActivateType(typeof(HasDependencyInjected)) as HasDependencyInjected)!.Run();
            
            // Assert.
            Dependency.Ran.Should().BeTrue();
        }

        public class ProvidedLogger : ILogger<HasDependencyInjected>
        {
            public void Log<TState>(
                LogLevel logLevel, 
                EventId eventId, 
                TState state, 
                Exception? exception, 
                Func<TState, Exception?, string> formatter
            )
            {
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

        public class HasLoggerInjected
        {
            public static Type InjectedLoggerType;
            
            public HasLoggerInjected(ILogger<HasLoggerInjected> logger)
            {
                InjectedLoggerType = logger.GetType();
            }
        }

        [Fact]
        public void It_Overrides_The_Relevant_Logger_Parameters_If_Provided()
        {
            // Arrange.
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<Dependency>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var activator = new DependencyInjectionActivator(serviceProvider);
            
            // Act.
            var _ = activator.ActivateType(typeof(HasLoggerInjected), new ProvidedLogger()) as HasLoggerInjected;
            
            // Assert.
            HasLoggerInjected.InjectedLoggerType.Should().Be(typeof(ProvidedLogger));
        }
    }
}