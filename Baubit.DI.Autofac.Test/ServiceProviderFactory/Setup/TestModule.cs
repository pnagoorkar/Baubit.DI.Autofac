using Autofac;
using Baubit.DI;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Baubit.DI.Autofac.Test.ServiceProviderFactory.Setup
{
    /// <summary>
    /// Test module that uses Autofac ContainerBuilder for registration.
    /// </summary>
    [BaubitModule("test-autofac-module")]
    public class TestModule : AModule<TestConfiguration>
    {
        public TestModule(IConfiguration configuration) : base(configuration) { }

        public TestModule(TestConfiguration configuration, List<Baubit.DI.IModule>? nestedModules = null)
            : base(configuration, nestedModules) { }

        public override void Load(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterInstance(new TestService(Configuration.ServiceName))
                           .As<ITestService>()
                           .SingleInstance();
            base.Load(containerBuilder);
        }
    }
}
