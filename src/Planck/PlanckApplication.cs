using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Planck.Configuration;
using Planck.Controls;
using Planck.Extensions;
using Planck.MacroConfig.Extensions;
using Planck.Utilities;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;

namespace Planck
{
  public static class PlanckApplication
  {
    public static IHostBuilder CreateHost(PlanckConfiguration? configuration = null) =>
      CreateHost(Array.Empty<string>(), configuration);

    public static IHostBuilder CreateHost(string[] args, PlanckConfiguration? configuration = null)
    {
      var host = Host.CreateDefaultBuilder(args);

      host.UsePlanckConfiguration(new()
      {
        {"ProjectRoot", GetProjectLocation() }
      });
      host.UsePlanck(configuration);
      return host;
    }
      

    public static async Task<IPlanckWindow> StartAsync(PlanckConfiguration? configuration = null)
    {
      var host = CreateHost(configuration)
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

    static string GetProjectLocation()
    {
      var currentDir = AppDomain.CurrentDomain.BaseDirectory;
      while (currentDir != null && Path.GetDirectoryName(currentDir) != currentDir)
      {
        if (Directory.GetFiles(currentDir).Any(file => file.EndsWith(".csproj")))
        {
          return currentDir;
        }
        currentDir = Path.GetDirectoryName(currentDir);
      }
      throw new FileNotFoundException("No csproj file found");
    }
  }
}
