/// <reference types="@planck/types" />

declare interface Planck {
  sendMessage<T>(
    command: 'SIGNAL_SET',
    body: { uuid: string; value: T }
  ): Promise<T>;
}
