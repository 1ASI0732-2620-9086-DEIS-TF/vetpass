import { expect } from '@playwright/test';

export const API = process.env.VETPASS_API_URL ?? 'http://127.0.0.1:5199/api/v1';
export const STAFF = {
  email: 'andrea.quispe@vetsanmiguel.pe',
  password: process.env.VETPASS_DEMO_PASSWORD ?? 'VetPass.2026'
};

/** Fecha de hoy en Lima, como la ve la clínica: [ISO, dd/mm/aaaa]. */
export function today(offsetYears = 0, offsetDays = 0) {
  const lima = new Date(new Date().toLocaleString('en-US', { timeZone: 'America/Lima' }));
  lima.setFullYear(lima.getFullYear() + offsetYears);
  lima.setDate(lima.getDate() + offsetDays);
  const [d, m, y] = [lima.getDate(), lima.getMonth() + 1, lima.getFullYear()].map((n) => String(n).padStart(2, '0'));
  return { iso: `${y}-${m}-${d}`, text: `${d}/${m}/${y}` };
}

/** Un DNI que no existe todavía, para que cada ejecución registre su propio cliente. */
export const freshDni = () => String(70_000_000 + Math.floor(Math.random() * 9_999_999));

export async function signIn(page) {
  await page.addInitScript(() => localStorage.setItem('vetpass.idioma', 'es'));
  await page.goto('/acceso');
  await page.locator('#correo').fill(STAFF.email);
  await page.locator('#contrasena').fill(STAFF.password);
  await page.getByRole('button', { name: 'Ingresar' }).click();
  await expect(page.getByRole('heading', { name: 'Pacientes' })).toBeVisible();
}

/** Prepara datos por la API: un cliente nuevo y, si se pide, su mascota. */
export async function createClientWithPet(request, { pet, birthDate, email = null } = {}) {
  const session = await (await request.post(`${API}/authentication/sign-in`, { data: STAFF })).json();
  const headers = { Authorization: `Bearer ${session.accessToken}` };
  const name = `Cliente ${freshDni()}`;

  const client = await (await request.post(`${API}/clients`, {
    headers, data: { fullName: name, documentType: 'Dni', documentNumber: freshDni(), phoneNumber: '912345678', email }
  })).json();

  if (!pet) return { client };
  const created = await (await request.post(`${API}/pets`, {
    headers, data: { clientId: client.id, name: pet, species: 'Canine', breed: 'Mestizo', sex: 'Female', birthDate }
  })).json();
  return { client, pet: created };
}

export const row = (page, text) => page.locator('.p-datatable-tbody tr', { hasText: text }).first();
