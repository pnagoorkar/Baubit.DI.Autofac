namespace Baubit.DI.Autofac
{
    /// <summary>
    /// Abstract base class for Autofac module configurations.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="Baubit.DI.AConfiguration"/> to provide configuration support
    /// for Autofac-based modules. Inherit from this class when creating configurations for
    /// modules that use Autofac's ContainerBuilder instead of IServiceCollection.
    /// Thread safety: All public members are thread-safe.
    /// </remarks>
    public abstract class AConfiguration : Baubit.DI.AConfiguration
    {
    }
}
