using System.Text.Json;

namespace Planck.Messages
{
  public interface IMessageService
  {
    Task<(int, IEnumerable<object?>)> HandleMessageAsync(JsonElement message);
  }
}
