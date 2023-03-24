using Planck.Controls;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Planck.Modules.Internal
{
  public class ClipboardModule : Module
  {
    public ClipboardModule(IPlanckWindow planckWindow) : base(planckWindow) { }

    public string WriteText(string text)
    {
      System.Windows.Clipboard.SetText(text);
      Console.WriteLine(text);
      return text;
    }

    public string ReadText() => System.Windows.Clipboard.GetText();
  }
}
