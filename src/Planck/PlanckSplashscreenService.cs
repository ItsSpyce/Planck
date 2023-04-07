using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Planck.Configuration;
using Planck.Controls;

namespace Planck
{
  internal class PlanckSplashscreenService : IHostedService
  {
    readonly IServiceProvider _services;
    readonly PlanckConfiguration _configuration;

    public PlanckSplashscreenService(IServiceProvider services, PlanckConfiguration configuration)
    {
      _services = services;
      _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      if (!string.IsNullOrEmpty(_configuration.Splashscreen))
      {
        var splashscreen = _services.GetService<IPlanckSplashscreen>();
        splashscreen?.Show();
        cancellationToken.Register(() =>
        {
          splashscreen?.Close();
        });
      }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      try
      {
        var splashscreen = _services.GetService<IPlanckSplashscreen>();
        splashscreen?.Close();
      }
      catch
      {
        // ignore
      }
    }
  }
}
