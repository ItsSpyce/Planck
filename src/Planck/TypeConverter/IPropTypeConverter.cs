namespace Planck.TypeConverter
{
  public interface IPropTypeConverter
  {
    /// <summary>
    ///   Converts the object synchronously. This is used during Property fetching and during
    ///   Invokes where <see cref="ConvertAsync(object?)"/> is not implemented.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    object? Convert(object? value);

    /// <summary>
    ///   Converts the object asynchronously. This is only used during <see cref="Modules.Module.InvokeMethodAsync(string, System.Text.Json.Nodes.JsonArray)"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task<object?> ConvertAsync(object? value);

    /// <summary>
    ///   Returns if the type converter is allowed to convert this type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    bool CanConvert(Type type);
  }
}
