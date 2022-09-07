import type { PlanckCommand } from './types';

type RoutedCommand<T> = PlanckCommand & T;
type CommandMessage<T> = MessageEventArg<{
  command: string;
  body: T;
}>;

export interface CommandSender<T> {
  sendMessage<TResponse>(
    command: RoutedCommand<T>,
    body?: any
  ): Promise<TResponse>;
}

export function commandSender<T>(): CommandSender<T> {
  let commandId = 0;

  function getNewCommandId() {
    if (commandId === Number.MAX_SAFE_INTEGER) {
      commandId = 0;
    } else {
      commandId++;
    }
    return commandId;
  }

  return {
    sendMessage(command, body?) {
      const id = getNewCommandId();
      const requestEventName = `${command}__request__${id}`;
      const responseEventName = `${command}__response__${id}`;
      return new Promise((resolve, reject) => {
        const handler = (args: CommandMessage<any>) => {
          try {
            if (args.data.command !== responseEventName) {
              return;
            }
            resolve(args.data.body);
          } catch (err) {
            reject(err);
          } finally {
            window.chrome.webview.removeEventListener(handler);
          }
        };
        window.chrome.webview.addEventListener('message', handler);
        window.chrome.webview.postMessage({ command: requestEventName, body });
        console.log('Posted message', { command, body });
      });
    },
  };
}
