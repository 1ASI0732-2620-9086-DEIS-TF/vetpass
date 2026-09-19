/* ==========================================================================
   VetPass — Landing Page
   La página no contiene reglas de negocio: solo navegación, validación de
   forma del formulario de contacto y el acceso a la aplicación web (US03).
   ========================================================================== */
(() => {
  'use strict';

  /**
   * Dirección de la aplicación web de la clínica (US03). Es el único punto
   * que hay que tocar cuando la aplicación se despliegue: mientras tanto
   * apunta al servidor de desarrollo.
   */
  const WEB_APP_URL = 'http://localhost:5173';

  document.querySelectorAll('[data-app-link]').forEach((enlace) => {
    enlace.href = WEB_APP_URL;
  });

  /* ----------------------------------------------- menú en pantalla chica */
  const toggle = document.querySelector('.nav-toggle');
  const nav = document.querySelector('.nav');

  const cerrarMenu = () => {
    nav.classList.remove('is-open');
    toggle.setAttribute('aria-expanded', 'false');
  };

  toggle?.addEventListener('click', () => {
    const abierto = nav.classList.toggle('is-open');
    toggle.setAttribute('aria-expanded', String(abierto));
  });

  nav?.querySelectorAll('a').forEach((enlace) => enlace.addEventListener('click', cerrarMenu));

  document.addEventListener('keydown', (evento) => {
    if (evento.key === 'Escape' && nav.classList.contains('is-open')) {
      cerrarMenu();
      toggle.focus();
    }
  });

  /* ------------------------------------- sombra del encabezado al bajar */
  const header = document.querySelector('.site-header');
  const marcarDesplazamiento = () => {
    header.classList.toggle('is-scrolled', window.scrollY > 8);
  };
  marcarDesplazamiento();
  window.addEventListener('scroll', marcarDesplazamiento, { passive: true });

  /* ------------------------ sección visible resaltada en la navegación */
  const enlaces = new Map();
  document.querySelectorAll('.nav__list a[href^="#"]').forEach((enlace) => {
    const seccion = document.querySelector(enlace.getAttribute('href'));
    if (seccion) enlaces.set(seccion, enlace);
  });

  if ('IntersectionObserver' in window && enlaces.size > 0) {
    const observador = new IntersectionObserver((entradas) => {
      entradas.forEach((entrada) => {
        if (!entrada.isIntersecting) return;
        enlaces.forEach((enlace) => enlace.classList.remove('is-active'));
        enlaces.get(entrada.target)?.classList.add('is-active');
      });
    }, { rootMargin: '-45% 0px -50% 0px' });

    enlaces.forEach((_, seccion) => observador.observe(seccion));
  }

  /* --------------------------------------------- formulario de contacto */
  const formulario = document.querySelector('#form-demo');
  if (!formulario) return;

  const estado = document.querySelector('#form-status');

  const reglas = {
    nombre: (valor) => (valor.trim().length >= 3 ? '' : 'Ingresa tu nombre y apellido.'),
    clinica: (valor) => (valor.trim().length >= 2 ? '' : 'Ingresa el nombre de tu clínica.'),
    correo: (valor) => (/^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(valor.trim())
      ? '' : 'Ingresa un correo electrónico válido.'),
    telefono: (valor) => (valor.replace(/\D/g, '').length >= 6
      ? '' : 'Ingresa un número de teléfono de contacto.')
  };

  /** Comunica la validación junto al campo que la origina (4.1.2). */
  const validarCampo = (campo) => {
    const mensaje = reglas[campo.name](campo.value);
    const error = document.querySelector(`#error-${campo.name}`);

    campo.setAttribute('aria-invalid', mensaje ? 'true' : 'false');
    error.textContent = mensaje;
    error.hidden = mensaje === '';

    return mensaje === '';
  };

  formulario.querySelectorAll('input').forEach((campo) => {
    // Al salir del campo la primera vez, y en cada tecla una vez marcado.
    campo.addEventListener('blur', () => validarCampo(campo));
    campo.addEventListener('input', () => {
      if (campo.getAttribute('aria-invalid') === 'true') validarCampo(campo);
    });
  });

  formulario.addEventListener('submit', (evento) => {
    evento.preventDefault();
    estado.textContent = '';

    const campos = [...formulario.querySelectorAll('input')];
    const validos = campos.map(validarCampo).every(Boolean);

    if (!validos) {
      campos.find((campo) => campo.getAttribute('aria-invalid') === 'true')?.focus();
      return;
    }

    // El envío real requiere un servicio de correo que está fuera del alcance
    // de esta versión: la página es estática y no expone ningún endpoint.
    formulario.reset();
    campos.forEach((campo) => campo.removeAttribute('aria-invalid'));
    estado.textContent = 'Gracias. Te contactaremos para coordinar la demostración.';
  });
})();
