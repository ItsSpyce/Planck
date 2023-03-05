using Microsoft.Web.WebView2.Core;

namespace Planck.Controls
{
  public interface IPlanckWindow
  {
    CoreWebView2 CoreWebView2 { get; }
    string Title { get; set; }
    double Width { get; set; }
    double Height { get; set; }

    internal bool HasCompletedBootstrap { get; set; }
    internal event EventHandler BootstrapCompleted;

    void Show();
    void Hide();
    void Close();

    void ShowSplashscreen();
    void CloseSplashscreen();
  }
}
