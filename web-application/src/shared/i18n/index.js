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

/**
 * Teléfono para mostrar. La API lo entrega en su forma canónica (+51 seguido
 * del número nacional) y aquí solo se agrupan los dígitos como se leen en el
 * Perú. Un valor que no esté en esa forma —registrado antes de que existiera
 * la regla— se muestra tal cual, sin intentar corregirlo.
 */
export function formatearTelefono(valor) {
  if (!valor) return '—';
  const nacional = valor.startsWith('+51') ? valor.slice(3) : null;
  if (!nacional) return valor;

  if (/^9\d{8}$/.test(nacional)) {
    return `+51 ${nacional.slice(0, 3)} ${nacional.slice(3, 6)} ${nacional.slice(6)}`;
  }
  if (/^1\d{7}$/.test(nacional)) {
    return `+51 1 ${nacional.slice(1, 4)} ${nacional.slice(4)}`;
  }
  if (/^\d{8}$/.test(nacional)) {
    return `+51 ${nacional.slice(0, 2)} ${nacional.slice(2, 5)} ${nacional.slice(5)}`;
  }
  return valor;
}

function diasDelMes(ano, mes) {
  return new Date(Date.UTC(ano, mes, 0)).getUTCDate();
}

/** La fecha `meses` meses después; si ese día no existe, el último del mes (31/01 → 29/02). */
function sumarMeses([ano, mes, dia], meses) {
  const indice = mes - 1 + meses;
  const a = ano + Math.floor(indice / 12);
  const m = (indice % 12) + 1;
  return Date.UTC(a, m - 1, Math.min(dia, diasDelMes(a, m)));
}

/**
 * Edad de calendario a partir de la fecha de nacimiento: «9 días»,
 * «2 meses, 9 días», «3 años, 1 mes». Se cuenta como se cuenta en la
 * consulta, por meses y días reales, no dividiendo semanas.
 */
export function formatearEdad(nacimientoISO, t, hoy = hoyISO()) {
  if (!nacimientoISO) return '—';

  const nacimiento = nacimientoISO.slice(0, 10).split('-').map(Number);
  const [ah, mh, dh] = hoy.slice(0, 10).split('-').map(Number);
  const hoyUTC = Date.UTC(ah, mh - 1, dh);

  // Meses cumplidos: los del calendario, menos uno si el día aún no llega.
  let mesesCumplidos = (ah - nacimiento[0]) * 12 + (mh - nacimiento[1]);
  if (sumarMeses(nacimiento, mesesCumplidos) > hoyUTC) mesesCumplidos -= 1;

  const anos = Math.floor(mesesCumplidos / 12);
  const meses = mesesCumplidos % 12;
  const dias = Math.round((hoyUTC - sumarMeses(nacimiento, mesesCumplidos)) / 86400000);

  const parte = (clave, n) => t(`edad.${clave}`, n, { named: { n } });

  if (anos >= 1) {
    return meses > 0
      ? t('edad.union', { a: parte('anos', anos), b: parte('meses', meses) })
      : parte('anos', anos);
  }
  if (meses >= 1) {
    return dias > 0
      ? t('edad.union', { a: parte('meses', meses), b: parte('dias', dias) })
      : parte('meses', meses);
  }
  return dias > 0 ? parte('dias', dias) : t('edad.recienNacido');
}

/** Documento de un cliente para mostrar: «DNI 45879123», «CE 001827364». */
export function formatearDocumento(cliente, t) {
  if (!cliente?.documentNumber) return '—';
  return `${t(`documento.corto.${cliente.documentType}`)} ${cliente.documentNumber}`;
}
