using Autofac;

namespace Baubit.DI.Autofac
{
    public interface IModule : Baubit.DI.IModule
    {
        void Load(ContainerBuilder containerBuilder);
    }
}
