using Baubit.DI;
using FluentResults;

namespace Baubit.DI.Autofac.Test.ServiceProviderFactory.Setup
{
    /// <summary>
    /// Test component that creates Autofac test modules programmatically.
    /// </summary>
    public class TestComponent : Component
    {
        private readonly string _serviceName;

        public TestComponent(string serviceName)
        {
            _serviceName = serviceName;
        }

        protected override Result<ComponentBuilder> Build(ComponentBuilder builder)
        {
            return builder.WithModule<TestModule, TestConfiguration>(config =>
            {
                config.ServiceName = _serviceName;
            }, config => new TestModule(config));
        }
    }

    /// <summary>
    /// Test component for standard DI modules.
    /// </summary>
    public class StandardDIComponent : Component
    {
        private readonly string _message;

        public StandardDIComponent(string message)
        {
            _message = message;
        }

        protected override Result<ComponentBuilder> Build(ComponentBuilder builder)
        {
            return builder.WithModule<StandardDITestModule, StandardDITestConfiguration>(config =>
            {
                config.Message = _message;
            }, config => new StandardDITestModule(config));
        }
    }
}
