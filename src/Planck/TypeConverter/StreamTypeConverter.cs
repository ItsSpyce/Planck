using Planck.IO;
using System.IO;

namespace Planck.TypeConverter
{
  public sealed class StreamTypeConverter : IPropTypeConverter
  {
    readonly IStreamPool _streamPool;

    public StreamTypeConverter(IStreamPool streamPool)
    {
      _streamPool = streamPool;
    }

    public object? Convert(object? value)
    {
      if (value is Stream stream)
      {
        return Convert(stream);
      }
      return null;
    }

    StreamLedger Convert(Stream stream) => _streamPool.CreateLedger(ref stream);

    public async Task<object?> ConvertAsync(object? value)
    {
      if (value is Stream stream)
      {
        return await ConvertAsync(stream);
      }
      return null;
    }

    Task<StreamLedger> ConvertAsync(Stream stream) => Task.FromResult(_streamPool.CreateLedger(ref stream));

    public object? ConvertBack(object? value) => throw new NotImplementedException();
    public Task<object?> ConvertBackAsync(object? value) => throw new NotImplementedException();

    public bool CanConvert(Type type) => type == typeof(Stream) || type.BaseType == typeof(Stream);
  }
}
