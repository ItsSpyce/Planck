using Planck.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planck.Modules.Internal
{
  public class AppModule : Module
  {
    public AppModule(IPlanckWindow planckWindow) : base(planckWindow) { }

    public void HideWindow()
    {
      Window.Hide();
    }

    public void ShowWindow()
    {
      Window.Show();
    }

    public string GetWindowState() => Window.GetWindowState().ToString();

    public void SetWindowState(int state) => Window.SetWindowState((IPlanckWindow.WindowState)state);
  }
}
