using Planck.Messages;
using Planck.Modules;
using System.Text.Json.Nodes;

namespace Planck.Controllers
{
  public class ModuleController : MessageController
  {
    readonly IModuleService _moduleService;

    public ModuleController(IModuleService moduleService) => _moduleService = moduleService;

    [MessageHandler("LOAD_MODULE")]
    public object LoadModule(string id)
    {
      if (_moduleService.GetModule(id) is not Module module)
      {
        throw new ArgumentException("No module found matching", nameof(id));
      }
      return module.GetModuleExports();
    }

    [MessageHandler("GET_MODULE_PROP")]
    public object? GetModuleProp(string id, string prop)
    {
      if (_moduleService.GetModule(id) is not Module module)
      {
        throw new ArgumentException("No module found matching", nameof(id));
      }
      return module.GetModuleProp(prop);
    }

    public void SetModuleProp(string id, string prop, object? value)
    {
      if (_moduleService.GetModule(id) is not Module module)
      {
        throw new ArgumentException("No module found matching", nameof(id));
      }
      module.SetModuleProp(prop, value);
    }

    [MessageHandler("INVOKE_MODULE_METHOD")]
    public Task<object?> InvokeModuleMethodAsync(string id, string method, JsonArray args)
    {
      if (_moduleService.GetModule(id) is not Module module)
      {
        throw new ArgumentException("No module found matching", nameof(id));
      }
      return module.InvokeMethodAsync(method, args);
    }
  }
}
