using Microsoft.Web.WebView2.Core;
using Planck.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planck.Controls
{
  public interface IPlanckWindow
  {
    CoreWebView2 CoreWebView2 { get; }
    string Title { get; set; }
    double Width { get; set; }
    double Height { get; set; }

    void Show();
    void Hide();
    void Close();

    void ShowSplashscreen();
    void CloseSplashscreen();
  }
}
