import { createI18n } from 'vue-i18n';
import es from './es';
import en from './en';

const STORAGE_KEY = 'vetpass.idioma';

/** El idioma recordado, o el del navegador, o español. */
function initialLanguage() {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved === 'es' || saved === 'en') return saved;
  } catch { /* sin almacenamiento disponible */ }

  return (navigator.language || 'es').toLowerCase().startsWith('en') ? 'en' : 'es';
}

export const i18n = createI18n({
  legacy: false,
  locale: initialLanguage(),
  fallbackLocale: 'es',
  messages: { es, en }
});

export function changeLanguage(language) {
  i18n.global.locale.value = language;
  document.documentElement.lang = language;
  try { localStorage.setItem(STORAGE_KEY, language); } catch { /* sin almacenamiento */ }
}

document.documentElement.lang = i18n.global.locale.value;

/** Formato de fecha del país, que es el que la cartilla en papel usa. */
export function formatDate(iso, language = i18n.global.locale.value) {
  if (!iso) return '—';
  const [year, month, day] = iso.slice(0, 10).split('-');
  return language === 'en' ? `${month}/${day}/${year}` : `${day}/${month}/${year}`;
}

export function isToday(iso) {
  return iso?.slice(0, 10) === new Date().toLocaleDateString('sv-SE');
}

export function todayISO() {
  return new Date().toLocaleDateString('sv-SE');
}

/** Ordinal del idioma activo: 1.ª / 1st, 2.ª / 2nd… */
function ordinal(n, language) {
  if (language !== 'en') return String(n);
  const rest100 = n % 100;
  if (rest100 >= 11 && rest100 <= 13) return `${n}th`;
  return `${n}${({ 1: 'st', 2: 'nd', 3: 'rd' })[n % 10] ?? 'th'}`;
}

/**
 * Etiqueta de una dosis en el idioma activo: «Quíntuple · 2.ª dosis» o
 * «DHPP · 2nd dose». La API entrega el nombre de la vacuna, el número de la
 * secuencia y si se trata de un refuerzo; la composición es de la interfaz,
 * porque es una decisión de idioma y no del dominio.
 */
export function doseLabel(dose, t, language = i18n.global.locale.value) {
  if (!dose) return '';

  const name = t(`vaccines.${dose.vaccineName}`, dose.vaccineName);
  const suffix = dose.isBooster
    ? t('doseBooster')
    : t('doseOrdinal', { n: ordinal(dose.sequenceNumber, language) });

  return `${name} · ${suffix}`;
}

/**
 * Teléfono para mostrar. La API lo entrega en su forma canónica (+51 seguido
 * del número nacional) y aquí solo se agrupan los dígitos como se leen en el
 * Perú. Un valor que no esté en esa forma —registrado antes de que existiera
 * la regla— se muestra tal cual, sin intentar corregirlo.
 */
export function formatPhone(value) {
  if (!value) return '—';
  const national = value.startsWith('+51') ? value.slice(3) : null;
  if (!national) return value;

  if (/^9\d{8}$/.test(national)) {
    return `+51 ${national.slice(0, 3)} ${national.slice(3, 6)} ${national.slice(6)}`;
  }
  if (/^1\d{7}$/.test(national)) {
    return `+51 1 ${national.slice(1, 4)} ${national.slice(4)}`;
  }
  if (/^\d{8}$/.test(national)) {
    return `+51 ${national.slice(0, 2)} ${national.slice(2, 5)} ${national.slice(5)}`;
  }
  return value;
}

function daysInMonth(year, month) {
  return new Date(Date.UTC(year, month, 0)).getUTCDate();
}

/** La fecha `meses` meses después; si ese día no existe, el último del mes (31/01 → 29/02). */
function addMonths([year, month, day], months) {
  const index = month - 1 + months;
  const a = year + Math.floor(index / 12);
  const m = (index % 12) + 1;
  return Date.UTC(a, m - 1, Math.min(day, daysInMonth(a, m)));
}

/**
 * Edad de calendario a partir de la fecha de nacimiento: «9 días»,
 * «2 meses, 9 días», «3 años, 1 mes». Se cuenta como se cuenta en la
 * consulta, por meses y días reales, no dividiendo semanas.
 */
export function formatAge(birthISO, t, today = todayISO()) {
  if (!birthISO) return '—';

  const birth = birthISO.slice(0, 10).split('-').map(Number);
  const [ah, mh, dh] = today.slice(0, 10).split('-').map(Number);
  const todayUTC = Date.UTC(ah, mh - 1, dh);

  // Meses cumplidos: los del calendario, menos uno si el día aún no llega.
  let elapsedMonths = (ah - birth[0]) * 12 + (mh - birth[1]);
  if (addMonths(birth, elapsedMonths) > todayUTC) elapsedMonths -= 1;

  const years = Math.floor(elapsedMonths / 12);
  const months = elapsedMonths % 12;
  const days = Math.round((todayUTC - addMonths(birth, elapsedMonths)) / 86400000);

  const part = (key, n) => t(`age.${key}`, n, { named: { n } });

  if (years >= 1) {
    return months > 0
      ? t('age.join', { a: part('years', years), b: part('months', months) })
      : part('years', years);
  }
  if (months >= 1) {
    return days > 0
      ? t('age.join', { a: part('months', months), b: part('days', days) })
      : part('months', months);
  }
  return days > 0 ? part('days', days) : t('age.newborn');
}

/** Documento de un cliente para mostrar: «DNI 45879123», «CE 001827364». */
export function formatDocument(client, t) {
  if (!client?.documentNumber) return '—';
  return `${t(`document.short.${client.documentType}`)} ${client.documentNumber}`;
}
