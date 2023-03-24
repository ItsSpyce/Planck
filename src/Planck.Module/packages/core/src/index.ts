import { sendMessage } from 'messages.js';
import { _import } from 'modules.js';
import { setupEvents } from 'events.js';
import * as core from 'core.js';

setupEvents();

const exports = Object.create(core);
Object.defineProperties(exports, {
  sendMessage: {
    value: sendMessage,
    enumerable: false,
    writable: false,
  },
  import: {
    enumerable: false,
    writable: false,
    value: _import,
  },
});

export default exports;
