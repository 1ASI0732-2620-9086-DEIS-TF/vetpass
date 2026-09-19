import axios from 'axios';

/**
 * Cliente HTTP hacia la RESTful API. La aplicación no habla con ningún otro
 * servicio: la autenticación, las reglas del esquema de vacunación y el
 * historial viven todos detrás de esta única dirección.
 */
export const http = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5199/api/v1',
  headers: { 'Content-Type': 'application/json' }
});

/**
 * Conecta el cliente con la sesión y con el router.
 *
 * Cada petición viaja con el token, y una respuesta 401 devuelve al inicio de
 * sesión: si la API dejó de reconocer al usuario, la aplicación no tiene nada
 * que mostrar.
 */
export function configureHttp({ getToken, onUnauthorized }) {
  http.interceptors.request.use((config) => {
    const token = getToken();
    if (token) config.headers.Authorization = `Bearer ${token}`;
    return config;
  });

  http.interceptors.response.use(
    (response) => response,
    (error) => {
      if (error.response?.status === 401) onUnauthorized();
      return Promise.reject(error);
    }
  );
}

/**
 * Extrae el mensaje de un ProblemDetails de la API. El dominio redacta esos
 * mensajes con el detalle que la interfaz necesita mostrar —la edad exigida,
 * la fecha más temprana admisible— de modo que se presentan tal cual.
 */
export function problemOf(error) {
  const datos = error?.response?.data;
  if (!datos) return { code: 'network', detail: null, status: 0 };

  return {
    status: error.response.status,
    code: datos.code ?? null,
    title: datos.title ?? null,
    detail: datos.detail ?? primerErrorDeValidacion(datos),
    ageInWeeks: datos.ageInWeeks,
    requiredWeeks: datos.requiredWeeks,
    earliestAdmissibleDate: datos.earliestAdmissibleDate
  };
}

function primerErrorDeValidacion(datos) {
  const errores = datos.errors;
  if (!errores) return null;
  const primera = Object.values(errores)[0];
  return Array.isArray(primera) ? primera[0] : String(primera);
}
