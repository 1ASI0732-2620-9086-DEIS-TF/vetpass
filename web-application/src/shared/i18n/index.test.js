import { beforeEach, describe, expect, it } from 'vitest';
import {
  i18n, cambiarIdioma, formatearEdad, formatearFecha, formatearTelefono, formatearDocumento, etiquetaDosis
} from './index';

const t = i18n.global.t;

describe('formatearEdad', () => {
  beforeEach(() => cambiarIdioma('es'));

  it.each([
    ['2026-09-28', '2026-10-07', '9 días'],
    ['2026-10-06', '2026-10-07', '1 día'],
    ['2026-08-05', '2026-10-07', '2 meses, 2 días'],
    ['2026-09-07', '2026-10-07', '1 mes'],
    ['2025-07-30', '2026-10-07', '1 año, 2 meses'],
    ['2022-10-07', '2026-10-07', '4 años'],
    ['2024-01-31', '2024-03-01', '1 mes, 1 día'],
    ['2026-10-07', '2026-10-07', 'Recién nacido']
  ])('%s al %s → %s', (nacimiento, hoy, esperado) => {
    expect(formatearEdad(nacimiento, t, hoy)).toBe(esperado);
  });

  it('se expresa en inglés', () => {
    cambiarIdioma('en');
    expect(formatearEdad('2026-08-05', t, '2026-10-07')).toBe('2 months, 2 days');
  });
});

describe('formatearFecha', () => {
  it('usa el formato de cada idioma', () => {
    expect(formatearFecha('2026-10-07', 'es')).toBe('07/10/2026');
    expect(formatearFecha('2026-10-07', 'en')).toBe('10/07/2026');
    expect(formatearFecha(null)).toBe('—');
  });
});

describe('formatearTelefono', () => {
  it.each([
    ['+51987654321', '+51 987 654 321'],
    ['+5112345678', '+51 1 234 5678'],
    ['+5144123456', '+51 44 123 456'],
    ['987654321', '987654321'],
    [null, '—']
  ])('%s → %s', (valor, esperado) => {
    expect(formatearTelefono(valor)).toBe(esperado);
  });
});

describe('formatearDocumento', () => {
  beforeEach(() => cambiarIdioma('es'));

  it('antepone el tipo abreviado', () => {
    expect(formatearDocumento({ documentType: 'Dni', documentNumber: '45879123' }, t)).toBe('DNI 45879123');
    expect(formatearDocumento({ documentType: 'ForeignerCard', documentNumber: '001827364' }, t)).toBe('CE 001827364');
    expect(formatearDocumento(null, t)).toBe('—');
  });
});

describe('etiquetaDosis', () => {
  it('compone vacuna y número de dosis en cada idioma', () => {
    cambiarIdioma('es');
    expect(etiquetaDosis({ vaccineName: 'Quíntuple', sequenceNumber: 2, isBooster: false }, t, 'es'))
      .toBe('Quíntuple · 2.ª dosis');
    expect(etiquetaDosis({ vaccineName: 'Antirrábica', sequenceNumber: 2, isBooster: true }, t, 'es'))
      .toBe('Antirrábica · refuerzo anual');

    cambiarIdioma('en');
    expect(etiquetaDosis({ vaccineName: 'Quíntuple', sequenceNumber: 2, isBooster: false }, t, 'en'))
      .toBe('DHPP · 2nd dose');
  });
});
