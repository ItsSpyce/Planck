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
export class InteropStream {
  private _buffer: Uint8Array;
  private _ledger: StreamLedger;
  private _isClosed = false;

  get remainingBytes() {
    return this._ledger.length - this._ledger.position;
  }

  constructor(ledger: StreamLedger) {
    this._ledger = ledger;
    this._buffer = new Uint8Array(Math.min(ledger.length, BUFSIZE));
  }

  private canRead() {
    return this.remainingBytes > 0;
  }

  [Symbol.asyncIterator]() {}

  async readToEnd(): Promise<Uint8Array> {
    if (this._isClosed) {
      throw 'Cannot read from a closed stream';
    }
    if (!this.canRead()) {
      return this._buffer;
    }
    const data = await sendMessage<string>('READ_STREAM', {
      id: this._ledger.id,
    });
    this._buffer = convertBase64(data);
    this._ledger.position += this._buffer.length;
    return this._buffer;
  }

  async readNext(): Promise<Uint8Array | null> {
    if (this._isClosed) {
      throw 'Cannot read from a closed stream';
    }
    if (!this.canRead()) {
      return null;
    }
    const data = await sendMessage<string>('READ_STREAM_CHUNK', {
      id: this._ledger.id,
    });
    this._buffer = convertBase64(data);
    this._ledger.position += this._buffer.length;
    return this._buffer;
  }

  data() {
    return this._buffer;
  }

  text() {
    const decoder = new TextDecoder();
    return decoder.decode(this._buffer);
  }

  async close() {
    await sendMessage('CLOSE_STREAM', { id: this._ledger.id });
    this._isClosed = true;
  }
}

function convertBase64(data: string) {
  return Uint8Array.from(atob(data), (c) => c.charCodeAt(0));
}
