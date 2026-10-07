import { test, expect } from '@playwright/test';
import { signIn, today, freshDni, createClientWithPet, row } from './support';

test.beforeEach(async ({ page }) => signIn(page));

test('el personal ve los pacientes con su estado y su edad @mobile', async ({ page }) => {
  await expect(row(page, 'Rocky')).toContainText('Pendiente');
  await expect(row(page, 'Simón')).toContainText('Vencida');
  await expect(row(page, 'Kiara')).toContainText('Al día');
  await expect(row(page, 'Rocky')).toContainText(/2 meses, \d+ días?/);
});

test('registra un cliente y una mascota adulta: su cartilla empieza hoy', async ({ page }) => {
  await page.getByRole('button', { name: 'Registrar mascota' }).first().click();
  await page.getByText('Cliente nuevo').click();
  await page.locator('#c-nombre').fill('Lucía Paredes');
  await page.locator('#c-documento').fill(freshDni());
  await page.locator('#c-telefono').fill('923 456 789');
  await page.locator('#m-nombre').fill('Max');
  await page.locator('#m-nacimiento').fill(today(-4).text);
  await page.locator('#m-nacimiento').press('Escape');
  await page.getByRole('button', { name: 'Guardar paciente' }).click();

  await expect(page.getByText('Esquema de vacunación')).toBeVisible();
  await expect(row(page, 'Quíntuple · 1.ª dosis')).toContainText(`Esperada: ${today().text}`);
  await expect(row(page, 'Antirrábica · 1.ª dosis')).toContainText(`Esperada: ${today().text}`);
  await expect(row(page, 'Quíntuple · 2.ª dosis')).toContainText('Primero, la Quíntuple · 1.ª dosis');
});

test('bloquea las dosis que el esquema no admite', async ({ page }) => {
  await row(page, 'Rocky').click();

  await expect(row(page, 'Quíntuple · 3.ª dosis')).toContainText('Primero, la Quíntuple · 2.ª dosis');
  await row(page, 'Antirrábica · 1.ª dosis').getByRole('button', { name: 'Registrar dosis' }).click();
  await expect(page.getByRole('dialog')).toContainText('no puede registrarse');
  await page.locator('#d-lote').fill('R-1300');
  await expect(page.getByRole('dialog').getByRole('button', { name: 'Registrar dosis' })).toBeDisabled();
});

test('registra una dosis válida', async ({ page, request }) => {
  const { pet } = await createClientWithPet(request, { pet: 'Nube', birthDate: today(0, -42).iso });
  await page.goto(`/pacientes/${pet.id}`);

  await row(page, 'Quíntuple · 1.ª dosis').getByRole('button', { name: 'Registrar dosis' }).click();
  await expect(page.getByRole('dialog')).toContainText('Cumple la edad mínima');
  await page.locator('#d-lote').fill('A-4521');
  await page.getByRole('dialog').getByRole('button', { name: 'Registrar dosis' }).click();

  await expect(row(page, 'Quíntuple · 1.ª dosis')).toContainText('Aplicada');
  await expect(row(page, 'Quíntuple · 1.ª dosis')).toContainText('A-4521');
});

test('registra una atención con receta y la muestra en el historial', async ({ page, request }) => {
  const { pet } = await createClientWithPet(request, { pet: 'Luna', birthDate: today(-1).iso });
  await page.goto(`/pacientes/${pet.id}`);

  await page.getByRole('tab', { name: 'Historial' }).click();
  await page.getByRole('button', { name: 'Nueva atención' }).click();
  await page.locator('#a-motivo').fill('Control anual');
  await page.locator('#a-diagnostico').fill('Paciente sano');
  await page.getByRole('button', { name: 'Agregar medicamento' }).click();
  await page.getByLabel('Medicamento').fill('Praziquantel 50 mg');
  await page.getByLabel('Dosificación').fill('Una tableta');
  await page.getByLabel('Duración').fill('Dosis única');
  await page.getByRole('button', { name: 'Guardar atención' }).click();

  await expect(page.getByText('Control anual')).toBeVisible();
  await expect(page.getByText('Con receta')).toBeVisible();
});

test('un documento repetido ofrece usar al cliente existente', async ({ page }) => {
  await page.goto('/pacientes/nuevo');
  await page.getByText('Cliente nuevo').click();
  await page.locator('#c-nombre').fill('Valeria C.');
  await page.locator('#c-documento').fill('45879123');
  await page.locator('#c-telefono').fill('987 111 222');
  await page.locator('#m-nombre').fill('Nube');
  await page.locator('#m-nacimiento').fill(today(-1).text);
  await page.locator('#m-nacimiento').press('Escape');
  await page.getByRole('button', { name: 'Guardar paciente' }).click();

  await expect(page.getByText('Este documento ya pertenece a Valeria Campos')).toBeVisible();
  await page.getByRole('button', { name: 'Usar este cliente' }).click();
  await expect(page.getByText('Valeria Campos · DNI 45879123')).toBeVisible();
});

test('da acceso a la app móvil y restablece la contraseña', async ({ page, request }) => {
  const email = `cliente.${freshDni()}@vetpass.test`;
  const { client } = await createClientWithPet(request, { email });
  await page.goto('/clientes');

  await row(page, client.fullName).getByRole('button', { name: 'Dar acceso' }).click();
  await page.getByRole('button', { name: 'Crear cuenta' }).click();
  const primera = await page.locator('.contrasena__valor').innerText();
  expect(primera).toHaveLength(12);
  await page.getByRole('button', { name: 'Cerrar' }).click();

  await row(page, client.fullName).getByRole('button', { name: 'Restablecer' }).click();
  await page.getByRole('button', { name: 'Restablecer contraseña' }).click();
  const segunda = await page.locator('.contrasena__valor').innerText();
  expect(segunda).toHaveLength(12);
  expect(segunda).not.toBe(primera);
});

test('cambia el idioma a inglés', async ({ page }) => {
  await page.getByRole('button', { name: 'EN', exact: true }).click();

  await expect(page.getByRole('heading', { name: 'Patients' })).toBeVisible();
  await expect(page.getByRole('columnheader', { name: 'Age' })).toBeVisible();
});
