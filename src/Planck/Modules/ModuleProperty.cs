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
    public event PropertyChangedEventHandler? PropertyChanged;

    public readonly string Name;
    private object? _value;
    private bool _isBound = false;

    protected object? Value
    {
      get => _value;
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

    public static ModuleProperty Register(string name) => new(name);

    public static ModuleProperty Register(string name, object initialValue) => new(name, initialValue);

    public object? GetValue() => Value;

    public void SetValue(object value) => Value = value;

    internal void Bind(Module module, IPlanckWindow planckWindow)
    {
      if (_isBound)
      {
        return;
      }
      PropertyChanged += (_, args) =>
        planckWindow.CoreWebView2.PostWebMessage(
          "MODULE_PROP_CHANGED",
          new { Name, Value, Module = module.Name });
    }
  }
}
