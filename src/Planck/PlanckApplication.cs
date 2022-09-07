using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Planck.Commands;
using Planck.Configuration;
using Planck.Controls;
using Planck.Extensions;
using Planck.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Planck
{
  public static class PlanckApplication
  {
    public static IHostBuilder CreateHost(PlanckConfiguration? configuration = null) =>
      CreateHost(Array.Empty<string>(), configuration);

    public static IHostBuilder CreateHost(string[] args, PlanckConfiguration? configuration = null) =>
      Host.CreateDefaultBuilder(args)
        .UsePlanck(configuration);

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
  }
}
