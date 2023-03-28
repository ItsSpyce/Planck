const BUFSIZE = 1024;

/**
 * Represents a stream from C#. These streams are meant to be read either all at once
 * or in chunks. Once the stream is advanced, all prior chunks will be lost.
 */
export class InteropStream<T = string | number> {
  #buffer: Buffer;
  #actualLength: number;
  #operationId: number;

  constructor(length: number, operationId: number) {
    this.#actualLength = length;
    this.#operationId = operationId;
    this.#buffer = Buffer.alloc(Math.min(length, BUFSIZE));
  }

  [Symbol.asyncIterator]() {}

  readToEnd(): Promise<T[]> {
    return Promise.resolve([]);
  }

  async readNext(): Promise<T[]> {
    return new Promise((resolve, reject) => {
      function handler(args: MessageEventArgs) {}

      window.chrome.webview.postMessage(`buf:${this.#operationId}`);
    });
  }

  chunk() {
    if (this.#buffer) {
      return this.#buffer;
    }
    return null;
  }
}
