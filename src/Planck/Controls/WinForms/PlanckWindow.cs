using Microsoft.Web.WebView2.Core;
using Planck.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planck.Controls.WinForms
{
  internal class PlanckWindow : Form, IPlanckWindow
  {
    public CoreWebView2 CoreWebView2 => throw new NotImplementedException();

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

    public void CloseSplashscreen()
    {
      throw new NotImplementedException();
    }

    public void ShowSplashscreen()
    {
      throw new NotImplementedException();
    }
  }
}
