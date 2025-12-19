using Baubit.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Baubit.DI.Autofac.Test.ServiceProviderFactory.Setup
{
    /// <summary>
    /// Configuration for standard DI test module.
    /// </summary>
    public class StandardDITestConfiguration : Baubit.DI.Configuration
    {
        public string Message { get; set; } = "Default Message";
    }

    /// <summary>
    /// Test module that uses standard IServiceCollection (not Autofac-specific).
    /// This demonstrates mixed module support.
    /// </summary>
    [BaubitModule("test-standard-di-module")]
    public class StandardDITestModule : Baubit.DI.Module<StandardDITestConfiguration>
    {
        public StandardDITestModule(IConfiguration configuration) : base(configuration) { }

        public StandardDITestModule(StandardDITestConfiguration configuration, List<Baubit.DI.IModule>? nestedModules = null)
            : base(configuration, nestedModules) { }

        public override void Load(IServiceCollection services)
        {
            services.AddSingleton<IAnotherService>(new AnotherService(Configuration.Message));
            base.Load(services);
        }
    }
}
