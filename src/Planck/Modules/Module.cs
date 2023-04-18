using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Planck.TypeConverter;
using Planck.Utilities;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Planck.Modules
{
    /// <summary>
    ///   References a module that can be used via `planck.import`
    /// </summary>
    /// <remarks>Module functionality is based on RPC and are ESM module based (no "default" export, only named).</remarks>
    public abstract class Module
  {
    delegate IPropTypeConverter GetPropTypeConverter();

    public struct ExportDefinition
    {
      [JsonProperty("name")]
      public string Name;
      [JsonProperty("isMethod")]
      public bool IsMethod;
      [JsonProperty("returnType")]
      public string ReturnType;
      [JsonProperty("hasGetter")]
      public bool HasGetter;
      [JsonProperty("hasSetter")]
      public bool HasSetter;
    }

    internal readonly string Name;
    protected readonly IPlanckWindow Window;
    protected readonly IServiceProvider Services;
    readonly Dictionary<string, MethodInfo> _moduleMethods;
    readonly Dictionary<string, PropertyInfo> _moduleProperties;
    readonly Dictionary<string, GetPropTypeConverter> _typeConverters;

    protected Module(string name, IPlanckWindow planckWindow, IServiceProvider services)
    {
      Name = name;
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
          _typeConverters.Add(methodName, () => (IPropTypeConverter)services.GetService(converter.GetType())!);
        }
      }
      foreach (var (propName, propInfo) in _moduleProperties)
      {
        // do type conversions
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
        ReturnType = GetExportReturnType(kvp.Value.ReturnType),
        IsMethod = true,
        HasGetter = true,
        HasSetter = false,
      });
      var publicProperties = _moduleProperties.Select(kvp => new ExportDefinition
      {
        Name = kvp.Key,
        ReturnType = GetExportReturnType(kvp.Value.PropertyType),
        IsMethod = false,
        HasGetter = kvp.Value.GetGetMethod() is not null,
        HasSetter = kvp.Value.GetSetMethod() is not null,
      });
      return publicMethods.Concat(publicProperties);
    }

    public object? GetModuleProp(string prop)
    {
      // we're going to hold off on watching properties until AFTER they've been fetched
      // since we don't want to notify the FE that properties have been updated if they're
      // not being used
      if (_moduleProperties.TryGetValue(prop, out var propInfo))
      {
        var value = ModuleProperty.Watch(
          () => propInfo.GetValue(this),
          out var registeredProperty);

        // register any updates to be sent through the window to notify the FE
        registeredProperty?.Bind(this, Window);

        if (_typeConverters.TryGetValue(prop, out var getConverter))
        {
          var converter = getConverter();
          return converter.Convert(value);
        }
        return value;
      }
      return null;
    }

    public bool SetModuleProp(string prop, object? value)
    {
      if (_moduleProperties.TryGetValue(prop, out var propInfo))
      {
        try
        {
          if (_typeConverters.TryGetValue(prop, out var getConverter))
          {
            var converter = getConverter();
            value = converter.Convert(value);
          }
          var setValueMethod = propInfo.GetSetMethod(false);
          setValueMethod?.Invoke(this, new[] { value });
          return true;
        }
        catch
        {
          return false;
        }
      }
      return false;
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

    static string GetExportReturnType(Type type)
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

      return typeResult;
    }
  }
}
