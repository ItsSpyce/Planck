using Microsoft.Web.WebView2.Core;
using Planck.Commands;
using Planck.Commands.Internal;
using Planck.Configuration;
using Planck.Controls;
using Planck.Messages;
using Planck.Modules;
using Planck.Resources;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Planck.Extensions
{
  internal static class PlanckExtensions
  {
    public static void ConfigureMessages(this IPlanckWindow planckWindow, IMessageService commandHandler)
    {

      planckWindow.CoreWebView2.WebMessageReceived += (_, args) =>
      {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(args.WebMessageAsJson));
        if (JsonElement.TryParseValue(ref reader, out var asJson) && asJson != null)
        {
          commandHandler.HandleMessageAsync((JsonElement)asJson);
        }
        //await commandHandler.HandleMessageAsync(asJson);
        //if (PlanckCommandMessage.TryParse(args.WebMessageAsJson, out var message))
        //{
        //  if (!_commandRequestRegex.IsMatch(message.Command) && !_commandResponseRegex.IsMatch(message.Command))
        //  {
        //    return;
        //  }
        //  var (commandName, commandId) = GetCommandParts(message.Command);
        //  if (message.Command.Contains("__request__"))
        //  {
        //    var result = await commandHandler.InvokeAsync(commandName, message.Body);
        //    var resultAsJson = JsonSerializer.SerializeToElement(result);
        //    var response = new PlanckCommandMessage
        //    {
        //      Command = $"{commandName}__response__${commandId}",
        //      Body = resultAsJson,
        //    };
        //    var responseBytes = JsonSerializer.SerializeToUtf8Bytes(response);
        //    planckWindow.CoreWebView2.PostWebMessageAsJson(Encoding.UTF8.GetString(responseBytes));
        //  }
        //  else if (message.Command.Contains("__response__"))
        //  {
        //    await commandHandler.InvokeAsync(commandName, message.Body);
        //  }
        //  else
        //  {
        //    // shouldn't happen, ignore
        //  }
        //}
      };
    }

    public static void ConfigureModules(this IPlanckWindow planckWindow, IModuleService modules)
    {
      modules.Initialize();
      planckWindow.CoreWebView2.AddHostObjectToScript("__modules__", new ModuleBridge(modules));
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
