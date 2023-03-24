using System.Text.Json;

namespace Planck.Messages
{
  public interface IMessageService
  {
    Task HandleMessageAsync(JsonElement message);
  }
}
