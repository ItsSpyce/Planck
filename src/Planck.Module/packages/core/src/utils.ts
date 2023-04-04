import type { PlanckMessage, PlanckResponse } from 'types.js';

export function postMessageAndWait<TResponse>(
  message: object,
  operationId: number,
  until?: (response: Array<TResponse>) => boolean
) {
  return new Promise<Array<TResponse>>((resolve) => {
    function handler(args: MessageEventArgs<PlanckResponse<TResponse>>) {
      if (
        args.data.operationId === operationId &&
        (typeof until === 'undefined' || until(args.data.body))
      ) {
        resolve(args.data.body);
        console.debug('Received webmessage', operationId, args.data);
        window.chrome.webview.removeEventListener('message', handler);
      }
    }

    window.chrome.webview.addEventListener('message', handler);
    window.chrome.webview.postMessage({ ...message, operationId });
    console.debug('Posted webmessage', operationId, message);
  });
}

export function postMessageAndWaitSync<TResponse>(
  message: object,
  operationId: number,
  until?: (response: Array<TResponse>) => boolean
) {
  let response: TResponse[] | null = null;
  function handler(args: MessageEventArgs<PlanckResponse<TResponse>>) {
    if (
      args.data.operationId === operationId &&
      (typeof until === 'undefined' || until(args.data.body))
    ) {
      response = args.data.body;
      console.debug('Received webmessage', operationId, args.data);
      window.chrome.webview.removeEventListener('message', handler);
    }
  }

  window.chrome.webview.addEventListener('message', handler);
  window.chrome.webview.postMessage({ ...message, operationId });
  console.debug('Posted sync webmessage', operationId, message);
  while (response === null) {
    // do nothing
  }
  return response as TResponse[];
}

export function createOperationIdFactory() {
  let id = -1;

  return {
    current: id,
    next() {
      if (id === Number.MAX_SAFE_INTEGER) {
        id = 0;
      } else {
        id++;
      }
      return id;
    },
  };
}
