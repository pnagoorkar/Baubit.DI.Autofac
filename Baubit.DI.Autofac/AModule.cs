using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Baubit.DI.Autofac
{
    public abstract class AModule<TConfiguration> : Baubit.DI.AModule<TConfiguration>, IModule where TConfiguration : AConfiguration
    {
        public AModule(IConfiguration configuration) : base(configuration)
        {
        }

        public AModule(TConfiguration configuration, List<Baubit.DI.IModule> nestedModules = null) : base(configuration, nestedModules)
        {
        }

        [Obsolete("Use the overload with ContainerBuilder", error: true)]
        public override void Load(IServiceCollection services)
        {            
            throw new System.NotImplementedException($"Use the overload with ContainerBuilder");
        }

        public virtual void Load(ContainerBuilder containerBuilder)
        {
            // NO ACTION NEEDED
        }
    }
}
