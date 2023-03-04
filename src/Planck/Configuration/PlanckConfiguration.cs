namespace Planck.Configuration
{
  public class PlanckConfiguration
  {
    public const string PlanckKey = "Planck";

    public enum LinkLaunchRule
    {
      MachineDefault,
      CurrentWindow,
      NewWindow,
      None,
    }

    public bool SslOnly { get; set; } = true;

    public LinkLaunchRule OpenLinksIn { get; set; } = LinkLaunchRule.None;

    public string? Splashscreen { get; set; }

    public bool AllowExternalMessages { get; set; }

    public string? ClientDirectory { get; set; }

    public string BuildDirectory { get; set; }

    public bool WaitForShowCommand { get; set; }

    public bool UseWpf { get; set; }

    public string? DevUrl { get; set; }

    public string? DevCommand { get; set; }

    public string BuildCommand { get; set; } = "echo";

    public string[] BuildArgs { get; set; } = new[] { "No build command specified" };

    public string Entry { get; set; }
  }
}
