import typescript from '@rollup/plugin-typescript';
import { minify } from 'rollup-plugin-esbuild';
import path from 'path';

const dir = 'dist';

/**
 * @type {import('rollup').RollupOptions[]}
 */
const config = [
  {
    input: 'src/index.ts',
    output: [
      {
        file: path.join(dir, 'index.js'),
        format: 'esm',
        plugins: [process.env.NODE_ENV !== 'development' && minify()],
      },
    ],
    plugins: [
      typescript({
        tsconfig: './tsconfig.json',
        declaration: true,
      }),
    ],
    treeshake: true,
  },
];

export default config;
