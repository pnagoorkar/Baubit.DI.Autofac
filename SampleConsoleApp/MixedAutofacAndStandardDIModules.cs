// ============================================================================
// Pattern 4: Mixed (Autofac + Standard DI Modules)
// ============================================================================
// Demonstrates using BOTH Autofac modules and standard DI modules together.
// This shows the unique capability of Baubit.DI.Autofac to support both
// module types in the same application.
// ============================================================================

using Baubit.DI;
using Baubit.DI.Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SampleConsoleApp;

public static class MixedAutofacAndStandardDIModules
{
    public static async Task RunAsync()
    {
        // Build host with BOTH Autofac and standard DI modules
        // - CodeGreetingComponent uses Autofac's ContainerBuilder
        // - CodeDataComponent uses standard IServiceCollection
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        
        // Explicitly specify the Autofac service provider factory
        builder.UseConfiguredServiceProviderFactory<HostApplicationBuilder, Baubit.DI.Autofac.ServiceProviderFactory>(
            componentsFactory: () =>
            [
                new CodeGreetingComponent("Hello from Autofac module!"),
                new CodeDataComponent("Data from standard DI module!")
            ]
        );
        
        using var host = builder.Build();
        
        // Both services work together seamlessly
        var greetingService = host.Services.GetRequiredService<IGreetingService>();
        var dataService = host.Services.GetRequiredService<IDataService>();
        
        Console.WriteLine($"  Autofac module: {greetingService.GetGreeting()}");
        Console.WriteLine($"  Standard DI module: {dataService.GetData()}");
        
        await Task.CompletedTask;
    }
}
