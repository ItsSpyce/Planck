using Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Planck.Commands;
using Planck.Configuration;
using Planck.Controls;
using Planck.HttpClients;
using Planck.MacroConfig.Extensions;
using Planck.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Planck.Extensions
{
  public static class InjectionExtensions
  {
    public static IHostBuilder UsePlanck(this IHostBuilder host, PlanckConfiguration? configuration = null) =>
      host
        .UseDefaultCommands()
        .ConfigureAppConfiguration((context, config) =>
          config
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
            .AddJsonFile("appsettings.local.json", true, true)
            .AddJsonFile($"appsettings.${context.HostingEnvironment.EnvironmentName}.local.json", true, true)
            //.AddJsonStream(new MemoryStream(configuration != null
            //  ? JsonSerializer.SerializeToUtf8Bytes(configuration)
            //  : Encoding.UTF8.GetBytes("{}"), true))
            .BindMacros(new()
            {
              { "ProjectRoot", "F:\\dev\\Planck\\src\\Planck.Demo.SelfHosted" },
              { "BuildDirectory", "" },
              { "ProjectVersion", "" }
            })
        )
        .ConfigureServices((context, services) =>
        {
          services
            .AddHostedService<PlanckHostService>();
          // Add HTTP clients
          //services
          //  .AddHttpClient(nameof(PlanckHttpClient), (services, client) =>
          //  {
          //    client.BaseAddress = new Uri(services.GetService<IOptions<PlanckConfiguration>>().Value.DevUrl);
          //    client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en-US;q=0.8,en;q=0.6,ru;q=0.4");
          //  });

          services
            // TODO: switch to embedded based on load type
            .AddSingleton<IResourceService, PackagedResourceService>();

          // Add WPF controls
          services
            .AddScoped<Controls.Wpf.PlanckSplashscreen>()
            .AddScoped<Controls.Wpf.PlanckWindow>()
            // Add WinForms controls
            .AddScoped<Controls.WinForms.PlanckWindow>()
            .AddScoped<IPlanckWindow>(services =>
              services.GetService<IOptions<PlanckConfiguration>>()!.Value.UseWpf
                ? services.GetService<Controls.Wpf.PlanckWindow>()!
                : services.GetService<Controls.WinForms.PlanckWindow>()!
            )
            .AddScoped<IPlanckSplashscreen>(services =>
              services.GetService<IOptions<PlanckConfiguration>>()!.Value.UseWpf
                ? services.GetService<Controls.Wpf.PlanckSplashscreen>()!
                // TODO: add WinForms splashscreen
                : null!
            );

          services.Configure<PlanckConfiguration>(context.Configuration.GetSection("Planck"));
        });


    /// <summary>
    ///   Registers the default command service and internal commands to the host.
    /// </summary>
    /// <remarks>
    ///   This is called from <see cref="UsePlanck(IHostBuilder, PlanckConfiguration?)"/> and not necessary if already called.
    /// </remarks>
    /// <param name="host"></param>
    /// <returns></returns>
    public static IHostBuilder UseDefaultCommands(this IHostBuilder host) =>
      host.ConfigureServices((context, services) =>
        services
          .AddHandlers<PlanckHandlers>()
      );

    public static IServiceCollection AddHandlers<T>(this IServiceCollection services)
    {
      var methods = typeof(T).GetMethods();
      // idea here is to register an ICommandHandlerService<T> that is initialized
      // with the methods to register
      services.AddSingleton<ICommandHandlerService, CommandHandlerService>((services) =>
      {
        var commandHandlerService = new CommandHandlerService(services.GetService<IServiceProvider>());
        commandHandlerService.BuildFromType<T>();
        return commandHandlerService;
      });

      return services;
    }
  }
}
