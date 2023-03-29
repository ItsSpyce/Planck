/// <reference types="./globals" />
/// <reference types="./modules" />

export type PlanckCommand =
  | 'SET_WINDOW_TITLE'
  | 'SET_WINDOW_SIZE'
  | 'HIDE_WINDOW'
  | 'SHOW_WINDOW'
  | 'REQUEST_SHUTDOWN';

export enum BodyType {
  REQUEST = 0,
  RESPONSE = 1,
  CHUNK_START = 2,
  CHUNK = 3,
  CHUNK_END = 4,
}

export interface PlanckMessage<T = unknown> {
  command: string;
  operationId: number;
  body?: T;
}

export interface PlanckResponse<T = unknown> {
  operationId: number;
  body: T[];
}
