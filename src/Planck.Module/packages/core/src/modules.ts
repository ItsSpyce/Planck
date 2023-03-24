export function _import<TModule>(id: string) {
  const found =
    window.chrome.webview.hostObjects.__modules__.GetModule<TModule>(id);
  return found;
}
