// ============================================================================
// Baubit.DI.Autofac Sample Console Application
// ============================================================================
// This application demonstrates three patterns for loading DI modules with Autofac:
//   Pattern 1: From appsettings.json only
//   Pattern 2: From code only (IComponent)
//   Pattern 3: Hybrid - both appsettings.json AND code
//   Pattern 4: Mixed - Autofac modules + standard DI modules
// ============================================================================

using Autofac;
using Baubit.DI;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleConsoleApp;

// Register modules from this assembly with the secure module registry
// This uses the source-generated Register() method
SampleModuleRegistry.Register();

Console.WriteLine("=== Baubit.DI.Autofac Sample Application ===\n");

// Run each pattern sequentially so output is clear
// Start with Pattern 2 since it's simplest (no appsettings dependency)
Console.WriteLine("--- Pattern 2: Modules from Code (IComponent) ---");
await ModulesLoadedFromExplicitlyGivenComponent.RunAsync();

Console.WriteLine("\n--- Pattern 1: Modules from appsettings.json ---");
await ModulesLoadedFromAppsettings.RunAsync();

Console.WriteLine("\n--- Pattern 3: Hybrid (appsettings.json + IComponent) ---");
await ModulesLoadedFromAppsettingsAndExplicitlyGivenComponent.RunAsync();

Console.WriteLine("\n--- Pattern 4: Mixed (Autofac + Standard DI Modules) ---");
await MixedAutofacAndStandardDIModules.RunAsync();

Console.WriteLine("\n=== All patterns completed ===");

// ============================================================================
// SERVICES
// ============================================================================

/// <summary>
/// Interface for the greeting service - allows verification of which module registered it.
/// </summary>
public interface IGreetingService
{
    string GetGreeting();
}

/// <summary>
/// A simple greeting service that returns a configured message.
/// </summary>
public class GreetingService : IGreetingService
{
    private readonly string _message;

    public GreetingService(string message)
    {
        _message = message;
    }

    public string GetGreeting() => _message;
}

/// <summary>
/// Interface for a data service to demonstrate standard DI module usage.
/// </summary>
public interface IDataService
{
    string GetData();
}

/// <summary>
/// A simple data service implementation.
/// </summary>
public class DataService : IDataService
{
    private readonly string _data;

    public DataService(string data)
    {
        _data = data;
    }

    public string GetData() => _data;
}

// ============================================================================
// AUTOFAC MODULE CONFIGURATION AND DEFINITION
// ============================================================================

/// <summary>
/// Configuration for Autofac GreetingModule.
/// - When loaded from appsettings.json, properties are bound automatically
/// - When created in code, properties are set directly
/// </summary>
public class GreetingModuleConfiguration : Baubit.DI.Autofac.Configuration
{
    public string Message { get; set; } = "Default greeting";
}

/// <summary>
/// A module that registers IGreetingService using Autofac ContainerBuilder.
/// Demonstrates:
/// - [BaubitModule] attribute for secure module loading
/// - Two constructors (IConfiguration vs typed configuration)
/// - Service registration in Load(ContainerBuilder)
/// - Using Autofac's registration API with SingleInstance lifetime
/// </summary>
[BaubitModule("greeting")]
public class GreetingModule : Baubit.DI.Autofac.Module<GreetingModuleConfiguration>
{
    // Constructor for loading from appsettings.json
    public GreetingModule(IConfiguration configuration) : base(configuration) { }

    // Constructor for programmatic creation
    public GreetingModule(GreetingModuleConfiguration configuration, List<Baubit.DI.IModule>? nestedModules = null)
        : base(configuration, nestedModules) { }

    public override void Load(ContainerBuilder containerBuilder)
    {
        // Register using Autofac's ContainerBuilder
        containerBuilder.RegisterInstance(new GreetingService(Configuration.Message))
                       .As<IGreetingService>()
                       .SingleInstance();
        base.Load(containerBuilder);
    }
}

// ============================================================================
// STANDARD DI MODULE (Non-Autofac)
// ============================================================================

/// <summary>
/// Configuration for standard DI DataModule.
/// </summary>
public class DataModuleConfiguration : Baubit.DI.Configuration
{
    public string Data { get; set; } = "Default data";
}

/// <summary>
/// A module that uses standard IServiceCollection instead of Autofac.
/// This demonstrates mixed module support where some modules use Autofac
/// and others use standard Microsoft DI.
/// </summary>
public class DataModule : Baubit.DI.Module<DataModuleConfiguration>
{
    public DataModule(IConfiguration configuration) : base(configuration) { }

    public DataModule(DataModuleConfiguration configuration, List<Baubit.DI.IModule>? nestedModules = null)
        : base(configuration, nestedModules) { }

    public override void Load(IServiceCollection services)
    {
        // Register using standard IServiceCollection
        services.AddSingleton<IDataService>(new DataService(Configuration.Data));
        base.Load(services);
    }
}

// ============================================================================
// COMPONENT DEFINITIONS
// ============================================================================

/// <summary>
/// A component that creates Autofac GreetingModule in code with a custom message.
/// </summary>
public class CodeGreetingComponent : Component
{
    private readonly string _message;

    public CodeGreetingComponent(string message)
    {
        _message = message;
    }

    protected override Result<ComponentBuilder> Build(ComponentBuilder componentBuilder)
    {
        return componentBuilder.WithModule<GreetingModule, GreetingModuleConfiguration>(config =>
        {
            config.Message = _message;
        }, config => new GreetingModule(config));
    }
}

/// <summary>
/// A component that creates standard DI DataModule in code.
/// </summary>
public class CodeDataComponent : Component
{
    private readonly string _data;

    public CodeDataComponent(string data)
    {
        _data = data;
    }

    protected override Result<ComponentBuilder> Build(ComponentBuilder componentBuilder)
    {
        return componentBuilder.WithModule<DataModule, DataModuleConfiguration>(config =>
        {
            config.Data = _data;
        }, config => new DataModule(config));
    }
}

