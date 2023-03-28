using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Planck.Messages
{

  public interface IPlanckMessage
  {
    string Command { get; }
    int OperationId { get; }
    JsonElement? Body { get; }
  }

  internal class PlanckMessage : IPlanckMessage
  {
    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonPropertyName("operationId")]
    public int OperationId { get; set; }

    [JsonPropertyName("body")]
    public JsonElement? Body { get; set; }
  }
}
