using System;
using System.Collections.Generic;
using System.Text;

namespace Planck.Modules
{
  public interface IModuleService
  {
    void Initialize();
    IModuleService AddModule<T>(string name) where T : Module;
    IModuleService AddModule<T>(string name, T module) where T : Module;
    object GetModule<T>() where T : Module;
    object GetModule(string name);
  }
}
