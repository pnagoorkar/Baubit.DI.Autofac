// ============================================================================
// Pattern 3: Hybrid (appsettings.json + IComponent)
// ============================================================================
// Combines BOTH configuration-based and code-based module loading.
// Components from code are loaded first, then modules from appsettings.json.
// This is the most flexible pattern.
// ============================================================================

using Baubit.DI;
using Baubit.DI.Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SampleConsoleApp;

public static class ModulesLoadedFromAppsettingsAndExplicitlyGivenComponent
{
    public static async Task RunAsync()
    {
        // Build host with modules from BOTH appsettings.json AND code
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = Array.Empty<string>(),
            ContentRootPath = AppContext.BaseDirectory
        });
        
        // Explicitly specify the Autofac service provider factory
        builder.UseConfiguredServiceProviderFactory<HostApplicationBuilder, Baubit.DI.Autofac.ServiceProviderFactory>(
            componentsFactory: () => [new CodeDataComponent("Data from code component")]
        );
        
        using var host = builder.Build();
        
        // IGreetingService comes from appsettings.json (GreetingModule)
        var greetingService = host.Services.GetRequiredService<IGreetingService>();
        Console.WriteLine($"  From appsettings.json: {greetingService.GetGreeting()}");
        
        // IDataService comes from code (CodeDataComponent)
        var dataService = host.Services.GetRequiredService<IDataService>();
        Console.WriteLine($"  From code component: {dataService.GetData()}");
        
        await Task.CompletedTask;
    }
}
