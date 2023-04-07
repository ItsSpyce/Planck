using System.Text.Json;

namespace Planck.Messages
{
  public interface IMessageService
  {
    Task<(int, object?)> HandleMessageAsync(JsonElement message);
  }
}
