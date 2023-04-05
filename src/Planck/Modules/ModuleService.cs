using Microsoft.Extensions.DependencyInjection;

namespace Planck.Modules
{
  // End goal is to use `planck.import('module_name')`
  public class ModuleService : IModuleService
  {
    internal const string PostInitializationMessage = "Cannot add modules after initialization";
    internal const string NotReadyMessage = "Cannot get modules before initialization is complete";

    readonly IServiceCollection _services = new ServiceCollection();
    readonly Dictionary<string, Type> _moduleTypeMap = new();
    IServiceProvider? _serviceProvider;

    /// <summary>
    ///   Initializes the service with an empty <see cref="IServiceCollection"/>
    /// </summary>
    public ModuleService()
    {

    }

    /// <summary>
    ///   Initializes the service with the provided <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services"></param>
    public ModuleService(IServiceCollection services)
    {
      _services = services;
    }

    public IModuleService AddModule<T>(string name) where T : Module
    {
      if (_serviceProvider != default)
      {
        throw new InvalidOperationException(PostInitializationMessage);
      }
      _services.AddSingleton<T>();
      _moduleTypeMap.Add(name, typeof(T));
      return this;
    }

    public IModuleService AddModule<T>(string name, T module) where T : Module
    {
      if (_serviceProvider != default)
      {
        throw new InvalidOperationException(PostInitializationMessage);
      }

      _services.AddSingleton(module);
      _moduleTypeMap.Add(name, typeof(T));
      return this;
    }

    public object GetModule<T>() where T : Module
    {
      if (_serviceProvider == default)
      {
        throw new InvalidOperationException(NotReadyMessage);
      }
      var module = _serviceProvider.GetRequiredService<T>();
      return module;
    }

    public object GetModule(string name)
    {
      if (_serviceProvider == default)
      {
        throw new InvalidOperationException(NotReadyMessage);
      }
      if (_moduleTypeMap.TryGetValue(name, out var t))
      {
        var module = _serviceProvider.GetRequiredService(t);
        return module;
      }
      throw new KeyNotFoundException($"No matching module found: {name}");
    }

    public void Initialize()
    {
      if (_serviceProvider != default)
      {
        return;
      }
      _serviceProvider = _services.BuildServiceProvider();
    }
  }
}
