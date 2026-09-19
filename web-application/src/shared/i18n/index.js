import { createI18n } from 'vue-i18n';
import es from './es';
import en from './en';

const ALMACEN = 'vetpass.idioma';

/** El idioma recordado, o el del navegador, o español. */
function idiomaInicial() {
  try {
    const guardado = localStorage.getItem(ALMACEN);
    if (guardado === 'es' || guardado === 'en') return guardado;
  } catch { /* sin almacenamiento disponible */ }

  return (navigator.language || 'es').toLowerCase().startsWith('en') ? 'en' : 'es';
}

export const i18n = createI18n({
  legacy: false,
  locale: idiomaInicial(),
  fallbackLocale: 'es',
  messages: { es, en }
});

export function cambiarIdioma(idioma) {
  i18n.global.locale.value = idioma;
  document.documentElement.lang = idioma;
  try { localStorage.setItem(ALMACEN, idioma); } catch { /* sin almacenamiento */ }
}

document.documentElement.lang = i18n.global.locale.value;

/** Formato de fecha del país, que es el que la cartilla en papel usa. */
export function formatearFecha(iso, idioma = i18n.global.locale.value) {
  if (!iso) return '—';
  const [ano, mes, dia] = iso.slice(0, 10).split('-');
  return idioma === 'en' ? `${mes}/${dia}/${ano}` : `${dia}/${mes}/${ano}`;
}

export function esHoy(iso) {
  return iso?.slice(0, 10) === new Date().toLocaleDateString('sv-SE');
}

export function hoyISO() {
  return new Date().toLocaleDateString('sv-SE');
}

/** Ordinal del idioma activo: 1.ª / 1st, 2.ª / 2nd… */
function ordinal(n, idioma) {
  if (idioma !== 'en') return String(n);
  const resto100 = n % 100;
  if (resto100 >= 11 && resto100 <= 13) return `${n}th`;
  return `${n}${({ 1: 'st', 2: 'nd', 3: 'rd' })[n % 10] ?? 'th'}`;
}

/**
 * Etiqueta de una dosis en el idioma activo: «Quíntuple · 2.ª dosis» o
 * «DHPP · 2nd dose». La API entrega el nombre de la vacuna, el número de la
 * secuencia y si se trata de un refuerzo; la composición es de la interfaz,
 * porque es una decisión de idioma y no del dominio.
 */
export function etiquetaDosis(dosis, t, idioma = i18n.global.locale.value) {
  if (!dosis) return '';

  const nombre = t(`vacunas.${dosis.vaccineName}`, dosis.vaccineName);
  const sufijo = dosis.isBooster
    ? t('dosisRefuerzo')
    : t('dosisOrdinal', { n: ordinal(dosis.sequenceNumber, idioma) });

  return `${nombre} · ${sufijo}`;
}
