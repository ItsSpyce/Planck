using Microsoft.Extensions.Options;
using Planck.Configuration;
using System.Net.Http;

namespace Planck.HttpClients
{
  public class PlanckHttpClient : HttpClient
  {
    private readonly PlanckConfiguration _configuration;

    public PlanckHttpClient(PlanckConfiguration options) : base()
    {
      _configuration = options;
      if (!string.IsNullOrEmpty(_configuration.DevUrl))
      {
        BaseAddress = new Uri(_configuration.DevUrl);
      }
    }
  }
}
