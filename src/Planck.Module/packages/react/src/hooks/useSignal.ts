import { type Dispatch, useState, useEffect } from 'react';

const { getUuid } = await planck.import('random');

export function useSignal<T = string | boolean | number>(
  initialValue?: T | (() => T)
): [T | undefined, Dispatch<T>] {
  const [uuid, setUuid] = useState<string>();
  const [clientValue, setClientValue] = useState(initialValue);
  const [planckValue, setPlanckValue] = useState<T>();

  useEffect(() => {
    getUuid().then((uuid) => {
      setUuid(uuid);
    });
  }, []);

  useEffect(() => {
    if (typeof uuid === 'undefined') {
      return;
    }
    planck
      .sendMessage('SIGNAL_SET', { uuid, value: clientValue })
      .then((returnedValue) => {
        setPlanckValue(returnedValue);
      });
  }, [clientValue]);

  return [planckValue, setClientValue];
}
