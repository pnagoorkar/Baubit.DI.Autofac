using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Baubit.DI.Autofac
{
    /// <summary>
    /// Abstract base class for Autofac modules with strongly-typed configuration.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration for this module, must inherit from <see cref="AConfiguration"/>.</typeparam>
    /// <remarks>
    /// This class extends <see cref="Baubit.DI.AModule{TConfiguration}"/> to provide Autofac-specific module functionality.
    /// Unlike standard DI modules that use IServiceCollection, Autofac modules register services using ContainerBuilder,
    /// enabling advanced Autofac features like interceptors, decorators, and custom lifetime scopes.
    /// 
    /// Thread safety: All public members are thread-safe.
    /// 
    /// Usage:
    /// 1. Create a configuration class inheriting from <see cref="AConfiguration"/>
    /// 2. Create a module class inheriting from this class
    /// 3. Override <see cref="Load(ContainerBuilder)"/> to register services using Autofac's API
    /// 4. Optionally override <see cref="Baubit.DI.AModule.OnInitialized"/> or <see cref="Baubit.DI.AModule.GetKnownDependencies"/> as needed
    /// </remarks>
    public abstract class AModule<TConfiguration> : Baubit.DI.AModule<TConfiguration>, IModule where TConfiguration : AConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the module from configuration.
        /// </summary>
        /// <param name="configuration">The configuration section to bind to the module's configuration object.</param>
        /// <remarks>
        /// This constructor is used when loading modules from appsettings.json or other IConfiguration sources.
        /// The configuration is automatically bound to the TConfiguration type.
        /// </remarks>
        public AModule(IConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the module with explicit configuration and optional nested modules.
        /// </summary>
        /// <param name="configuration">The strongly-typed configuration for this module.</param>
        /// <param name="nestedModules">Optional list of nested modules to be loaded when this module loads.</param>
        /// <remarks>
        /// This constructor is used when creating modules programmatically, typically from IComponent implementations.
        /// Nested modules are loaded recursively when this module's Load method is called.
        /// </remarks>
        public AModule(TConfiguration configuration, List<Baubit.DI.IModule> nestedModules = null) : base(configuration, nestedModules)
        {
        }

        /// <summary>
        /// This method is obsolete for Autofac modules. Use <see cref="Load(ContainerBuilder)"/> instead.
        /// </summary>
        /// <param name="services">The service collection (not used).</param>
        /// <exception cref="NotImplementedException">Always throws when called. Use the ContainerBuilder overload.</exception>
        /// <remarks>
        /// Autofac modules must use ContainerBuilder for registration. This method is marked as obsolete with error=true
        /// to prevent accidental usage of the IServiceCollection-based Load method.
        /// </remarks>
        [Obsolete("Use the overload with ContainerBuilder", error: true)]
        public override void Load(IServiceCollection services)
        {            
            throw new System.NotImplementedException($"Use the overload with ContainerBuilder");
        }

        /// <summary>
        /// Loads the module's service registrations into the Autofac container builder.
        /// </summary>
        /// <param name="containerBuilder">The Autofac container builder to register services with.</param>
        /// <remarks>
        /// Override this method to register services, factories, and other components using Autofac's registration API.
        /// The base implementation does nothing, so calling base.Load(containerBuilder) is optional unless you need
        /// to load nested modules using custom logic.
        /// 
        /// Note: Nested modules are handled automatically by the ServiceProviderFactory. If all nested modules
        /// should be loaded, you don't need to explicitly iterate through them here.
        /// 
        /// Example:
        /// <code>
        /// public override void Load(ContainerBuilder containerBuilder)
        /// {
        ///     containerBuilder.RegisterType&lt;MyService&gt;().As&lt;IMyService&gt;().SingleInstance();
        ///     containerBuilder.Register(c => new MyFactory(Configuration.ConnectionString)).AsSelf();
        /// }
        /// </code>
        /// </remarks>
        public virtual void Load(ContainerBuilder containerBuilder)
        {
            // NO ACTION NEEDED - Override in derived classes to register services
        }
    }
}
