using Microsoft.Extensions.Options;
using Planck.Configuration;
using System.IO;

namespace Planck.Resources
{
    internal class LocalResourceService : InternalResourceService
  {
    public LocalResourceService(PlanckConfiguration options) : base(options)
    {
    }

    public override void ConnectToPlanck(IPlanckWindow planck, string? root)
    {
      InitializeStartup(planck, root);
      planck.CoreWebView2.NavigationStarting += (_, args) =>
      {
        if (args.Uri == IResourceService.AppUrl)
        {
          // just navigated to the app
          args.Cancel = true;
          planck.CoreWebView2.Navigate(root);
        }
        else if (args.Uri.StartsWith(IResourceService.AppUrl))
        {
          planck.CoreWebView2.Navigate(args.Uri.Replace(IResourceService.AppUrl, null));
        }
      };

      planck.CoreWebView2.WebResourceRequested += (_, args) =>
      {
        // TODO: figure out how to fix base-path relative resource requests in WebView2
        if (args.Request.Uri.StartsWith(IResourceService.AppUrl))
        {
          args.Request.Uri = args.Request.Uri.Replace(IResourceService.AppUrl, null);
        }
        else
        {

        }
      };
    }

    public override Stream? GetResource(string name)
    {
      return File.OpenRead(Path.Join(_configuration.ClientDirectory, name));
    }
  }
}
