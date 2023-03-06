using Microsoft.Extensions.Options;
using Planck.Configuration;
using Planck.Controls;
using Planck.Extensions;
using Planck.Utilities;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Planck.Resources
{

  internal class EmbeddedResourceService : InternalResourceService
  {
    readonly Assembly _assembly;
    string[] _availableResources; // might as well store it since it won't change

    public EmbeddedResourceService(Assembly assembly, IOptions<PlanckConfiguration> config) : base(config)
    {
      _assembly = assembly;
    }

    public override void ConnectToPlanck(IPlanckWindow planck, string? root)
    {
      ConnectWithLocalUri(planck, root);
      planck.CoreWebView2.WebResourceRequested += (_, args) =>
      {
        if (args.Request.Uri.StartsWith(IResourceService.AppUrl, StringComparison.OrdinalIgnoreCase))
        {
          var deferral = args.GetDeferral();
          var parsedUri = new Uri(args.Request.Uri);
          var uriWithoutQuery = parsedUri.AbsolutePath[1..];
          if (string.IsNullOrEmpty(uriWithoutQuery))
          {
            uriWithoutQuery = _configuration.Entry;
          }
          else
          {
            // need to add the build directory since it's transparent to the url
            uriWithoutQuery = Path.Join(_configuration.BuildDirectory, uriWithoutQuery);
          }
          try
          {
            var resx = GetResource(uriWithoutQuery);
            if (resx != null)
            {
              var response = planck.CoreWebView2.CreateResourceResponse(resx);
              args.Response = response;
            }
          }
          catch (Exception)
          {
            Console.WriteLine($"No resource found matching {uriWithoutQuery}");
          }
          finally
          {
            deferral.Complete();
          }
        }
      };
    }

    public override Stream? GetResource(string name)
    {
      if (_availableResources == default)
      {
        _availableResources = _assembly.GetManifestResourceNames();
      }
      var resourceName = UrlUtilities.GetResourceName(name);
      if (!_availableResources.Contains(resourceName))
      {
        // we can do a check here instead of trying to read from the assembly
        return null;
      }
      return _assembly.GetManifestResourceStream(resourceName);
    }
  }
}
