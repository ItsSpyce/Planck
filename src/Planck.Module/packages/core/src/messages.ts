import {
  postMessageAndWait,
  createOperationIdFactory,
  postMessageAndWaitSync,
} from 'utils.js';

const operationIdFactory = createOperationIdFactory();

export async function sendMessage<TResponse = any>(
  command: string,
  body?: any
): Promise<TResponse> {
  const operationId = operationIdFactory.next();
  const [result] = await postMessageAndWait<TResponse>(
    { command, body },
    operationId
  );
  return result;
}

export function sendMessageSync<TResponse = any>(
  command: string,
  body?: any
): TResponse {
  const operationId = operationIdFactory.next();
  const [result] = postMessageAndWaitSync<TResponse>(
    { command, body },
    operationId
  );
  return result;
}
