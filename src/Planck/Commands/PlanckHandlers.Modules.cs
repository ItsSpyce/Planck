using Planck.Modules;
using System.Text.Json.Nodes;

namespace Planck.Commands
{
  internal partial class PlanckHandlers
  {
    [CommandHandler("LOAD_MODULE")]
    public static object LoadModule([Service] IModuleService moduleService, string id)
    {
      if (moduleService.GetModule(id) is not Module module)
      {
        throw new ArgumentException("No module found matching", nameof(id));
      }
      return module.GetModuleExports();
    }

    [CommandHandler("GET_MODULE_PROP")]
    public static object? GetModuleProp([Service] IModuleService moduleService, string id, string prop)
    {
      if (moduleService.GetModule(id) is not Module module)
      {
        throw new ArgumentException("No module found matching", nameof(id));
      }
      return module.GetModuleProp(prop);
    }

    [CommandHandler("INVOKE_MODULE_METHOD")]
    public static Task<object?> InvokeModuleMethodAsync([Service] IModuleService moduleService, string id, string method, JsonArray args)
    {
      if (moduleService.GetModule(id) is not Module module)
      {
        throw new ArgumentException("No module found matching", nameof(id));
      }
      return module.InvokeMethodAsync(method, args);
    }
  }
}
