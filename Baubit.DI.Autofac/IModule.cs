using Autofac;

namespace Baubit.DI.Autofac
{
    /// <summary>
    /// Interface for Autofac dependency injection modules.
    /// </summary>
    /// <remarks>
    /// This interface extends <see cref="Baubit.DI.IModule"/> to provide support for Autofac's
    /// ContainerBuilder. Modules implementing this interface can register services using
    /// Autofac's advanced features like interceptors, decorators, and lifetime scopes.
    /// </remarks>
    public interface IModule : Baubit.DI.IModule
    {
        /// <summary>
        /// Loads the module's service registrations into the Autofac container builder.
        /// </summary>
        /// <param name="containerBuilder">The Autofac container builder to register services with.</param>
        /// <remarks>
        /// Override this method to register services, factories, and other components using
        /// Autofac's registration API. This method is called during the container building phase.
        /// Thread safety: This method should be thread-safe as it may be called during concurrent initialization.
        /// </remarks>
        void Load(ContainerBuilder containerBuilder);
    }
}
