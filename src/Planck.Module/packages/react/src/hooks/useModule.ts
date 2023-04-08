import { useState, useEffect } from 'react';

export function useModule<TModule = any>(name: string): TModule | undefined {
  const [module, setModule] = useState();

  const importModule = async () => {
    const module = await planck.import(name);
    console.debug(`Updated module ${name}`);
    setModule(module);
  };

  useEffect(() => {
    const listenForModPropChange = (
      args: MessageEventArgs<{ command: string; body: any }>
    ) => {
      if (
        args.data?.command === 'MODULE_PROP_CHANGED' &&
        args.data.body.Module === name
      ) {
        importModule();
      }
    };

    window.chrome.webview.addEventListener('message', listenForModPropChange);
    importModule();

    return () => {
      window.chrome.webview.removeEventListener(
        'message',
        listenForModPropChange
      );
    };
  }, [name, setModule]);

  return module;
}
