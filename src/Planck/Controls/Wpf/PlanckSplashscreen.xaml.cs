using Microsoft.Extensions.Options;
using Planck.Configuration;
using Planck.Utilities;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
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

    public PlanckSplashscreen(IOptions<PlanckConfiguration> configuration, Assembly assembly)
    {
      // TODO: add height and width restrictions, don't want to take up the entire screen
      InitializeComponent();
      Source = configuration.Value.Splashscreen;
      var bitmap = GetBitmapFromResources(assembly, Source);
      if (bitmap != null)
      {
        Content = new Image
        {
          Source = bitmap,
        };
      }
    }

    private static BitmapImage? GetBitmapFromResources(Assembly assembly, string? path)
    {
      if (string.IsNullOrEmpty(path))
      {
        return null;
      }
      var resourceName = UrlUtilities.GetResourceName(path);
      var resourceStream = assembly.GetManifestResourceStream(resourceName);
      if (resourceStream != null)
      {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = assembly.GetManifestResourceStream(resourceName);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
      }
      return null;
    }
  }
}
