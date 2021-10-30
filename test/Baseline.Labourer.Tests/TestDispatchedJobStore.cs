using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Tests
{
    public class TestDispatchedJobStore : IDispatchedJobStore
    {
        private readonly List<DispatchedJobDefinition> _dispatchedJobs = new List<DispatchedJobDefinition>();
        
        public Task<DispatchedJobDefinition> SaveDispatchedJobDefinitionAsync(
            DispatchedJobDefinition definition, 
            CancellationToken cancellationToken
        )
        {
            _dispatchedJobs.Add(definition);
            return Task.FromResult(definition);
        }

        public DispatchedJobDefinition AssertJobWithTypesStored(Type jobType, Type parametersType)
        {
            var jobDefinition = _dispatchedJobs.FirstOrDefault(
                j => j.Type == jobType.AssemblyQualifiedName &&
                     j.ParametersType == parametersType.AssemblyQualifiedName
            );

            if (jobDefinition == null)
            {
                throw new Exception(
                    $"No jobs found with job type of {jobType.AssemblyQualifiedName} and parameter type of {parametersType.AssemblyQualifiedName}."
                );
            }

            return jobDefinition;
        }
    }
}