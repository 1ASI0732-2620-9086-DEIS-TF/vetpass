import Aura from '@primevue/themes/aura';
import { definePreset } from '@primevue/themes';

/**
 * Paleta de la sección 4.1.1 del informe llevada a los tokens de PrimeVue.
 *
 * El teal asocia el producto al ámbito sanitario sin el azul clínico, que
 * resulta frío para el segmento de dueños; el ámbar secundario aporta la
 * calidez del vínculo con la mascota.
 */
export const VetPassPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50:  '#f0fdfa',
      100: '#ccfbf1',
      200: '#99f6e4',
      300: '#5eead4',
      400: '#2dd4bf',
      500: '#14b8a6',
      600: '#0d9488',
      700: '#0f766e',
      800: '#115e59',
      900: '#134e4a',
      950: '#042f2e'
    },
    colorScheme: {
      light: {
        primary: {
          color: '{primary.700}',
          contrastColor: '#ffffff',
          hoverColor: '{primary.800}',
          activeColor: '{primary.800}'
        },
        surface: {
          0: '#ffffff',
          50: '#f8fafc',
          100: '#f1f5f9',
          200: '#e2e8f0',
          300: '#cbd5e1',
          400: '#94a3b8',
          500: '#64748b',
          600: '#475569',
          700: '#334155',
          800: '#1e293b',
          900: '#0f172a',
          950: '#020617'
        },
        content: { borderColor: '{surface.200}' },
        text: { color: '#0f172a', mutedColor: '#475569' },
        formField: {
          borderColor: '{surface.200}',
          hoverBorderColor: '{surface.400}',
          focusBorderColor: '{primary.700}'
        }
      }
    },
    focusRing: {
      width: '2px',
      style: 'solid',
      color: '{primary.700}',
      offset: '2px'
    },
    borderRadius: { sm: '4px', md: '6px', lg: '8px', xl: '12px' }
  }
});
