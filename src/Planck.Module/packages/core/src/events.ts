import { setTitle } from 'core.js';

export function setupEvents() {
  window.addEventListener('contextmenu', (window) => window.preventDefault());
  document.addEventListener('DOMContentLoaded', () => {
    updateTitle();
    const target = document.querySelector('title')!;
    const observer = new MutationObserver((mutations) => {
      mutations.forEach(updateTitle);
    });
    observer.observe(target, { childList: true });
  });
}

function updateTitle() {
  setTitle(document.title);
}
