import { test, expect } from '@playwright/test';

test.beforeEach(async ({ page }) => page.goto('/'));

test('presenta la propuesta de valor (US01)', async ({ page }) => {
  await expect(page.getByRole('heading', { level: 1, name: 'La cartilla de vacunación que no se pierde' })).toBeVisible();
  await expect(page.locator('#problema h2')).toBeVisible();
  await expect(page.locator('#como-funciona h2')).toBeVisible();
});

test('lleva a la información para clínicas (US02)', async ({ page }) => {
  await page.getByRole('link', { name: 'Para clínicas' }).first().click();

  await expect(page).toHaveURL(/#clinicas$/);
  await expect(page.getByRole('heading', { name: 'El expediente completo, apenas entra el paciente' })).toBeInViewport();
});

test('da acceso a la aplicación web (US03)', async ({ page }) => {
  await expect(page.getByRole('link', { name: 'Ingresar' })).toHaveAttribute('href', /^http:\/\/localhost:5173/);
});

test('cambia el idioma a inglés', async ({ page }) => {
  await page.getByRole('button', { name: 'EN', exact: true }).click();

  await expect(page.getByRole('heading', { level: 1, name: 'The vaccination card that never gets lost' })).toBeVisible();
  await expect(page.locator('html')).toHaveAttribute('lang', /^en/);
});
