using Planck.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planck.Modules.Internal
{
  public class AppModule : Module
  {
    public AppModule(IPlanckWindow planckWindow, IServiceProvider services) : base(planckWindow, services) { }

    [ExportMethod]
    public void HideWindow()
    {
      Window.Hide();
    }

    [ExportMethod]
    public void ShowWindow()
    {
      Window.Show();
    }

    [ExportMethod]
    public string GetWindowState() => Window.GetWindowState().ToString();

    [ExportMethod]
    public void SetWindowState(string windowState)
    {
      switch (windowState.ToLower())
      {
        case "minimized":
          Window.SetWindowState(IPlanckWindow.WindowState.Minimized);
          break;
        case "maximized":
          Window.SetWindowState(IPlanckWindow.WindowState.Maximized);
          break;
        case "normal":
          Window.SetWindowState(IPlanckWindow.WindowState.Normal);
          break;
      }
    }

    [ExportMethod("randomDictionary")]
    public Dictionary<string, object> RandomDictionary() => new()
    {
      { "answerToLife", 42 },
      { "a_bool", true },
      { "a_string", "foo" }
    };
  }
}
