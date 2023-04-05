﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Planck.Commands;
using Planck.Configuration;
using Planck.Controls;
using Planck.HttpClients;
using Planck.IO;
using Planck.MacroConfig.Extensions;
using Planck.Messages;
using Planck.Modules;
using Planck.Modules.Internal;
using Planck.Resources;
using Planck.TypeConverter;
using System.IO;
using System.Reflection;

namespace Planck.Extensions
{
  public static class InjectionExtensions
  {
    public static IHostBuilder UsePlanck(this IHostBuilder host, ResourceMode resourceMode, PlanckConfiguration? configuration = null) =>
      host
        .UseDefaultCommands()
        .ConfigureServices((context, services) =>
        {
          var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
          services.AddHostedService<PlanckSplashscreenService>();

          // for reference in the components
          services.AddSingleton(assembly);

          // Add HTTP clients
          services.AddHttpClient(nameof(PlanckHttpClient), (services, client) =>
            {
              client.BaseAddress = new Uri(services.GetService<IOptions<PlanckConfiguration>>()!.Value.DevUrl!);
              client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en-US;q=0.8,en;q=0.6,ru;q=0.4");
            });

          switch (resourceMode)
          {
            case ResourceMode.Local:
              services.AddSingleton<IResourceService, LocalResourceService>();
#if DEBUG
              services.AddHostedService<PlanckDevServerService>();
#endif
              break;
            case ResourceMode.Embedded:
              services.AddSingleton<IResourceService, EmbeddedResourceService>();
              break;
          }

          // Add WPF controls
          services
            // Add WinForms controls
            .AddScoped<Controls.WinForms.PlanckWindow>()
            // .AddScoped<Controls.WinForms.PlanckSplashscreen>()
            .AddScoped<Controls.Wpf.PlanckWindow>()
            .AddScoped<Controls.Wpf.PlanckSplashscreen>()
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

          // Add IO stuff
          services
            .AddSingleton<IStreamPool, StreamPool>()
            .AddScoped<IPropTypeConverter, StreamPropTypeConverter>()
            .AddScoped<StreamPropTypeConverter>();
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

    /// <summary>
    ///   Configures the app to use the Planck object within loaded configurations.
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public static IHostBuilder UsePlanckConfiguration(this IHostBuilder host, Dictionary<string, string> macros) =>
      host
        .ConfigureServices((context, services) =>
          services.Configure<PlanckConfiguration>(
            context.Configuration.GetSection(PlanckConfiguration.PlanckKey)))
        .ConfigureAppConfiguration(config =>
          config.BindMacros(macros));

    public static IHostBuilder UsePlanckModules(this IHostBuilder host) =>
      host.ConfigureServices((services) =>
        services
          .AddSingleton<IModuleService, ModuleService>(serviceProvider =>
          {
            var moduleService = new ModuleService(services);
            moduleService
              .AddModule<FileSystemModule>("fs")
              .AddModule<ClipboardModule>("clipboard")
              .AddModule<AppModule>("app");
            return moduleService;
          }));

    public static IServiceCollection AddHandlers<T>(this IServiceCollection services)
    {
      var methods = typeof(T).GetMethods();
      services.AddSingleton<IMessageService, MessageService>((services) =>
      {
        var messageService = new MessageService(
          services.GetService<IServiceProvider>()!,
          services.GetService<ILogger<MessageService>>()!);
        return messageService
          .AddHandlersFromType<T>();
      });

      return services;
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
