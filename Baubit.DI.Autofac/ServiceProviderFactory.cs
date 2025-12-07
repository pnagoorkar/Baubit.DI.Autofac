using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Baubit.DI.Autofac
{
    public class ServiceProviderFactory : AServiceProviderFactory<ContainerBuilder>
    {
        public ServiceProviderFactory(Microsoft.Extensions.DependencyInjection.IServiceProviderFactory<ContainerBuilder> internalFactory, 
                                      IConfiguration configuration, 
                                      IComponent[] components) : base(internalFactory, configuration, components)
        {
        }
        public ServiceProviderFactory(IConfiguration configuration,
                                      IComponent[] components) : this(new AutofacServiceProviderFactory(), configuration, components)
        {
        }

        public override void Load(ContainerBuilder containerBuilder)
        {
            var services = new ServiceCollection();
            foreach (var module in Modules)
            {
                if (module is IModule autofacModule)
                {
                    autofacModule.Load(containerBuilder);
                }
                else
                {
                    module.Load(services);
                }
            }
            containerBuilder.Populate(services);
        }
    }
}
