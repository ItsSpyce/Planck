interface Planck {
  import(name: 'random'): Promise<RandomModule>;
}

interface RandomModule {
  getUuid(): Promise<string>;
}
