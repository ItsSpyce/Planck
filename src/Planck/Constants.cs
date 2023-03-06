using Planck.Resources;
using System.Text;

namespace Planck
{
  internal static class Constants
  {
    public const string StartPageContent = $$"""
      <!DOCTYPE html>
      <html>
        <head>
          <script>
            const planck = Object.create(null);
      
            (() => {
              let commandId = 0;
              const baseUrl = "{{IResourceService.AppUrl}}";
      
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
                  const response = await fetch(baseUrl + args.to.replace('\\', '__'));
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
      
              function updateTitle() {
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
            })();
          </script>
      </html>
      """;

    public static readonly string StartPageContentAsBase64 =
      $"data:text/html;charset=utf-8;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(StartPageContent))}";
  }
}
