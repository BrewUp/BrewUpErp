import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import { defineConfig, globalIgnores } from 'eslint/config'
import boundaries from 'eslint-plugin-boundaries'

export default defineConfig([
  globalIgnores(['dist', 'src/mocks/**', 'src/setupTests.ts']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    plugins: {
      boundaries,
    },
    settings: {
      'boundaries/elements': [
        { type: 'feature', pattern: 'src/features/*' },
        { type: 'shared', pattern: 'src/shared/*' },
      ],
      'boundaries/ignore': ['src/mocks/**', 'src/setupTests.ts'],
    },
    rules: {
      // Fitness function: no cross-feature imports (Constitution Principle II)
      // Note: boundaries/element-types is the v5-compatible name; v6 renamed it
      // to boundaries/dependencies but the selector format changed — using v5 API
      'boundaries/element-types': [
        'error',
        {
          default: 'disallow',
          rules: [
            { from: 'feature', allow: ['shared'] },
            { from: 'shared', allow: ['shared'] },
          ],
        },
      ],
    },
    languageOptions: {
      globals: globals.browser,
    },
  },
])
