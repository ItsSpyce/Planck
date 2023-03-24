import commonjs from '@rollup/plugin-commonjs';
import { nodeResolve } from '@rollup/plugin-node-resolve';
import typescript from '@rollup/plugin-typescript';
import { readFileSync } from 'fs';
import esbuild, { minify } from 'rollup-plugin-esbuild';
import peerDepsExternal from 'rollup-plugin-peer-deps-external';
import pkgJson from './package.json';

const umdName = pkgJson.name;

const globals = {
  ...(pkgJson.dependencies || {}),
};

const dir = 'dist';

/**
 *
 * @param {import('rollup').ModuleFormat} format
 *
 * @returns {import('rollup').OutputOptions[]}
 */
function createOutputsFor(format) {
  const ext = format === 'cjs' ? 'cjs' : 'js';
  const name = format === 'iife' || format === 'umd' ? umdName : undefined;
  return [
    {
      file: `${dir}/index.${format}.${ext}`,
      format,
      sourcemap: true,
      name,
    },
    {
      file: `${dir}/index.${format}.min.${ext}`,
      format,
      sourcemap: true,
      name,
      plugins: [minify()],
    },
  ];
}

const plugins = [
  nodeResolve(),
  commonjs({ include: 'node_modules/**' }),
  typescript({
    tsconfig: './tsconfig.build.json',
    declaration: false,
  }),
  esbuild({
    include: /\.[jt]sx?$/,
    exclude: /node_modules/,
    sourceMap: false,
    minify: process.env.NODE_ENV === 'production',
    target: 'esnext',
    tsconfig: './tsconfig.json',
    loaders: {
      '.json': 'json',
    },
  }),
  peerDepsExternal(),
];

/**
 * @type {import('rollup').RollupOptions[]}
 */
const config = [
  {
    input: 'src/index.ts',
    external: [...Object.keys(globals)],
    output: [
      ...createOutputsFor('umd'),
      ...createOutputsFor('iife'),
      ...createOutputsFor('cjs'),
      ...createOutputsFor('esm'),
    ],
    plugins,
    treeshake: true,
  },
  {
    input: 'src/stdlib/index.ts',
    external: [...Object.keys(globals)],
    output: [
      {
        file: `${dir}/stdlib.js`,
        format: 'iife',
        sourcemap: false,
        plugins: [minify()],
      },
    ],
    plugins,
    treeshake: true,
  },
];

export default config;
