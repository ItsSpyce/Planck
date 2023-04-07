import { useState, useEffect } from 'react';

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

  async function sendMessage<TResponse>(body?: any) {
    const response = await planck.sendMessage<TResponse>(command, body);
    // @ts-ignore I have no clue why it's saying it needs a SetState modifier???
    setResponse(response);
    return response;
  }

  return [response, sendMessage];
}
