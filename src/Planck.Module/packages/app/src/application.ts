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
  return {
    async start() {
      const app = await planck.import('app');
      const args = { ...defaultOpts, ...opts };
      // start C# process
    },
    async setWindowTitle(title) {
      try {
        await planck.setTitle(title);
      } catch (err) {
        //
      }
    },
    async setWindowSize(width, height) {
      try {
        await planck.setWindowSize(width, height);
      } catch (err) {
        //
      }
    },
    async hideWindow() {
      try {
        await planck.hideWindow();
      } catch (err) {
        //
      }
    },
    async showWindow() {
      try {
        await planck.showWindow();
      } catch (err) {
        //
      }
    },
    async requestShutdown() {
      try {
        await planck.sendMessage('REQUEST_SHUTDOWN');
      } catch (err) {
        //
      }
    },
  };
}
