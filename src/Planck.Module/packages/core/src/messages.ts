import type { PlanckMessage } from 'types.js';

let operationId = 0;

function getOperationId() {
  if (operationId === Number.MAX_SAFE_INTEGER) {
    operationId = 0;
  } else {
    operationId++;
  }
  return operationId;
}

export function sendMessage<TResponse>(command: string, body?: any) {
  const operationId = getOperationId();
  return new Promise<TResponse>((resolve) => {
    function handler(args: MessageEventArgs<PlanckMessage<any>>) {
      if (args.data.operationId === operationId) {
        resolve(args.data.body);
        window.chrome.webview.removeEventListener('message', handler);
      }
    }
    window.chrome.webview.addEventListener('message', handler);
    window.chrome.webview.postMessage({
      command,
      operationId,
      body,
    });
    console.debug('Posted webmessage', command, operationId, body);
  });
}
