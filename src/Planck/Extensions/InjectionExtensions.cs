using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Web.WebView2.Core;
using Planck.Commands;
using Planck.Configuration;
using Planck.Controls;
using Planck.HttpClients;
using Planck.MacroConfig.Extensions;
using Planck.Resources;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Resources;

namespace Planck.Extensions
{
  public static class InjectionExtensions
  {
    public static IHostBuilder UsePlanck(this IHostBuilder host, PlanckConfiguration? configuration = null) =>
      host
        .UseDefaultCommands()
        .ConfigureServices((context, services) =>
        {
          var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

          // for reference in the components
          services.AddSingleton(assembly);

          // Add HTTP clients
          services.AddHttpClient(nameof(PlanckHttpClient), (services, client) =>
            {
              client.BaseAddress = new Uri(services.GetService<IOptions<PlanckConfiguration>>()!.Value.DevUrl!);
              client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en-US;q=0.8,en;q=0.6,ru;q=0.4");
            });


          services
            // TODO: switch to embedded based on load type
#if !DEBUG
            .AddSingleton<IResourceService, LocalResourceService>()
            .AddHostedService<PlanckDevServerService>();
#else
            .AddSingleton<IResourceService, EmbeddedResourceService>();
#endif
          // add environment
          services
            .AddSingleton(new CoreWebView2EnvironmentOptions("--disable-web-security"));

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

    public static IServiceCollection AddHandlers<T>(this IServiceCollection services)
    {
      var methods = typeof(T).GetMethods();
      // idea here is to register an ICommandHandlerService<T> that is initialized
      // with the methods to register
      services.AddSingleton<ICommandHandlerService, CommandHandlerService>((services) =>
      {
        var commandHandlerService = new CommandHandlerService(
          services.GetService<IServiceProvider>()!,
          services.GetService<ILogger<CommandHandlerService>>()!);
        commandHandlerService.BuildFromType<T>();
        return commandHandlerService;
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
