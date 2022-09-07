using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Planck.Configuration;
using Planck.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Loader;
using System.Text;

namespace Planck
{
  internal class PlanckHostService : IHostedService
  {
    private readonly PlanckConfiguration _config;
    private Process? _nodeProcess;

    public PlanckHostService(IOptions<PlanckConfiguration> configuration)
    {
      _config = configuration.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      if (!string.IsNullOrEmpty(_config.DevCommand))
      {
        if (ProcessUtilities.TryRunCommand(_config.DevCommand, _config.ClientDirectory ?? AppDomain.CurrentDomain.BaseDirectory, out var process))
        {
          _nodeProcess = process;
          // none of these are working :)
          AssemblyLoadContext.Default.Unloading += (_) =>
          {
            process.Kill();
          };
          Process.GetCurrentProcess().EnableRaisingEvents = true;
          Process.GetCurrentProcess().Exited += (_, _) =>
          {
            process.Kill();
          };
        }
      }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      _nodeProcess?.Kill();
      _nodeProcess?.Dispose();
    }
  }
}
