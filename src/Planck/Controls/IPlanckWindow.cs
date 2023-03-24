using Microsoft.Web.WebView2.Core;

namespace Planck.Controls
{
  public interface IPlanckWindow
  {
    public enum WindowState
    {
      Maximized = System.Windows.WindowState.Maximized,
      Minimized = System.Windows.WindowState.Minimized,
      Normal = System.Windows.WindowState.Normal,
    }

    CoreWebView2 CoreWebView2 { get; }
    string Title { get; set; }
    double Width { get; set; }
    double Height { get; set; }

    internal bool HasCompletedBootstrap { get; set; }
    internal event EventHandler BootstrapCompleted;

    void Show();
    void Hide();
    void Close();

    void SetWindowState(WindowState state);
    WindowState GetWindowState();

    void ShowSplashscreen();
    void CloseSplashscreen();
  }
}
