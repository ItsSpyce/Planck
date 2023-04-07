using Microsoft.Extensions.DependencyInjection;

namespace Planck.Modules
{
  // End goal is to use `planck.import('module_name')`
  public class ModuleService : IModuleService
  {
    public interface IModuleDefinition
    {
      string Name { get; }
      Type Type { get; }
    }

    public class ModuleDefinition<T> : IModuleDefinition where T : Module
    {
      public string Name { get; }
      public Type Type { get; }

      public ModuleDefinition(string name)
      {
        Name = name;
        Type = typeof(T);
      }
    }

    internal const string PostInitializationMessage = "Cannot add modules after initialization";
    internal const string NotReadyMessage = "Cannot get modules before initialization is complete";

    IServiceProvider? _serviceProvider;

    /// <summary>
    ///   Initializes the service with the provided <see cref="IServiceProvider"/>
    /// </summary>
    /// <param name="serviceProvider"></param>
    public ModuleService(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
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
      var moduleWrapper = _serviceProvider.GetServices<IModuleDefinition>().SingleOrDefault(m => m.Name.Equals(name));
      if (moduleWrapper is not null)
      {
        return _serviceProvider.GetRequiredService(moduleWrapper.Type);
      }
      throw new KeyNotFoundException($"No matching module found: {name}");
    }
  }
}
