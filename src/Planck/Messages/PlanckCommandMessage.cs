using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Planck.Messages
{
  public interface IPlanckCommandMessage
  {
    string Command { get; }
    JsonElement? Body { get; }
  }

  internal class PlanckCommandMessage : IPlanckCommandMessage
  {
    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonPropertyName("body")]
    public JsonElement? Body { get; set; }

    internal static bool TryParse(string? webMessageAsJson, out PlanckCommandMessage message)
    {
      message = null;
      if (string.IsNullOrEmpty(webMessageAsJson))
      {
        return false;
      }
      using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(webMessageAsJson));
      try
      {
        var deserialized = JsonSerializer.Deserialize<PlanckCommandMessage>(jsonStream);
        if (deserialized is PlanckCommandMessage planckCommandMessage)
        {
          message = planckCommandMessage;
          return true;
        }
        return false;
      }
      catch (JsonException)
      {
        return false;
      }
      catch (Exception)
      {
        throw;
      }
    }
  }
}
