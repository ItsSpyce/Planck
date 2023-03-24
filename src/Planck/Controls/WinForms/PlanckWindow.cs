using Microsoft.Web.WebView2.Core;

namespace Planck.Controls.WinForms
{
  internal class PlanckWindow : Form, IPlanckWindow
  {
    public CoreWebView2 CoreWebView2 => throw new NotImplementedException();
    public event EventHandler BootstrapCompleted;

    public string Title { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public new double Width
    {
      get => base.Width;
      set => base.Width = (int)value;
    }

    public new double Height
    {
      get => base.Height;
      set => base.Height = (int)value;
    }

    CoreWebView2 IPlanckWindow.CoreWebView2 => throw new NotImplementedException();

    bool IPlanckWindow.HasCompletedBootstrap { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void CloseSplashscreen()
    {
      throw new NotImplementedException();
    }

    public void ShowSplashscreen()
    {
      throw new NotImplementedException();
    }

    void IPlanckWindow.Close()
    {
      throw new NotImplementedException();
    }

    void IPlanckWindow.CloseSplashscreen()
    {
      throw new NotImplementedException();
    }

    void IPlanckWindow.Hide()
    {
      throw new NotImplementedException();
    }

    void IPlanckWindow.Show()
    {
      throw new NotImplementedException();
    }

    void IPlanckWindow.ShowSplashscreen()
    {
      throw new NotImplementedException();
    }

    public Task EnsureCoreWebView2Async()
    {
      throw new NotImplementedException();
    }

    public void SetWindowState(IPlanckWindow.WindowState state)
    {
      throw new NotImplementedException();
    }

    public IPlanckWindow.WindowState GetWindowState()
    {
      throw new NotImplementedException();
    }
  }
}
