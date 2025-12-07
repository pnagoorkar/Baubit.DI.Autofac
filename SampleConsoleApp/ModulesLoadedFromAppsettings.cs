// ============================================================================
// Pattern 1: Modules from appsettings.json
// ============================================================================
// All modules are defined in configuration - no code-based modules.
// Module types, configurations, and nested modules come from JSON.
// ============================================================================

using Baubit.DI;
using Baubit.DI.Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SampleConsoleApp;

public static class ModulesLoadedFromAppsettings
{
    public static async Task RunAsync()
    {
        // Build host with modules from appsettings.json only
        // For Autofac, we need to explicitly specify the factory type in configuration
        var builder = Host.CreateApplicationBuilder();
        
        // Since appsettings.json already specifies the serviceProviderFactoryType,
        // the framework will automatically use the Autofac factory
        builder.UseConfiguredServiceProviderFactory();
        
        using var host = builder.Build();
        
        // Verify the module was loaded from appsettings.json
        var greetingService = host.Services.GetRequiredService<IGreetingService>();
        Console.WriteLine($"  {greetingService.GetGreeting()}");
        
        await Task.CompletedTask;
    }
}
