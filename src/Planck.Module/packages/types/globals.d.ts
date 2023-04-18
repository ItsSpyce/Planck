type WebViewEvent = 'message';

interface Window {
  get chrome(): Chrome;
}

interface Chrome {
  get webview(): WebView;
}

interface WebView {
  addEventListener<T>(
    event: WebViewEvent,
    handler: (arg: MessageEventArgs<T>) => void
  ): void;
  removeEventListener<T>(
    name: string,
    handler: (arg: MessageEventArgs<T>) => void
  ): void;
  postMessage(message: string | object): void;
  hostObjects: HostObjects;
}

interface HostObjects {
  //
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
  sendMessage<TResponse = any>(command: string, body?: any): Promise<TResponse>;
  /**
   * Imports a remote module exposed by Planck
   *
   * @param name Name of the module
   */
  import<TModule = any>(name: string): Promise<TModule>;
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

/**
 * Exposes the core standard library for Planck
 */
declare const planck: Planck;
