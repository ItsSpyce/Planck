using Planck.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planck.TypeConverter
{
  public class ModulePropertyTypeConverter : IPropTypeConverter
  {
    public bool CanConvert(Type type) =>
      type == typeof(ModuleProperty);

    public object? Convert(object? value)
    {
      if (value is ModuleProperty property)
      {
        return property.GetValue();
      }
      return null;
    }

    public Task<object?> ConvertAsync(object? value)
    {
      if (value is ModuleProperty property)
      {
        return Task.FromResult(property.GetValue());
      }
      return null;
    }

    public object? ConvertBack(object? value) => throw new NotImplementedException();
    public Task<object?> ConvertBackAsync(object? value) => throw new NotImplementedException();
  }
}
