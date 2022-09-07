using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Planck.Configuration
{
  public class PlanckConfiguration
  {
    public enum LinkLaunchRule
    {
      MachineDefault,
      CurrentWindow,
      NewWindow,
      None,
    }

    [JsonPropertyName("sslOnly")]
    public bool SslOnly { get; set; } = true;

    [JsonPropertyName("openLinksIn")]
    public LinkLaunchRule OpenLinksIn { get; set; } = LinkLaunchRule.None;

    [JsonPropertyName("splashScreen")]
    public string? Splashscreen { get; set; }

    [JsonPropertyName("allowExternalMessages")]
    public bool AllowExternalMessages { get; set; }

    public string? ClientDirectory { get; set; }

    public bool WaitForShowCommand { get; set; }

    public bool UseWpf { get; set; }

    public string? DevUrl { get; set; }

    public string DevCommand { get; set; } = "http://localhost/";
  }
}
