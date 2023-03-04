using Microsoft.Web.WebView2.Core;
using Planck.Commands;
using Planck.Configuration;
using Planck.Controls;
using Planck.Messages;
using Planck.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Planck.Extensions
{
  internal static class PlanckExtensions
  {
    private static readonly string _stdlib = """
      const planck = Object.create(null);

      (() => {
        let commandId = 0;

        function getNewCommandId() {
          if (commandId === Number.MAX_SAFE_INTEGER) {
            commandId = 0;
          } else {
            commandId++;
          }
          return commandId;
        }

        /**
         *
         * @param {string} command
         * @param {object} body
         */
        function sendMessage(command, body = null) {
          const id = getNewCommandId();
          const requestEventName = `${command}__request__${id}`;
          const responseEventName = `${command}__response__${id}`;
          return new Promise((resolve, reject) => {
            const handler = (args) => {
              try {
                if (args.data.command !== responseEventName) {
                  return;
                }
                resolve(args.data.body);
              } catch (err) {
                reject(err);
              } finally {
                window.chrome.webview.removeEventListener('message', handler);
              }
            };
            window.chrome.webview.addEventListener('message', handler);
            window.chrome.webview.postMessage({ command: requestEventName, body });
            console.debug('Posted message', { command, body });
          });
        }

        Object.defineProperty(planck, 'sendMessage', {
          get() {
            return sendMessage;
          },
        });

        // remove context menu
        window.addEventListener('contextmenu', window => window.preventDefault());

        const handlers = {
          async navigate(args) {
            const response = await fetch("{URL}" + args.to.replace(/\\|\//g, '__'));
            const html = await response.text();
            const iframe = document.getElementById('embedded-content');
            iframe.contentWindow.document.write(html);
          },
        };

        window.chrome.webview.addEventListener('message', (args) => {
          console.log(args.data);
          const handler = handlers[args.data.command];
          if (handler) {
            handler(args.data.body);
          }
        });
      })();
      
      """.Replace("{URL}", IResourceService.AppUrl);

    private static readonly Regex _commandRequestRegex = new(@"^([A-Za-z0-9\._]+)__request__([0-9]+)$");
    private static readonly Regex _commandResponseRegex = new(@"^([A-Za-z0-9\._]+)__response__([0-9]+)$");

    public static void ConfigureCommands(this IPlanckWindow planckWindow, ICommandHandlerService commandHandler)
    {
      async void WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs args)
      {
        if (PlanckCommandMessage.TryParse(args.WebMessageAsJson, out var message))
        {
          if (!_commandRequestRegex.IsMatch(message.Command) && !_commandResponseRegex.IsMatch(message.Command))
          {
            return;
          }
          var (commandName, commandId) = GetCommandParts(message.Command);
          if (message.Command.Contains("__request__"))
          {
            var result = await commandHandler.InvokeAsync(commandName, message.Body);
            var resultAsJson = JsonSerializer.SerializeToElement(result);
            var response = new PlanckCommandMessage
            {
              Command = $"{commandName}__response__${commandId}",
              Body = resultAsJson,
            };
            var responseBytes = JsonSerializer.SerializeToUtf8Bytes(response);
            planckWindow.CoreWebView2.PostWebMessageAsJson(Encoding.UTF8.GetString(responseBytes));
          }
          else if (message.Command.Contains("__response__"))
          {
            await commandHandler.InvokeAsync(commandName, message.Body);
          }
          else
          {
            // shouldn't happen, ignore
          }
        }
      }

      planckWindow.CoreWebView2.WebMessageReceived += WebMessageReceived;
      planckWindow.CoreWebView2.FrameCreated += (_, args)
        => args.Frame.WebMessageReceived += WebMessageReceived;
    }

    public static async void ConfigureResources(this IPlanckWindow planckWindow, IResourceService resources)
    {
      planckWindow.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);

      await planckWindow.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(_stdlib);
      resources.ConnectToPlanck(planckWindow, null);
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

    static (string commandName, string commandId) GetCommandParts(string fullCommand)
    {
      var commandSections = _commandRequestRegex.Matches(fullCommand)[0];
      var commandName = commandSections.Groups[1]?.Value!;
      var commandId = commandSections.Groups[2]?.Value!;
      return (commandName, commandId);
    }
  }
}
