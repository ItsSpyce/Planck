using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Planck.Configuration;
using Planck.Utilities;
using System.Diagnostics;
using System.IO;

namespace Planck
{
  internal class PlanckDevServerService : IHostedService
  {
    private readonly PlanckConfiguration _config;

    public PlanckDevServerService(IOptions<PlanckConfiguration> configuration)
    {
      _config = configuration.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
#if DEBUG
      if (!string.IsNullOrEmpty(_config.DevCommand))
      {
        var commandParts = new[] { "run", _config.DevCommand };
        var process = new Process
        {
          StartInfo = new()
          {
            WorkingDirectory = Path.Join(ProcessUtilities.GetProjectLocation(), _config.ClientDirectory ?? "."),
            UseShellExecute = true,
            CreateNoWindow = false,
            FileName = "npm.cmd",
            Arguments = $"run {_config.DevCommand}",
          },
          EnableRaisingEvents = true,
        };
        process.Start();
      }
#endif
      return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      return Task.CompletedTask;
    }
  }
}
