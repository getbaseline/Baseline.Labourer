using System;
using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Server.JobProcessorWorker
{
    public class JobRetrier
    {
        private readonly JobContext _jobContext;
        
        public JobRetrier(JobContext jobContext)
        {
            _jobContext = jobContext;
        }

        public async Task RetryJobOnFailureAsync()
        {
          
        }
    }
}