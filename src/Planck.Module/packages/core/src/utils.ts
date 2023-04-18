import type { PlanckResponse } from '@planck/types';

export function postMessageAndWait<TResponse>(
  message: object,
  operationId: number,
  until?: (response: TResponse) => boolean
) {
  return new Promise<TResponse>((resolve, reject) => {
    function handler(args: MessageEventArgs<PlanckResponse<TResponse>>) {
      if (
        args.data.operationId === operationId &&
        (typeof until === 'undefined' || until(args.data.body))
      ) {
        const totalTime = new Date().getTime() - start.getTime();
        console.debug('Received webmessage', operationId, args.data, totalTime);
        window.chrome.webview.removeEventListener('message', handler);
        if (args.data.error) {
          reject(args.data.error);
        } else {
          resolve(args.data.body);
        }
      }
    }

    window.chrome.webview.addEventListener('message', handler);
    const start = new Date();
    window.chrome.webview.postMessage({ ...message, operationId });
    console.debug('Posted webmessage', operationId, message);
  });
}

export function postMessageAndWaitSync<TResponse>(
  message: object,
  operationId: number,
  until?: (response: TResponse) => boolean
) {
  let response: TResponse | null = null;
  function handler(args: MessageEventArgs<PlanckResponse<TResponse>>) {
    if (
      args.data.operationId === operationId &&
      (typeof until === 'undefined' || until(args.data.body))
    ) {
      const totalTime = new Date().getTime() - start.getTime();
      console.debug('Received webmessage', operationId, args.data, totalTime);
      window.chrome.webview.removeEventListener('message', handler);
      if (args.data.error) {
        throw args.data.error;
      }
      response = args.data.body;
    }
  }

  window.chrome.webview.addEventListener('message', handler);
  const start = new Date();
  window.chrome.webview.postMessage({ ...message, operationId });
  console.debug('Posted sync webmessage', operationId, message);
  while (response === null) {
    // do nothing
  }
  return response as TResponse;
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

export function waitFor(check: () => boolean, timeoutMs?: number) {
  return new Promise((resolve, reject) => {
    const interval = setInterval(() => {
      if (check()) {
        clearTimeout(timeout);
        clearInterval(interval);
        resolve(true);
      }
    }, 5);
    const timeout = setTimeout(() => {
      reject('Wait timed out');
    }, timeoutMs || 1000);
  });
}
