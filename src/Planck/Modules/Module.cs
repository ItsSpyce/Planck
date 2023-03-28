using Newtonsoft.Json;
using Planck.Controls;
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
        HasGetter = kvp.Value.GetGetMethod() != null,
        HasSetter = kvp.Value.GetSetMethod() != null,
      });
      return publicMethods.Concat(publicProperties);
    }

    public object? GetModuleProp(string prop)
    {
      if (_moduleProperties.TryGetValue(prop, out var propInfo))
      {
        return propInfo.GetValue(this);
      }
      return null;
    }

    public async Task<object?> InvokeMethodAsync(string method, JsonArray jsonArgs)
    {
      if (_moduleMethods.TryGetValue(method, out var methodInfo))
      {
        var args = InteropConverter.ConvertJsonToMethodArgs(jsonArgs, methodInfo, Services.GetService);
        var pureResult = methodInfo.Invoke(this, args);
        if (pureResult is Task awaitable)
        {
          await awaitable;
          return null;
        }
        if (pureResult is Task<object?> awaitableWithResult)
        {
          return await awaitableWithResult;
        }
        return pureResult;
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
