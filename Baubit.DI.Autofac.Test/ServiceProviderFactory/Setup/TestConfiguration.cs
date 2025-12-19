namespace Baubit.DI.Autofac.Test.ServiceProviderFactory.Setup
{
    /// <summary>
    /// Test configuration for Autofac test modules.
    /// </summary>
    public class TestConfiguration : Configuration
    {
        public string ServiceName { get; set; } = "Default Service";
    }
}
