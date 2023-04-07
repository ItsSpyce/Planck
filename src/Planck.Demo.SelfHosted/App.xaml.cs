using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Planck.Controls;
using Planck.Resources;
using System.Windows;

namespace Planck.Demo.SelfHosted
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    IHost? _host;

    async void OnStartup(object sender, StartupEventArgs args)
    {
      _host = PlanckApplication.CreateHost(new()
      {
        SslOnly = true,
        OpenLinksIn = Configuration.PlanckConfiguration.LinkLaunchRule.MachineDefault,
        Splashscreen = "splashscreen.jpg",
        AllowExternalMessages = true,
        ClientDirectory = "Client",
        BuildDirectory = "Client\\dist",
        UseWpf = true,
        WaitForShowCommand = true,
        DevUrl = "http://localhost:3000/",
        Entry = "Client\\dist\\index.html",
        DevCommand = "dev",
        BuildCommand = "build",
      })
        .Build();
      var window = _host.Services.GetService<IPlanckWindow>();
      window.Show();

      await _host.StartAsync();
    }

    async void OnExit(object sender, ExitEventArgs args)
    {
      if (_host is not null)
      {
        await _host.StopAsync();
      }
    }
  }
}
