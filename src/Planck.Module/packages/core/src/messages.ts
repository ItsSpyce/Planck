import { postMessageAndWait, createOperationIdFactory } from 'utils.js';

const operationIdFactory = createOperationIdFactory();

export function sendMessage<TResponse>(command: string, body?: any) {
  const operationId = operationIdFactory.next();
  return postMessageAndWait<TResponse>({ command, body }, operationId);
}
