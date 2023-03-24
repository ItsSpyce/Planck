/// <reference types="vite/client" />
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
	removeEventListener(handler: (arg: MessageEventArgs) => void): void;
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
