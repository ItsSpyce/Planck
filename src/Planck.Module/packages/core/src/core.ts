import { sendMessage } from 'messages.js';

export function setTitle(title: string) {
  document.title = title;
  return sendMessage('SET_WINDOW_TITLE', { title });
}

export function setWindowSize(width: number, height: number) {
  return sendMessage('SET_WINDOW_SIZE', { width, height });
}

export function showWindow() {
  return sendMessage('SET_WINDOW_STATE', 'normal');
}

export function hideWindow() {
  return sendMessage('SET_WINDOW_STATE', 'minimized');
}

export function setWindowState(state: 'minimized' | 'maximized' | 'normal') {
  return sendMessage('SET_WINDOW_STATE', state);
}
