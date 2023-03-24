using Planck.Controls;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Planck.Modules
{
  /// <summary>
  ///   References a module that can be used via `planck.import`
  /// </summary>
  public abstract class Module : HostObject
  {
    protected readonly IPlanckWindow Window;

    protected Module(IPlanckWindow planckWindow)
    {
      Window = planckWindow;
    }
  }
}
