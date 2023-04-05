using Planck.Commands;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Planck.Utils
{
  internal static class InteropConverter
  {
    public delegate object? TypeFromServiceDelegate(Type type);

    public static object?[] ConvertJsonToMethodArgs(
      JsonElement json,
      MethodInfo method,
      TypeFromServiceDelegate typeFromServiceDelegate) => method.GetParameters().Select(parameter =>
      {
        if (parameter.GetCustomAttribute<ServiceAttribute>() != null)
        {
          return typeFromServiceDelegate(parameter.ParameterType);
        }
        if (json.TryGetProperty(parameter.Name!, out var parameterValue))
        {
          return JsonSerializer.Deserialize(parameterValue, parameter.ParameterType);
        }
        return null;
      }).ToArray();

    public static object?[] ConvertJsonToMethodArgs(
      JsonArray json,
      MethodInfo method,
      TypeFromServiceDelegate typeFromServiceDelegate)
    {
      var jsonEnumerator = json.GetEnumerator();
      return method.GetParameters().Select(parameter =>
      {
        if (parameter.GetCustomAttribute<ServiceAttribute>() != null)
        {
          return typeFromServiceDelegate(parameter.ParameterType);
        }
        if (jsonEnumerator.MoveNext())
        {
          var nextJsonValue = jsonEnumerator.Current;
          if (nextJsonValue != null)
          {
            return JsonSerializer.Deserialize(nextJsonValue, parameter.ParameterType);
          }
        }
        return null;
      }).ToArray();
    }
  }
}
