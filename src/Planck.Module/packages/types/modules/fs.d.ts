interface Planck {
  import(name: 'fs'): Promise<FileSystemModule>;
}

interface FileSystemModule {
  get directorySeparator(): string;
  readFile(path: string): Promise<any>; // TODO: move stream here so that we can use it
  openFileDialog(root: string, filter?: string): Promise<string | null>;
  openFolderDialog(root: string, filter?: string): Promise<string | null>;
}
