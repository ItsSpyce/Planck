using Newtonsoft.Json;
using Planck.Messages;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Planck.Utilities
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
          if (parameter.ParameterType.GetCustomAttribute<JsonObjectAttribute>() != null)
          {
            var rawText = parameterValue.GetRawText();
            return JsonConvert.DeserializeObject(rawText, parameter.ParameterType);
          }
          return parameterValue.Deserialize(parameter.ParameterType);
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
            return nextJsonValue.Deserialize(parameter.ParameterType);
          }
        }
        return null;
      }).ToArray();
    }
  }
}
