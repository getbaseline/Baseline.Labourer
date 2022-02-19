using System;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.DependencyInjection
{
    public class BaseLabourerBuilder
    {
        public Func<ILoggerFactory> LoggerFactory { get; set; }

        public IQueue Queue { get; set; }

        public IStore Store { get; set; }
    }
}
