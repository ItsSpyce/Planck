using System.Text.Json;

namespace Planck.Messages
{
  public interface IMessageService
  {
    void HandleMessage(JsonElement message);
  }
}
