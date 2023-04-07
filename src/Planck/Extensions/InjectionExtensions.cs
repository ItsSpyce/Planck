using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Planck.Configuration;
using Planck.Controllers;
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
    public static IHostBuilder UsePlanck(this IHostBuilder host, ResourceMode resourceMode, PlanckConfiguration configuration) =>
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
              client.BaseAddress = new Uri(services.GetService<PlanckConfiguration>()!.DevUrl!);
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

          // Core services
          services
            .AddSingleton<IModuleService, ModuleService>()
            .AddSingleton<IMessageService, MessageService>();

          // add PlanckConfiguration
          services
            .AddSingleton(configuration);

          // Add controls
          if (configuration.UseWpf)
          {
            services
              .AddScoped<IPlanckWindow, Controls.Wpf.PlanckWindow>()
              .AddScoped<IPlanckSplashscreen, Controls.Wpf.PlanckSplashscreen>();
          }
          else
          {
            services
              .AddScoped<IPlanckWindow, Controls.WinForms.PlanckWindow>();
            // .AddScoped<IPlanckSplashscreen, Controls.WinForms.PlanckSplashscreen>();
          }

          // Add IO stuff
          services
            .AddSingleton<IStreamPool, StreamPool>()
            .AddTypeConverter<StreamPropTypeConverter>()
            .AddTypeConverter<ModulePropertyTypeConverter>();
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
          .AddMessageController<WindowController>()
          .AddMessageController<StreamController>()
          .AddMessageController<ModuleController>()
      );

    public static IHostBuilder UsePlanckModules(this IHostBuilder host) =>
      host.ConfigureServices((services) =>
        services
          .AddModule<FileSystemModule>("fs")
          .AddModule<AppModule>("app")
          .AddModule<ClipboardModule>("clipboard")
      );

    public static IServiceCollection AddTypeConverter<T>(this IServiceCollection services) where T : class, IPropTypeConverter =>
      services
        .AddScoped<IPropTypeConverter, T>()
        .AddScoped<T>();

    /// <summary>
    ///   Adds the message controller to Planck
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMessageController<T>(this IServiceCollection services) where T : MessageController =>
      services
        .AddScoped<MessageController, T>()
        .AddScoped<T>();

    /// <summary>
    ///   Adds the message controller to Planck
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <param name="implementationFactory"></param>
    /// <returns></returns>
    public static IServiceCollection AddMessageController<T>(this IServiceCollection services, Func<IServiceProvider, T> implementationFactory) where T : MessageController =>
      services
        .AddScoped<MessageController, T>(implementationFactory)
        .AddScoped(implementationFactory);

    public static IServiceCollection AddModule<T>(this IServiceCollection services, string name) where T : Modules.Module =>
      services
        .AddSingleton<ModuleService.IModuleDefinition, ModuleService.ModuleDefinition<T>>((_) => new ModuleService.ModuleDefinition<T>(name))
        .AddScoped<T>();

    public static IServiceCollection AddModule<T>(this IServiceCollection services, string name, Func<IServiceProvider, T> implementationFactory) where T : Modules.Module =>
      services
        .AddSingleton<ModuleService.IModuleDefinition, ModuleService.ModuleDefinition<T>>((_) => new ModuleService.ModuleDefinition<T>(name))
        .AddScoped(implementationFactory);


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
