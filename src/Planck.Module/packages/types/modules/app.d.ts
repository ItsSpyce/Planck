interface Planck {
  import(name: 'app'): Promise<AppModule>;
}

interface AppModule {
  windowState: string;
  hideWindow(): Promise<void>;
  showWindow(): Promise<void>;
  randomDictionary(): Promise<object>;
}
