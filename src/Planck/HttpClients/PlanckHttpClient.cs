using Microsoft.Extensions.Options;
using Planck.Configuration;
using System.Net.Http;

namespace Planck.HttpClients
{
  public class PlanckHttpClient : HttpClient
  {
    private readonly PlanckConfiguration _configuration;

    public PlanckHttpClient(IOptions<PlanckConfiguration> options) : base()
    {
      _configuration = options.Value;
      if (!string.IsNullOrEmpty(_configuration.DevUrl))
      {
        BaseAddress = new Uri(_configuration.DevUrl);
      }
    }
  }
}
