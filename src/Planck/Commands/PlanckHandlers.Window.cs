using Planck.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Planck.Commands
{
  internal partial class PlanckHandlers
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

    [CommandHandler("SET_WINDOW_STATE")]
    public static void HandleSetWindowState([Service] IPlanckWindow window, string state)
    {
      if (window is Controls.Wpf.PlanckWindow wpfWindow)
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
      else if (window is Controls.WinForms.PlanckWindow winFormsWindow)
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
