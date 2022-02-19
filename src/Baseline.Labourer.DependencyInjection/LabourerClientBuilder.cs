namespace Baseline.Labourer.DependencyInjection
{
    /// <summary>
    /// Interim configuration object for dependency injection configured Baseline.Labourer client.
    /// </summary>
    public class LabourerClientBuilder : BaseLabourerBuilder
    {
        /// <summary>
        /// Converts the <see cref="LabourerClientBuilder"/> instance to a <see cref="BaselineLabourerConfiguration"/>
        /// instance. 
        /// </summary>
        public BaselineLabourerConfiguration ToConfiguration()
        {
            return new BaselineLabourerConfiguration
            {
                LoggerFactory = LoggerFactory,
                Queue = Queue,
                Store = Store
            };
        }
    }
}