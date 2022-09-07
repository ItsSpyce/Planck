using System;
using System.Collections.Generic;
using System.Text;

namespace Planck.Controls
{
  public interface IPlanckSplashscreen
  {
    string Source { get; set; }
    bool IsActive { get; }

    void Show();
    void Close();
  }
}
