using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Planck.Messages
{
  public interface IPlanckCommandMessage
  {
    string Command { get; }
    int OperationId { get; }
    JsonElement? Body { get; }
  }

  internal class PlanckCommandMessage : IPlanckCommandMessage
  {
    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonPropertyName("operationId")]
    public int OperationId { get; set; }

    [JsonPropertyName("body")]
    public JsonElement? Body { get; set; }
  }
}
