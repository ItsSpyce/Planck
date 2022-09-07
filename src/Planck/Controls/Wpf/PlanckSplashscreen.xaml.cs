using Microsoft.Extensions.Options;
using Planck.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;

namespace Planck.Controls.Wpf
{
  /// <summary>
  /// Interaction logic for PlanckSplashScreen.xaml
  /// </summary>
  public partial class PlanckSplashscreen : Window, IPlanckSplashscreen
  {
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
      "Source",
      typeof(string),
      typeof(PlanckSplashscreen));

    public string Source
    {
      get => (string)GetValue(SourceProperty);
      set => SetValue(SourceProperty, value);
    }

    public PlanckSplashscreen(IOptions<PlanckConfiguration> configuration)
    {
      // TODO: add height and width restrictions, don't want to take up the entire screen
      InitializeComponent();
      Source = configuration.Value.Splashscreen;
      Content = new Image
      {
        Source = new BitmapImage(new Uri(Source)),
      };
    }
  }
}
