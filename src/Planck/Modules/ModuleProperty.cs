using Planck.Controls;
using Planck.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Planck.Modules
{
  // I know this class seems funny but it's all for the sake of being alerted of
  // value changes without using proxy objects
  public class ModuleProperty : INotifyPropertyChanged
  {
    class PropertyGetEventArgs : EventArgs
    {
      public ModuleProperty Property { get; }

      public PropertyGetEventArgs(ModuleProperty moduleProperty)
      {
        Property = moduleProperty;
      }
    }

    delegate void PropertyGetEventHandler(object? sender, PropertyGetEventArgs e);

    public event PropertyChangedEventHandler? PropertyChanged;
    static event PropertyGetEventHandler? PropertyGet;

    public readonly string Name;
    private object? _value;
    private bool _isBound = false;

    protected object? Value
    {
      get
      {
        PropertyGet?.Invoke(null, new PropertyGetEventArgs(this));
        return _value;
      }
      set
      {
        _value = value;
        PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(Name));
      }
    }

    protected ModuleProperty(string name)
    {
      Name = name;
    }

    protected ModuleProperty(string name, object value)
    {
      Name = name;
      Value = value;
    }

    /// <summary>
    ///   Registers a module property to Planck for watching with an initial value of NULL.
    /// </summary>
    /// <param name="name">The name of the property. If this property is for one with the <see cref="ExportMethodAttribute"/>, ensure the two names match.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static ModuleProperty Register(string name) => new(name);

    /// <summary>
    ///   Registers a module property to Planck for watching with an initial value.
    /// </summary>
    /// <param name="name">The name of the property. If this property is for one with the <see cref="ExportMethodAttribute"/>, ensure the two names match.</param>
    /// <param name="initialValue"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static ModuleProperty Register(string name, object initialValue) => new(name, initialValue);

    internal static T Watch<T>(Func<T> watchDuring, out ModuleProperty? moduleProperty)
    {
      ModuleProperty? localModuleProperty = null;
      void HandleGet(object? sender, PropertyGetEventArgs args)
      {
        localModuleProperty = args.Property;
      }
      PropertyGet += HandleGet;
      var result = watchDuring();
      PropertyGet -= HandleGet;
      moduleProperty = localModuleProperty;
      return result;
    }

    public object? GetValue() => Value;

    public void SetValue(object value) => Value = value;

    internal void Bind(Module module, IPlanckWindow planckWindow)
    {
      if (_isBound)
      {
        return;
      }
      PropertyChanged += (_, args) =>
        planckWindow.PostWebMessage(
          "MODULE_PROP_CHANGED",
          new { Name, Value, Module = module.Name });
    }
  }
}
