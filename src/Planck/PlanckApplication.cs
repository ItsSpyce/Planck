using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Planck.Configuration;
using Planck.Controls;
using Planck.Extensions;
using Planck.Resources;
using Planck.Utilities;

namespace Planck
{
  public static class PlanckApplication
  {
    public static IHostBuilder CreateHost(ResourceMode resourceMode, PlanckConfiguration? configuration = null) =>
      CreateHost(Array.Empty<string>(), resourceMode, configuration);

    public static IHostBuilder CreateHost(string[] args, ResourceMode resourceMode, PlanckConfiguration? configuration = null)
    {
      var host = Host.CreateDefaultBuilder(args);

      host
        .UsePlanckConfiguration(new()
        {
          {"ProjectRoot", ProcessUtilities.GetProjectLocation() }
        })
        .UsePlanck(resourceMode, configuration)
        .UsePlanckModules();
      return host;
    }
      
    public static Task<IPlanckWindow> StartAsync(PlanckConfiguration? configuration = null)
    {
      var resourceMode =
#if DEBUG
        ResourceMode.Local;
#else
        ResourceMode.Embedded;
#endif
      return StartAsync(resourceMode, configuration);
    }

    public static async Task<IPlanckWindow> StartAsync(ResourceMode resourceMode, PlanckConfiguration? configuration = null)
    {
      var host = CreateHost(resourceMode, configuration)
        .Build();
      configuration ??= host.Services.GetService<IOptions<PlanckConfiguration>>()!.Value;
      if (!string.IsNullOrEmpty(configuration.Splashscreen))
      {
        var splashscreen = host.Services.GetService<IPlanckSplashscreen>();
        splashscreen?.Show();
      }

      await host.StartAsync();

      var window = host.Services.GetService<IPlanckWindow>()!;
      window.Show();
      return window;
    }
  }
}
