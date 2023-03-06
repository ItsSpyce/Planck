using Microsoft.Extensions.Options;
using Microsoft.Web.WebView2.Core;
using Planck.Configuration;
using Planck.Controls;
using Planck.Extensions;
using System.IO;

namespace Planck.Resources
{
  public interface IResourceService
  {
    public const string AppUrl = "http://app.planck/";

    Stream? GetResource(string name);
    void ConnectToPlanck(IPlanckWindow planck, string? root);
  }

  internal abstract class InternalResourceService : IResourceService
  {
    protected readonly PlanckConfiguration _configuration;

    protected const string _stdlib = """
      if (typeof globalThis.planck === 'undefined') {
        const planck = Object.create(null);
    
        let commandId = 0;
    
        const getNewCommandId = () => {
          if (commandId === Number.MAX_SAFE_INTEGER) {
            commandId = 0;
          } else {
            commandId++;
          }
          return commandId;
        }

        const sendMessage = (command, body = null) => {
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
            const response = await fetch(window.location.origin + args.to.replace('\\', '__'));
            const html = await response.text();
            document.open();
            document.write(html);
            document.close();
            updateTitle();
          },
        };
    
        window.chrome.webview.addEventListener('message', (args) => {
          const handler = handlers[args.data.command];
          if (handler) {
            handler(args.data.body);
          }
        });
    
        const updateTitle = () => {
          sendMessage('SET_WINDOW_TITLE', { title: document.title });
        }
    
        setTimeout(() => {
          const target = document.querySelector('title');
          const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
              udpateTitle();
            });
          });
          observer.observe(target, { childList: true });
          if (document.title) {
            updateTitle();
          }
        }, 500);
        globalThis.planck = planck;
      }

    """;

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

      planck.CoreWebView2.NavigationCompleted += async (_, args) =>
      {
        if (!planck.HasCompletedBootstrap)
        {
          planck.HasCompletedBootstrap = true;
        }
      };
      planck.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(_stdlib);
    }

    public abstract void ConnectToPlanck(IPlanckWindow planck, string? root);
    public abstract Stream? GetResource(string name);
  }
}
