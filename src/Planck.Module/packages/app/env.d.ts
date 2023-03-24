type WebViewEvent = 'message';

interface Window {
  get chrome(): Chrome;
}

interface Chrome {
  get webview(): WebView;
}

interface WebView {
  addEventListener(
    event: WebViewEvent,
    handler: (arg: MessageEventArgs) => void
  ): void;
  removeEventListener(
    name: string,
    handler: (arg: MessageEventArgs) => void
  ): void;
  postMessage(message: any): void;
}

interface MessageEventArgs<T = unknown> {
  get isTrusted(): boolean;
  get data(): T;
  bubbles: boolean;
  cancelBubble: boolean;
  get cancelable(): boolean;
  composed: boolean;
  get currentTarget(): EventTarget;
  defaultPrevented: boolean;
  eventPhase: number;
  returnValue: boolean;
  get srcElement(): EventTarget;
  get target(): EventTarget;
  get timeStamp(): number;
  get type(): 'message';
}

interface Planck {
  /**
   * Sends a command to Planck for interception
   *
   * @param command The command name
   * @param body The JSON argument of argument names and variables
   */
  sendMessage<TResponse>(command: string, body?: any): Promise<TResponse>;
  /**
   * Imports a remote module exposed by Planck
   *
   * @param name Name of the module
   */
  import<TModule>(name: string): Promise<TModule>;
  /**
   * Sets the window title
   *
   * @param title
   */
  setTitle(title: string): Promise<void>;
  /**
   * Sets the window size
   *
   * @param width
   * @param height
   */
  setWindowSize(width: number, height: number): Promise<void>;
  showWindow(): Promise<void>;
  hideWindow(): Promise<void>;
  setWindowState(state: 'minimized' | 'maximized' | 'normal'): Promise<void>;
}

declare const planck: Planck;
