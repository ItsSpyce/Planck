using Planck.Controls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Planck.Demo.SelfHosted
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private IPlanckWindow _planck;

    async void OnStartup(object sender, StartupEventArgs args)
    {
      _planck = await PlanckApplication.StartAsync();
      await Task.Delay(3000);
      _planck.CloseSplashscreen();
    }
  }
}
