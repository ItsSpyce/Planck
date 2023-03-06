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
    IPlanckWindow _planck;

    async void OnStartup(object sender, StartupEventArgs args)
    {
      _planck = await PlanckApplication.StartAsync(ResourceMode.Embedded);
    }
  }
}
