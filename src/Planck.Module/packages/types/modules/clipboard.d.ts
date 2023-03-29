interface Planck {
  import(name: 'clipboard'): Promise<ClipboardModule>;
}

interface ClipboardModule {
  writeText(text: string): Promise<string>;
  readText(): Promise<string>;
}
