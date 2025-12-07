using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Baubit.DI.Autofac
{
    /// <summary>
    /// Service provider factory that integrates Baubit.DI module system with Autofac container.
    /// </summary>
    /// <remarks>
    /// This factory enables seamless integration between Baubit.DI's module system and Autofac's
    /// dependency injection container. It supports:
    /// - Loading modules from configuration (appsettings.json)
    /// - Loading modules from code (IComponent)
    /// - Hybrid loading (both configuration and code)
    /// - Mixed module types (both Autofac and standard DI modules)
    /// 
    /// When loading modules, the factory distinguishes between:
    /// - Autofac modules (implementing <see cref="IModule"/>) - registered directly with ContainerBuilder
    /// - Standard DI modules (implementing <see cref="Baubit.DI.IModule"/>) - registered with IServiceCollection and populated into Autofac
    /// 
    /// Thread safety: All public members are thread-safe.
    /// </remarks>
    public class ServiceProviderFactory : AServiceProviderFactory<ContainerBuilder>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderFactory"/> class with a custom Autofac factory.
        /// </summary>
        /// <param name="internalFactory">The Autofac service provider factory to use for container management.</param>
        /// <param name="configuration">The configuration to load modules from.</param>
        /// <param name="components">Optional array of components containing modules to load programmatically.</param>
        /// <remarks>
        /// This constructor allows using a custom configured AutofacServiceProviderFactory if special
        /// Autofac container options are needed (e.g., custom configuration actions).
        /// </remarks>
        public ServiceProviderFactory(Microsoft.Extensions.DependencyInjection.IServiceProviderFactory<ContainerBuilder> internalFactory, 
                                      IConfiguration configuration, 
                                      IComponent[] components) : base(internalFactory, configuration, components)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderFactory"/> class with the default Autofac factory.
        /// </summary>
        /// <param name="configuration">The configuration to load modules from.</param>
        /// <param name="components">Optional array of components containing modules to load programmatically.</param>
        /// <remarks>
        /// This is the most common constructor. It creates a default AutofacServiceProviderFactory internally.
        /// Modules are loaded in the following order:
        /// 1. Modules from components (if provided)
        /// 2. Modules from configuration (if present in appsettings.json)
        /// All modules are then flattened to resolve nested module hierarchies.
        /// </remarks>
        public ServiceProviderFactory(IConfiguration configuration,
                                      IComponent[] components) : this(new AutofacServiceProviderFactory(), configuration, components)
        {
        }

        /// <summary>
        /// Loads all modules into the Autofac container builder.
        /// </summary>
        /// <param name="containerBuilder">The Autofac container builder to load modules into.</param>
        /// <remarks>
        /// This method handles both Autofac-specific modules and standard DI modules:
        /// 
        /// For Autofac modules (implementing <see cref="IModule"/>):
        /// - Calls Load(ContainerBuilder) directly, allowing full use of Autofac features
        /// 
        /// For standard DI modules (implementing only <see cref="Baubit.DI.IModule"/>):
        /// - Calls Load(IServiceCollection) and populates the results into Autofac
        /// 
        /// This approach allows mixing both module types in the same application, enabling
        /// gradual migration or selective use of Autofac features where needed.
        /// 
        /// Thread safety: This method is thread-safe and can be called during concurrent initialization.
        /// </remarks>
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
