using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;
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
          var (operationId, body) = commandHandler.HandleMessageAsync((JsonElement)asJson).Result;
          var message = JObject.FromObject(new { body, operationId });
          
          planckWindow.CoreWebView2.PostWebMessageAsJson(message.ToString());
        }
      };
    }

    public static void ConfigureModules(this IPlanckWindow planckWindow, IModuleService modules)
    {
      // do we need to do anything here?
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
  }
}
