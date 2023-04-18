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

  public class PlanckMessage : IPlanckMessage
  {
    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonPropertyName("operationId")]
    public int OperationId { get; set; }

    [JsonPropertyName("body")]
    public JsonElement? Body { get; set; }
  }

  public class MessageWorkResponse
  {
    public readonly int OperationId;
    public readonly object? Body;
    public readonly string? Error;

    public MessageWorkResponse(int operationId) =>
      OperationId = operationId;

    public MessageWorkResponse(int operationId, string error) =>
      (OperationId, Error) = (operationId, error);

    public MessageWorkResponse(int operationId, object? body) =>
      (OperationId, Body) = (operationId, body);
  }
}
