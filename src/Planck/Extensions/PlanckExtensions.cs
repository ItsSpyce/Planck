using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;
using Planck.Commands.Internal;
using Planck.Configuration;
using Planck.Controls;
using Planck.Messages;
using Planck.Modules;
using Planck.Resources;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Planck.Extensions
{
  internal static class PlanckExtensions
  {
    static readonly Newtonsoft.Json.JsonSerializer _jsonSerializer = new()
    {

    };

    public static void ConfigureCoreWebView2(this IPlanckWindow planckWindow)
    {
      planckWindow.CoreWebView2.Settings.IsStatusBarEnabled = false;
#if DEBUG
      planckWindow.CoreWebView2.Settings.AreDevToolsEnabled = true;
#else
      planckWindow.CoreWebView2.Settings.AreDevToolsEnabled = false;
#endif

    }

    public static void ConfigureMessages(this IPlanckWindow planckWindow, IMessageService commandHandler)
    {
      planckWindow.CoreWebView2.WebMessageReceived += (_, args) =>
      {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(args.WebMessageAsJson));
        if (JsonElement.TryParseValue(ref reader, out var asJson) && asJson != null)
        {
          var (operationId, results) = commandHandler.HandleMessageAsync((JsonElement)asJson).Result;
          var jarray = JArray.FromObject(results.Where(r => r != null), _jsonSerializer);
          planckWindow.CoreWebView2.PostWebMessageAsJson($$"""
            { "operationId": {{operationId}}, "body": {{jarray}} }
            """);
        }
      };
    }

    public static void ConfigureModules(this IPlanckWindow planckWindow, IModuleService modules)
    {
      modules.Initialize();
    }

    public static void ConfigureResources(this IPlanckWindow planckWindow, IResourceService resources, string root)
    {
      planckWindow.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
      resources.ConnectToPlanck(planckWindow, root);
    }

    public static void ConfigureSecurityPolicies(this IPlanckWindow planckWindow, PlanckConfiguration.LinkLaunchRule openLinksIn)
    {
      planckWindow.CoreWebView2.NewWindowRequested += (_, args) =>
      {
        args.Handled = true;
        switch (openLinksIn)
        {
          case PlanckConfiguration.LinkLaunchRule.MachineDefault:
            break;
          case PlanckConfiguration.LinkLaunchRule.CurrentWindow:
            planckWindow.CoreWebView2.Navigate(args.Uri);
            break;
          case PlanckConfiguration.LinkLaunchRule.NewWindow:
            break;
          case PlanckConfiguration.LinkLaunchRule.None:
            break;
        }
      };

    }

    internal static void NavigateToEntry(this IPlanckWindow planckWindow, PlanckConfiguration config)
    {
#if DEBUG
      var url = config.DevUrl;
      if (string.IsNullOrEmpty(url))
      {
        url = Directory.GetCurrentDirectory();
      }
      else
      {
        if (!Path.IsPathFullyQualified(url) && !Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
          url = Path.Combine(Directory.GetCurrentDirectory(), url);
        }
      }
      planckWindow.CoreWebView2.PostWebMessage(new NavigateCommand { To = url });
#else
      planckWindow.CoreWebView2.PostWebMessage(new NavigateCommand { To = config.Entry });
#endif
    }
  }
}
