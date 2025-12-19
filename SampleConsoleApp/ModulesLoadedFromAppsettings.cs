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
        // Must explicitly specify Autofac factory type for security
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = Array.Empty<string>(),
            ContentRootPath = AppContext.BaseDirectory
        });
        
        // Explicitly specify the Autofac service provider factory
        builder.UseConfiguredServiceProviderFactory<HostApplicationBuilder, Baubit.DI.Autofac.ServiceProviderFactory>();
        
        using var host = builder.Build();
        
        // Verify the module was loaded from appsettings.json
        var greetingService = host.Services.GetRequiredService<IGreetingService>();
        Console.WriteLine($"  {greetingService.GetGreeting()}");
        
        await Task.CompletedTask;
    }
}
