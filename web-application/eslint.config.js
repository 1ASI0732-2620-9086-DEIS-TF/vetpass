// Análisis estático de la aplicación web y del JS de la landing (sección 6.2.1).
// Reglas recomendadas de JavaScript y de Vue, más límites de complejidad para
// que ninguna función crezca hasta volverse difícil de probar.
import js from '@eslint/js';
import vue from 'eslint-plugin-vue';
import globals from 'globals';

export default [
  { ignores: ['dist/**', 'playwright-report/**', 'test-results/**'] },
  js.configs.recommended,
  ...vue.configs['flat/recommended'],
  {
    files: ['**/*.{js,vue}'],
    languageOptions: {
      ecmaVersion: 'latest',
      sourceType: 'module',
      globals: globals.browser
    },
    rules: {
      complexity: ['error', 10],
      'max-depth': ['error', 3],
      'max-params': ['error', 4],
      // Formato: el proyecto usa marcado compacto (sección 5.1.3).
      'vue/max-attributes-per-line': 'off',
      'vue/singleline-html-element-content-newline': 'off'
    }
  },
  {
    files: ['e2e/**', '*.config.js'],
    languageOptions: { globals: globals.node }
  }
];
