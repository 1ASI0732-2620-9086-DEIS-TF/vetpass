import { describe, expect, it } from 'vitest';
import { problemOf } from './http';

const failure = (status, data) => ({ response: { status, data } });

describe('problemOf', () => {
  it('lee el código y los datos de una regla incumplida', () => {
    const problem = problemOf(failure(422, {
      code: 'minimum-age-not-reached', title: 'Regla del esquema incumplida', detail: 'Edad mínima no alcanzada.',
      ageInWeeks: 9, requiredWeeks: 12, earliestAdmissibleDate: '2026-10-28'
    }));

    expect(problem).toMatchObject({
      status: 422, code: 'minimum-age-not-reached', requiredWeeks: 12, earliestAdmissibleDate: '2026-10-28'
    });
  });

  it('expone el cliente existente de un documento repetido', () => {
    const problem = problemOf(failure(409, {
      code: 'duplicate-client', existingClientId: 'c1', existingClientName: 'Valeria Campos'
    }));

    expect(problem.existingClientName).toBe('Valeria Campos');
  });

  it('usa el primer error de validación cuando no hay detalle', () => {
    expect(problemOf(failure(400, { errors: { FullName: ['El nombre es obligatorio.'] } })).detail)
      .toBe('El nombre es obligatorio.');
  });

  it('distingue la falta de conexión', () => {
    expect(problemOf(new Error('Network Error'))).toMatchObject({ code: 'network', status: 0 });
  });
});
