using Planck.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planck.Modules.Internal
{
  public class AppModule : Module
  {
    [ExportProperty("windowState")]
    public string WindowState
    {
      get => Window.GetWindowState().ToString();
      set
      {
        switch (value.ToLower())
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
    }

    public AppModule(IPlanckWindow planckWindow, IServiceProvider services) : base(planckWindow, services) { }

    [ExportMethod("hideWindow")]
    public void HideWindow()
    {
      Window.Hide();
    }

    [ExportMethod("showWindow")]
    public void ShowWindow()
    {
      Window.Show();
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
