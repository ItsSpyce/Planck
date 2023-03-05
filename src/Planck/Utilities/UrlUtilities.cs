using Planck.Resources;
using System.Reflection;

namespace Planck.Utilities
{
  internal static class UrlUtilities
  {
    static Assembly _assembly;

    public static string GetResourceName(string path)
    {
      if (_assembly == null)
      {
        _assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
      }
      return $"{_assembly.GetName().Name}.{path.Replace('\\', '.').Replace('/', '.')}";
    }

    public static Uri GetLocalUri(string path) => new(IResourceService.AppUrl + path);
  }
}
