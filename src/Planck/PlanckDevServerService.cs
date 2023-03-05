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
    private Process? _nodeProcess;

    public PlanckDevServerService(IOptions<PlanckConfiguration> configuration)
    {
      _config = configuration.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
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
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      _nodeProcess?.Kill();
      _nodeProcess?.Dispose();
    }

  }
}
