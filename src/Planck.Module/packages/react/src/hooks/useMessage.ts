import { useState, useEffect, useCallback } from 'react';

export function useMessage<TResponse>(
  command: string,
  body?: any
): TResponse | undefined {
  const [response, setResponse] = useState<TResponse>();

  useEffect(() => {
    planck.sendMessage<TResponse>(command, body).then((response) => {
      setResponse(response);
    });
  }, [command, body, setResponse]);

  return response;
}

export function useLazyMessage<TResponse>(
  command: string
): [TResponse | undefined, (body?: any) => Promise<TResponse>] {
  const [response, setResponse] = useState<TResponse>();
  const sendMessage = useCallback(
    async (body: any) => {
      const response = await planck.sendMessage<TResponse>(command, body);
      setResponse(response);
      return response;
    },
    [command]
  );

  return [response, sendMessage];
}
