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
    async void OnStartup(object sender, StartupEventArgs args)
    {
      var host = await PlanckApplication.StartAsync();
    }
  }
}
