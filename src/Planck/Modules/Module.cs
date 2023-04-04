using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Planck.Controls;
using Planck.IO;
using Planck.TypeConverter;
using Planck.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Planck.Modules
{
  /// <summary>
  ///   References a module that can be used via `planck.import`
  /// </summary>
  /// <remarks>Module functionality is based on RPC and are ESM module based (no "default" export, only named).</remarks>
  public abstract class Module : HostObject
  {
    delegate IPropTypeConverter GetPropTypeConverter();

    public struct ExportDefinition
    {
      [JsonProperty("name")]
      public string Name;
      [JsonProperty("returnType")]
      public string ReturnType;
      [JsonProperty("hasGetter")]
      public bool HasGetter;
      [JsonProperty("hasSetter")]
      public bool HasSetter;
    }

    protected readonly IPlanckWindow Window;
    protected readonly IServiceProvider Services;
    readonly Dictionary<string, MethodInfo> _moduleMethods;
    readonly Dictionary<string, PropertyInfo> _moduleProperties;
    readonly Dictionary<string, GetPropTypeConverter> _typeConverters;

    protected Module(IPlanckWindow planckWindow, IServiceProvider services)
    {
      Window = planckWindow;
      Services = services;
      _moduleMethods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .Where(m => m.GetCustomAttribute<ExportMethodAttribute>() != null)
        .ToDictionary(m => m.GetCustomAttribute<ExportMethodAttribute>()!.Name ?? m.Name, m => m);
      _moduleProperties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.GetCustomAttribute<ExportPropertyAttribute>() != null)
        .ToDictionary(p => p.GetCustomAttribute<ExportPropertyAttribute>()!.Name ?? p.Name, p => p);
      _typeConverters = new();

      var converters = services.GetServices<IPropTypeConverter>();

      foreach (var (methodName, methodInfo) in _moduleMethods)
      {
        var converter = converters.SingleOrDefault(c => c.CanConvert(methodInfo.ReturnType));
        if (converter is not null)
        {
          _typeConverters.Add(methodName, () => (IPropTypeConverter) services.GetService(converter.GetType())!);
        }
      }
      foreach (var (propName, propInfo) in _moduleProperties)
      {
        var converter = converters.SingleOrDefault(c => c.CanConvert(propInfo.PropertyType));
        if (converter is not null)
        {
          _typeConverters.Add(propName, () => (IPropTypeConverter)services.GetService(converter.GetType())!);
        }
      }
    }

    public IEnumerable<ExportDefinition> GetModuleExports()
    {
      // exports will not be cached (for now). This is to let the JS handle the caching.
      var publicMethods = _moduleMethods.Select(kvp => new ExportDefinition
      {
        Name = kvp.Key,
        ReturnType = GetExportReturnType(kvp.Value.ReturnType, true),
        HasGetter = true,
        HasSetter = false,
      });
      var publicProperties = _moduleProperties.Select(kvp => new ExportDefinition
      {
        Name = kvp.Key,
        ReturnType = GetExportReturnType(kvp.Value.PropertyType, false),
        HasGetter = kvp.Value.GetGetMethod() is not null,
        HasSetter = kvp.Value.GetSetMethod() is not null,
      });
      return publicMethods.Concat(publicProperties);
    }

    public object? GetModuleProp(string prop)
    {
      if (_moduleProperties.TryGetValue(prop, out var propInfo))
      {
        if (_typeConverters.TryGetValue(prop, out var getConverter))
        {
          var converter = getConverter();
          return converter.Convert(propInfo.GetValue(this));
        }
        return propInfo.GetValue(this);
      }
      return null;
    }

    public async Task<object?> InvokeMethodAsync(string method, JsonArray jsonArgs)
    {
      // this method is unnecessarily complicated but it's all for the sake of async
      if (_moduleMethods.TryGetValue(method, out var methodInfo))
      {
        var args = InteropConverter.ConvertJsonToMethodArgs(jsonArgs, methodInfo, Services.GetService);
        var result = methodInfo.Invoke(this, args);
        _typeConverters.TryGetValue(method, out var getConverter);
        var converter = getConverter is not null ? getConverter() : null;
        
        if (result is Task<object?> awaitableWithResult)
        {
          result = await awaitableWithResult;
        }
        if (converter is not null)
        {
          try
          {
            return await converter.ConvertAsync(result);
          }
          catch (NotImplementedException)
          {
            return converter.Convert(result);
          }
        }
        return result;
      }
      return null;
    }

    static string GetExportReturnType(Type type, bool isMethod)
    {
      var typeResult = "void";
      if (type == typeof(string) || type == typeof(char))
      {
        typeResult = "string";
      }
      if (
        type == typeof(int) ||
        type == typeof(uint) ||
        type == typeof(short) ||
        type == typeof(ushort) ||
        type == typeof(sbyte) ||
        type == typeof(byte) ||
        type == typeof(long) ||
        type == typeof(ulong) ||
        type == typeof(float) ||
        type == typeof(double))
      {
        typeResult = "number";
      }
      if (type == typeof(bool))
      {
        typeResult = "boolean";
      }
      if (type == typeof(object))
      {
        typeResult = "object";
      }
      if (type.BaseType == typeof(Stream) || type == typeof(Stream))
      {
        typeResult = "stream";
      }
      if (type.IsArray)
      {
        typeResult = "array";
      }
      if (type.BaseType == typeof(DictionaryBase))
      {
        typeResult = "object";
      }

      return isMethod ? $"fn:{typeResult}" : typeResult;
    }
  }
}
