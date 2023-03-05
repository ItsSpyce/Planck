using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Web.WebView2.Core;
using Planck.Commands.Internal;
using Planck.Configuration;
using Planck.Controls;
using Planck.Controls.Wpf;
using Planck.Extensions;
using Planck.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace Planck.Resources
{
  public interface IResourceService
  {
    public const string AppUrl = "http://appassets.resx/";

    Stream? GetResource(string name);
    void ConnectToPlanck(IPlanckWindow planck, string? root);
  }

  internal abstract class InternalResourceService : IResourceService
  {
    protected readonly PlanckConfiguration _configuration;

    protected InternalResourceService(IOptions<PlanckConfiguration> options)
    {
      _configuration = options.Value;
    }

    protected void ConnectWithLocalUri(IPlanckWindow planck, string? root)
    {
      if (planck.CoreWebView2 == null)
      {
        throw new ArgumentNullException("CoreWebView2 is not initialized", nameof(planck.CoreWebView2));
      }

      planck.CoreWebView2.NavigationStarting += (_, args) =>
      {
        if (args.Uri == Constants.StartPageContentAsBase64)
        {
          // reload the entry because for some reason this gets called here on refresh
          void AfterRequestCompleted(object sender, CoreWebView2NavigationCompletedEventArgs args)
          {
            planck.NavigateToEntry(_configuration);
            planck.CoreWebView2.NavigationCompleted -= AfterRequestCompleted;
          }
          planck.CoreWebView2.NavigationCompleted += AfterRequestCompleted;
        }
      };

      planck.CoreWebView2.NavigationCompleted += (_, args) =>
      {
        // Debugger.Break();
        if (!planck.HasCompletedBootstrap)
        {
          planck.HasCompletedBootstrap = true;
        }
      };
    }

    public abstract void ConnectToPlanck(IPlanckWindow planck, string? root);
    public abstract Stream? GetResource(string name);
  }

  internal class LocalResourceService : InternalResourceService
  {

    public LocalResourceService(IOptions<PlanckConfiguration> options) : base(options)
    {
    }

    public override void ConnectToPlanck(IPlanckWindow planck, string? root)
    {
      ConnectWithLocalUri(planck, root);
      planck.CoreWebView2.NavigationStarting += (_, args) =>
      {
        if (args.Uri.StartsWith(IResourceService.AppUrl))
        {
          planck.CoreWebView2.Navigate(args.Uri.Replace(IResourceService.AppUrl, root));
        }
      };
      planck.CoreWebView2.WebResourceRequested += (_, args) =>
      {
        if (args.Request.Uri.StartsWith(IResourceService.AppUrl))
        {
          var uri = new Uri(args.Request.Uri);

        }
      };
    }

    public override Stream? GetResource(string name)
    {
      return File.OpenRead(Path.Join(_configuration.ClientDirectory, name));
    }
  }

  internal class EmbeddedResourceService : InternalResourceService
  {
    readonly Assembly _assembly;

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
          var parsedUri = new Uri(args.Request.Uri);
          var uriWithoutQuery = parsedUri.AbsolutePath[1..].Replace("__", "\\");
          if (string.IsNullOrEmpty(uriWithoutQuery))
          {
            uriWithoutQuery = _configuration.Entry;
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
        }
      };
    }

    public override Stream? GetResource(string name)
    {
      var resourceName = UrlUtilities.GetResourceName(name);
      return _assembly.GetManifestResourceStream(resourceName);
    }
  }
}
