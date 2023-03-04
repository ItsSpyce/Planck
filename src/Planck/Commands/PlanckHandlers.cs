using Planck.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planck.Commands
{
  internal class PlanckHandlers
  {
    [CommandHandler("SET_WINDOW_TITLE")]
    public static void HandleSetWindowTitle([Service] IPlanckWindow window, string title)
    {
      window.Title = title;
    }

    [CommandHandler("SET_WINDOW_SIZE")]
    public static void HandleSetWindowSize([Service] IPlanckWindow window, int width, int height)
    {
      window.Width = width;
      window.Height = height;
    }

    [CommandHandler("HIDE_WINDOW")]
    public static void HandleHideWindow([Service] IPlanckWindow window)
    {
      window.Hide();
    }

    [CommandHandler("SHOW_WINDOW")]
    public static void HandleShowWindow([Service] IPlanckWindow window, [Service] IPlanckSplashscreen splashscreen)
    {
      if (splashscreen.IsActive)
      {
        splashscreen.Close();
      }
      window.Show();
    }

    [CommandHandler("REQUEST_SHUTDOWN")]
    public static void HandleRequestShutdown([Service] IPlanckWindow window)
    {

    }

    public static void HandleNavigateRequest([Service] IPlanckWindow window, string to)
    {

    }
  }
}
