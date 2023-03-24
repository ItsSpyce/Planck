using System.Dynamic;
using System.Reflection;

namespace Planck.Modules
{
  public class ModuleBridge : HostObject
  {
    readonly IModuleService _moduleService;

    public ModuleBridge(IModuleService moduleService)
    {
      _moduleService = moduleService;
    }

    public object GetModule(string name)
    {
      var module = _moduleService.GetModule(name);
      return module;
      //var publicMethods = module.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
      //  .Where(m => m.GetCustomAttribute<ExportMethodAttribute>() != null)
      //  .Select(m => $"{m.GetCustomAttribute<ExportMethodAttribute>()!.Name ?? m.Name}={m.ReturnType.Name}");
      //return string.Join(';', publicMethods);
    }
  }
}
