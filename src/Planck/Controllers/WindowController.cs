using Planck.Controls;
using Planck.Messages;
using System.Windows;

namespace Planck.Controllers
{
  public class WindowController : MessageController
  {
    readonly IPlanckWindow _window;
    readonly IPlanckSplashscreen _splashscreen;

    public WindowController(IPlanckWindow window, IPlanckSplashscreen splashscreen)
    {
      _window = window;
      _splashscreen = splashscreen;
    }

    [MessageHandler("SET_WINDOW_TITLE")]
    public void HandleSetWindowTitle(string title)
    {
      _window.Title = title;
    }

    [MessageHandler("SET_WINDOW_SIZE")]
    public void HandleSetWindowSize(int width, int height)
    {
      _window.Width = width;
      _window.Height = height;
    }

    [MessageHandler("HIDE_WINDOW")]
    public void HandleHideWindow()
    {
      _window.Hide();
    }

    [MessageHandler("SHOW_WINDOW")]
    public void HandleShowWindow()
    {
      if (_splashscreen.IsActive)
      {
        _splashscreen.Close();
      }
      _window.Show();
    }

    [MessageHandler("REQUEST_SHUTDOWN")]
    public void HandleRequestShutdown()
    {

    }

    [MessageHandler("SET_WINDOW_STATE")]
    public void HandleSetWindowState(string state)
    {
      if (_window is Controls.Wpf.PlanckWindow wpfWindow)
      {
        switch (state)
        {
          case "minimized":
            wpfWindow.WindowState = WindowState.Minimized;
            break;
          case "maximized":
            wpfWindow.WindowState = WindowState.Maximized;
            break;
          case "normal":
            wpfWindow.WindowState = WindowState.Normal;
            break;
        }
      }
      else if (_window is Controls.WinForms.PlanckWindow winFormsWindow)
      {
        switch (state)
        {
          case "minimzed":
            winFormsWindow.WindowState = FormWindowState.Minimized;
            break;
          case "maximized":
            winFormsWindow.WindowState = FormWindowState.Maximized;
            break;
          case "normal":
            winFormsWindow.WindowState = FormWindowState.Normal;
            break;
        }
      }
    }
  }
}
