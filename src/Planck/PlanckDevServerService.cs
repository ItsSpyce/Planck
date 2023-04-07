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
    private readonly PlanckConfiguration _configuration;
    private Process? _devServerProcess;

    public PlanckDevServerService(PlanckConfiguration configuration)
    {
      _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
#if DEBUG
      if (!string.IsNullOrEmpty(_configuration.DevCommand))
      {
        var commandParts = new[] { "run", _configuration.DevCommand };
        _devServerProcess = new Process
        {
          StartInfo = new()
          {
            WorkingDirectory = Path.Join(ProcessUtilities.GetProjectLocation(), _configuration.ClientDirectory ?? "."),
            UseShellExecute = true,
            CreateNoWindow = false,
            FileName = "npm.cmd",
            Arguments = $"run {_configuration.DevCommand}",
          },
          EnableRaisingEvents = true,
        };
        _devServerProcess.Start();
      }
#endif
      return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      _devServerProcess?.Kill();
      return Task.CompletedTask;
    }
  }
}
