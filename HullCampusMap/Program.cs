using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HullCampusMap;
using HullCampusMap.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        ConfigureServices(builder.Services, builder.HostEnvironment.BaseAddress);
        ConfigureRootComponents(builder);

        await BuildAndRunApplication(builder);
    }

    private static void ConfigureServices(IServiceCollection services, string baseAddress)
    {
        // Register HttpClient with proper configuration
        services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(baseAddress),
            DefaultRequestHeaders = { 
                // Add any default headers if needed
                // {"User-Agent", "HullCampusMap/1.0"}
            }
        });

        // Register application services
        services.AddScoped<SvgParserService>();
        services.AddScoped<RoomInterop>();

        // Add logging if needed
        // services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Debug));
    }

    private static void ConfigureRootComponents(WebAssemblyHostBuilder builder)
    {
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
    }

    private static async Task BuildAndRunApplication(WebAssemblyHostBuilder builder)
    {
        try
        {
            var host = builder.Build();

            // Uncomment to verify services (development only)
            // VerifyServices(host.Services);

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            // This will be caught by Blazor's error UI
            Console.Error.WriteLine($"Application startup failed: {ex}");
            throw;
        }
    }

    // Helper method to verify DI services (for debugging)
    private static void VerifyServices(IServiceProvider services)
    {
#if DEBUG
        using var scope = services.CreateScope();
        var httpClient = scope.ServiceProvider.GetService<HttpClient>();
        var svgParser = scope.ServiceProvider.GetService<SvgParserService>();
        var roomInterop = scope.ServiceProvider.GetService<RoomInterop>();

        if (httpClient == null || svgParser == null || roomInterop == null)
        {
            throw new InvalidOperationException("Critical services not registered");
        }
#endif
    }
}