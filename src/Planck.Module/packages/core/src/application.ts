import { commandSender } from './commands';

export interface Application {
  setWindowTitle(title: string): Promise<void>;
  setWindowSize(width: number, height: number): Promise<void>;
  hideWindow(): Promise<void>;
  showWindow(): Promise<void>;
  requestShutdown(): Promise<void>;
  start(): Promise<void>;
}

export enum LinkLaunchRule {
  MachineDefault,
  CurrentWindow,
  NewWindow,
  None,
}

export interface ApplicationOpts {
  get sslOnly(): boolean;
  get openLinksIn(): LinkLaunchRule;
  get splashScreen(): string | null;
}

const defaultOpts: ApplicationOpts = {
  sslOnly: true,
  openLinksIn: LinkLaunchRule.None,
  splashScreen: null,
};

export function application(opts: Partial<ApplicationOpts> = {}): Application {
  const commandSenderInstance = commandSender();

  return {
    async start() {
      const args = { ...defaultOpts, ...opts };
      // start C# process
    },
    async setWindowTitle(title) {
      try {
        await commandSenderInstance.sendMessage('SET_WINDOW_TITLE', { title });
      } catch (err) {
        //
      }
    },
    async setWindowSize(width, height) {
      try {
        await commandSenderInstance.sendMessage('SET_WINDOW_SIZE', {
          width,
          height,
        });
      } catch (err) {
        //
      }
    },
    async hideWindow() {
      try {
        await commandSenderInstance.sendMessage('HIDE_WINDOW');
      } catch (err) {
        //
      }
    },
    async showWindow() {
      try {
        await commandSenderInstance.sendMessage('SHOW_WINDOW');
      } catch (err) {
        //
      }
    },
    async requestShutdown() {
      try {
        await commandSenderInstance.sendMessage('REQUEST_SHUTDOWN');
      } catch (err) {
        //
      }
    },
  };
}
