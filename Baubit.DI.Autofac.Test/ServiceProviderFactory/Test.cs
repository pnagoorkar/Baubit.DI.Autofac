using Baubit.DI.Autofac.Test.ServiceProviderFactory.Setup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MsConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace Baubit.DI.Autofac.Test.ServiceProviderFactory
{
    /// <summary>
    /// Unit tests for <see cref="Baubit.DI.Autofac.ServiceProviderFactory"/>
    /// </summary>
    public class Test
    {
        [Fact]
        public void Constructor_WithValidConfig_LoadsAutofacModules()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "modules:0:type", typeof(TestModule).AssemblyQualifiedName },
                { "modules:0:configuration:serviceName", "Test Service from Config" }
            };
            var configuration = new MsConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var factory = new Baubit.DI.Autofac.ServiceProviderFactory(configuration, []);
            var builder = Host.CreateApplicationBuilder();
            var result = factory.UseConfiguredServiceProviderFactory(builder);
            var host = result.Value.Build();
            var service = host.Services.GetRequiredService<ITestService>();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(service);
            Assert.Equal("Test Service from Config", service.Name);
        }

        [Fact]
        public void Constructor_WithComponent_LoadsAutofacModules()
        {
            // Arrange
            var configuration = new MsConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();
            var components = new[] { new TestComponent("Test Service from Component") };

            // Act
            var factory = new Baubit.DI.Autofac.ServiceProviderFactory(configuration, components);
            var builder = Host.CreateApplicationBuilder();
            var result = factory.UseConfiguredServiceProviderFactory(builder);
            var host = result.Value.Build();
            var service = host.Services.GetRequiredService<ITestService>();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(service);
            Assert.Equal("Test Service from Component", service.Name);
        }

        [Fact]
        public void Load_WithAutofacModule_RegistersServicesWithContainerBuilder()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "modules:0:type", typeof(TestModule).AssemblyQualifiedName },
                { "modules:0:configuration:serviceName", "Autofac Test Service" }
            };
            var configuration = new MsConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            var factory = new Baubit.DI.Autofac.ServiceProviderFactory(configuration, []);
            var builder = Host.CreateApplicationBuilder();
            factory.UseConfiguredServiceProviderFactory(builder);
            
            // Act
            using var host = builder.Build();
            var service = host.Services.GetRequiredService<ITestService>();

            // Assert
            Assert.NotNull(service);
            Assert.Equal("Autofac Test Service", service.Name);
        }

        [Fact]
        public void Load_WithMixedModules_RegistersBothAutofacAndStandardDI()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "modules:0:type", typeof(TestModule).AssemblyQualifiedName },
                { "modules:0:configuration:serviceName", "Autofac Module" },
                { "modules:1:type", typeof(StandardDITestModule).AssemblyQualifiedName },
                { "modules:1:configuration:message", "Standard DI Module" }
            };
            var configuration = new MsConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            var factory = new Baubit.DI.Autofac.ServiceProviderFactory(configuration, []);
            var builder = Host.CreateApplicationBuilder();
            factory.UseConfiguredServiceProviderFactory(builder);

            // Act
            using var host = builder.Build();
            var autofacService = host.Services.GetRequiredService<ITestService>();
            var standardService = host.Services.GetRequiredService<IAnotherService>();

            // Assert
            Assert.NotNull(autofacService);
            Assert.Equal("Autofac Module", autofacService.Name);
            Assert.NotNull(standardService);
            Assert.Equal("Standard DI Module", standardService.GetMessage());
        }

        [Fact]
        public void Load_WithMixedComponents_RegistersBothAutofacAndStandardDI()
        {
            // Arrange
            var configuration = new MsConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();
            var components = new Baubit.DI.IComponent[]
            {
                new TestComponent("Autofac Component"),
                new StandardDIComponent("Standard DI Component")
            };

            var factory = new Baubit.DI.Autofac.ServiceProviderFactory(configuration, components);
            var builder = Host.CreateApplicationBuilder();
            factory.UseConfiguredServiceProviderFactory(builder);

            // Act
            using var host = builder.Build();
            var autofacService = host.Services.GetRequiredService<ITestService>();
            var standardService = host.Services.GetRequiredService<IAnotherService>();

            // Assert
            Assert.NotNull(autofacService);
            Assert.Equal("Autofac Component", autofacService.Name);
            Assert.NotNull(standardService);
            Assert.Equal("Standard DI Component", standardService.GetMessage());
        }

        [Fact]
        public void Load_WithNoModules_DoesNothing()
        {
            // Arrange
            var configuration = new MsConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();

            var factory = new Baubit.DI.Autofac.ServiceProviderFactory(configuration, []);
            var builder = Host.CreateApplicationBuilder();
            factory.UseConfiguredServiceProviderFactory(builder);

            // Act
            using var host = builder.Build();

            // Assert - just verify host builds successfully with no modules
            Assert.NotNull(host);
            Assert.NotNull(host.Services);
        }

        [Fact]
        public void UseConfiguredServiceProviderFactory_ReturnsSuccessResult()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "modules:0:type", typeof(TestModule).AssemblyQualifiedName }
            };
            var configuration = new MsConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            var factory = new Baubit.DI.Autofac.ServiceProviderFactory(configuration, []);
            var builder = Host.CreateApplicationBuilder();

            // Act
            var result = factory.UseConfiguredServiceProviderFactory(builder);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Same(builder, result.Value);
        }

        [Fact]
        public void Modules_ReturnsAllLoadedModules()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "modules:0:type", typeof(TestModule).AssemblyQualifiedName },
                { "modules:0:configuration:serviceName", "Module 1" }
            };
            var configuration = new MsConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();
            var components = new[] { new TestComponent("Module 2") };

            // Act
            var factory = new Baubit.DI.Autofac.ServiceProviderFactory(configuration, components);

            // Assert
            Assert.Equal(2, factory.Modules.Count);
            Assert.All(factory.Modules, module => Assert.IsAssignableFrom<Baubit.DI.IModule>(module));
        }

        [Fact]
        public void Constructor_WithHybridLoading_LoadsComponentsFirstThenConfig()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                { "modules:0:type", typeof(StandardDITestModule).AssemblyQualifiedName },
                { "modules:0:configuration:message", "From Config" }
            };
            var configuration = new MsConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();
            var components = new Baubit.DI.IComponent[] { new TestComponent("From Component") };

            // Act
            var factory = new Baubit.DI.Autofac.ServiceProviderFactory(configuration, components);
            var builder = Host.CreateApplicationBuilder();
            factory.UseConfiguredServiceProviderFactory(builder);
            using var host = builder.Build();

            // Assert - verify both services are registered
            var autofacService = host.Services.GetRequiredService<ITestService>();
            var standardService = host.Services.GetRequiredService<IAnotherService>();
            Assert.Equal("From Component", autofacService.Name);
            Assert.Equal("From Config", standardService.GetMessage());
        }
    }
}
