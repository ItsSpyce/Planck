using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Planck.Configuration;
using Planck.Extensions;
using Planck.Resources;
using Planck.Utilities;

namespace Planck
{
    public static class PlanckApplication
  {
    public static IHostBuilder CreateHost(PlanckConfiguration configuration) =>
#if DEBUG
      CreateHost(ResourceMode.Local, configuration);
#else
      CreateHost(ResourceMode.Embedded, configuration);
#endif

    public static IHostBuilder CreateHost(ResourceMode resourceMode, PlanckConfiguration configuration) =>
      CreateHost(Array.Empty<string>(), resourceMode, configuration);

    public static IHostBuilder CreateHost(string[] args, ResourceMode resourceMode, PlanckConfiguration configuration)
    {
      var host = Host.CreateDefaultBuilder(args);

      host
        .UsePlanck(resourceMode, configuration)
        .UsePlanckModules();
      return host;
    }

    public static Task<IPlanckWindow> StartAsync(PlanckConfiguration configuration)
    {
      var resourceMode =
#if DEBUG
        ResourceMode.Local;
#else
        ResourceMode.Embedded;
#endif
      return StartAsync(resourceMode, configuration);
    }

    public static Task<IPlanckWindow> StartAsync(ResourceMode resourceMode, PlanckConfiguration configuration) =>
      StartAsync(resourceMode, (services) => { }, configuration);

    public static Task<IPlanckWindow> StartAsync(
      Action<IServiceCollection> configureServicesDelegate,
      PlanckConfiguration configuration)
    {
      var resourceMode =
#if DEBUG
        ResourceMode.Local;
#else
        ResourceMode.Embedded;
#endif
      return StartAsync(resourceMode, configureServicesDelegate, configuration);
    }

    public static async Task<IPlanckWindow> StartAsync(
      ResourceMode resourceMode,
      Action<IServiceCollection> configureServicesDelegate,
      PlanckConfiguration configuration)
    {
      var host = CreateHost(resourceMode, configuration)
        .ConfigureServices(configureServicesDelegate)
        .Build();

      await host.StartAsync();

      var window = host.Services.GetService<IPlanckWindow>()!;
      window.Show();
      return window;
    }
  }
}
