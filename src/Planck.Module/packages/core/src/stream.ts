import { sendMessage } from 'messages.js';

const BUFSIZE = 1024;

export interface StreamLedger {
  id: string;
  position: number;
  length: number;
}

/**
 * Represents a stream from C#. These streams are meant to be read either all at once
 * or in chunks. Once the stream is advanced, all prior chunks will be lost.
 */
export class InteropStream<T = string | number> {
  #buffer: ArrayBuffer;
  #ledger: StreamLedger;
  #streamId: string | undefined;

  constructor(ledger: StreamLedger) {
    this.#ledger = ledger;
    this.#buffer = new ArrayBuffer(Math.min(ledger.length, BUFSIZE));
  }

  [Symbol.asyncIterator]() {}

  async readToEnd(): Promise<T[]> {
    const data = await sendMessage<T[]>('READ_STREAM', { id: this.#ledger.id });
    return data;
  }

  async readNext(): Promise<T[]> {
    const data = await sendMessage<T[]>('READ_STREAM_CHUNK', {
      id: this.#ledger.id,
    });
    return data;
  }

  chunk() {
    if (this.#buffer) {
      return this.#buffer;
    }
    return null;
  }

  async close() {
    return await sendMessage('CLOSE_STREAM', { id: this.#ledger.id });
  }
}
