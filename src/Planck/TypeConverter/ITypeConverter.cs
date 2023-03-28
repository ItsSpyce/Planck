namespace Planck.TypeConverter
{
  public interface ITypeConverter<TIn, TOut>
  {
    TOut Convert(TIn value);

    Task<TOut> ConvertAsync(TIn value);
  }
}
