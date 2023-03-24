import typescript from '@rollup/plugin-typescript';
import { minify } from 'rollup-plugin-esbuild';
import path from 'path';

const dir = 'dist';
const planckScriptsDir = path.resolve('../../../Planck/Scripts');

/**
 * @type {import('rollup').RollupOptions[]}
 */
const config = [
  {
    input: 'src/index.ts',
    output: [
      {
        file: path.join(dir, 'index.js'),
        name: 'planck',
        format: 'iife',
      },
      {
        file: path.join(planckScriptsDir, 'core.js'),
        name: 'planck',
        format: 'iife',
        plugins: [minify()],
      },
    ],
    plugins: [
      typescript({
        tsconfig: './tsconfig.json',
        declaration: false,
      }),
    ],
    treeshake: true,
  },
];

export default config;
