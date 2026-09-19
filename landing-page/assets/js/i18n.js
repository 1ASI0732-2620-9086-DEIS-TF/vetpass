/* ==========================================================================
   VetPass — Landing Page · internacionalización
   El español vive en el HTML y es lo que ve quien llega sin JavaScript o con
   un rastreador; este diccionario aporta la versión en inglés. La terminología
   sigue la tabla de etiquetas de la sección 4.2.2 del informe: Cartilla →
   Vaccination Card, Al día → Up to date, Aplicada → Applied, Pendiente →
   Pending.
   ========================================================================== */
window.VetPassI18n = (() => {
  'use strict';

  const EN = {
    'nav.problema': 'The problem',
    'nav.clinicas': 'For clinics',
    'nav.duenos': 'For owners',
    'nav.comoFunciona': 'How it works',
    'nav.preguntas': 'FAQ',
    'nav.ingresar': 'Sign in',

    'hero.badge': 'Dogs and cats',
    'hero.titulo': 'The vaccination card that never gets lost',
    'hero.lead': 'VetPass digitises the vaccination card and the veterinary record of your patients. Your clinic registers them from the web; the owner checks them on their phone.',
    'hero.ctaPrimario': 'Request a demo',
    'hero.ctaSecundario': 'See how it works',
    'hero.fuente': 'Source: INEI, 2026.',
    'hero.cifra': '<strong>64%</strong> of Peruvian households have at least one pet.',

    'demo.kiaraMeta': 'Canine · Shih tzu · 3 years',
    'demo.dosis1': 'DHPP',
    'demo.meta1': 'Batch A-4471 · 03/14/2026',
    'demo.dosis2': 'Rabies',
    'demo.meta2': 'Batch R-1180 · 04/11/2026',
    'demo.dosis3': 'Annual booster',
    'demo.meta3': 'Due date: 04/11/2027',
    'demo.pie': 'Record issued by the treating clinic.',
    'estado.alDia': 'Up to date',
    'estado.aplicada': 'Applied',
    'estado.pendiente': 'Pending',

    'problema.titulo': 'Paper always fails in the same way',
    'problema.lead': 'Three problems that every clinic working with printed forms and cards recognises at once.',
    'problema.1.titulo': 'It gets lost',
    'problema.1.texto': 'The card is misplaced, gets wet or falls apart. When that happens, the whole record of doses disappears and there is no way to recover it.',
    'problema.2.titulo': 'It gets split',
    'problema.2.texto': 'The clinic keeps the chart and the owner takes the card. Neither of them holds the complete file of the animal.',
    'problema.3.titulo': 'It gets repeated',
    'problema.3.texto': 'With no way to verify earlier doses, the schedule starts over. The client pays for it and trust suffers.',

    'clinicas.eyebrow': 'For veterinary clinics',
    'clinicas.titulo': 'The complete file, the moment the patient walks in',
    'clinicas.lead': 'Without migrating your historical archive and without modules your practice does not need.',
    'clinicas.1.titulo': 'The card builds itself',
    'clinicas.1.texto': 'When you register the pet, the system lays out its full schedule according to the species and calculates the due date of every dose.',
    'clinicas.2.titulo': 'Schedule validation',
    'clinicas.2.texto': 'If a dose does not meet the minimum age or the interval since the previous one, the system warns you before recording it.',
    'clinicas.3.titulo': 'Search by name',
    'clinicas.3.texto': 'Type the name of the pet or of its owner and open the complete record, even if the last visit was three years ago.',
    'clinicas.4.titulo': 'Who has an overdue dose',
    'clinicas.4.texto': 'Filter your patients by card status and put together the list for a campaign without going through notebook after notebook.',

    'duenos.eyebrow': 'For pet owners',
    'duenos.titulo': 'A benefit your clinic offers its client',
    'duenos.lead': 'The mobile application is free for the owner and only shows information recorded by the veterinarian.',
    'duenos.1.titulo': 'The card always at hand',
    'duenos.1.texto': 'Nothing to look for in a drawer, nothing to remember to bring to the appointment.',
    'duenos.2.titulo': 'Each pet on its own',
    'duenos.2.texto': 'The vaccination status of every animal is shown separately, with no confusion between one and another.',
    'duenos.3.titulo': 'Record and prescriptions',
    'duenos.3.texto': 'Previous visits and the indication of the veterinarian, available whenever they are needed.',

    'pasos.titulo': 'How it works',
    'pasos.1.titulo': 'The clinic registers the pet',
    'pasos.1.texto': 'Name, species and date of birth. Nothing else.',
    'pasos.2.titulo': 'VetPass builds its card',
    'pasos.2.texto': 'With the schedule of its species and the date of every dose already calculated.',
    'pasos.3.titulo': 'The owner checks it',
    'pasos.3.texto': 'From their phone, whenever they want, without depending on paper.',

    'faq.titulo': 'Frequently asked questions',
    'faq.1.p': 'Which species does the platform support?',
    'faq.1.r': 'Canine and feline. They are the two predominant species in companion animal care, and the ones whose vaccination schedule is standardised enough to be built into the system.',
    'faq.2.p': 'Can the owner modify the clinical information?',
    'faq.2.r': 'No. Recording and editing belong exclusively to the staff of the clinic. The owner holds consultation permissions only, so that the file keeps its value as a verifiable document.',
    'faq.3.p': 'Do I have to migrate my historical archive to start?',
    'faq.3.r': 'No. The platform starts building the file from the first patient you register, with no previous setup and no initial data load.',
    'faq.4.p': 'Does VetPass handle appointments, billing or inventory?',
    'faq.4.r': 'No. The platform concentrates on the vaccination card and the veterinary record. Appointments, payments, medication inventory and grooming services are deliberately out of its scope.',
    'faq.5.p': 'Who defines the vaccination schedule?',
    'faq.5.r': 'The system ships a template per species, with the minimum age and the interval between doses of every vaccine, drawn from the international reference guidelines.',

    'cta.titulo': 'Digitise the vaccination card of your patients',
    'cta.texto': 'Leave us your details and we will arrange a 20-minute demo at your clinic, in person or by video call.',
    'cta.yaTienes': 'Already have an account?',
    'cta.ingresa': 'Sign in to the application',

    'form.nombre': 'Full name',
    'form.clinica': 'Clinic name',
    'form.correo': 'Email address',
    'form.telefono': 'Phone number',
    'form.enviar': 'Request a demo',

    'footer.por': 'by PawCode Studio',
    'footer.clinicas': 'For clinics',
    'footer.duenos': 'For owners',
    'footer.contacto': 'Contact',
    'footer.terminos': 'Terms and conditions',
    'footer.privacidad': 'Privacy policy',
    'footer.lugar': 'Lima, Peru · 2026',

    'a11y.saltar': 'Skip to main content',
    'a11y.menu': 'Open the navigation menu',
    'a11y.inicio': 'VetPass, home',
    'a11y.navPrincipal': 'Main navigation',
    'a11y.navPie': 'Footer navigation',
    'a11y.cartilla': 'Example view of a vaccination card',
    'a11y.volver': 'Back to home',

    // Documento: título y descripción, que también cambian de idioma
    'meta.titulo': 'VetPass — Digital vaccination card and veterinary record',
    'meta.descripcion': 'Web and mobile platform for veterinary clinics. Digitise the vaccination card and the record of dogs and cats, and let your clients check them on their phone.',
    'meta.ogTitulo': 'VetPass — Digital vaccination card and veterinary record',
    'meta.ogDescripcion': 'Digitise the vaccination card and the record of your patients. Your clients check them on their phone.',

    // Mensajes que produce el formulario en tiempo de ejecución
    'validacion.nombre': 'Enter your full name.',
    'validacion.clinica': 'Enter the name of your clinic.',
    'validacion.correo': 'Enter a valid email address.',
    'validacion.telefono': 'Enter a contact phone number.',
    'form.exito': 'Thank you. We will contact you to arrange the demo.',

    'legal.volver': 'Back to home',
    'legal.terminosTitulo': 'Terms and conditions',
    'legal.privacidadTitulo': 'Privacy policy',
    'legal.pendiente': 'Document pending legal drafting. Its final content will be incorporated before the platform goes into production.',
    'legal.t1p': 'Scope of the service',
    'legal.t1r': 'VetPass is a platform for recording and consulting the vaccination card and the veterinary record of dogs and cats. It does not provide veterinary services and does not replace the professional judgement of the treating veterinarian.',
    'legal.t2p': 'Responsibility for the information',
    'legal.t2r': 'Clinical information is recorded by the staff of the veterinary clinic, who are responsible for its accuracy. The owner of the pet holds consultation permissions only.',
    'legal.p1p': 'Data the platform handles',
    'legal.p1r': 'Contact details of the client of the clinic, identification data of their pets and the clinical information recorded by the veterinary staff: applied doses, visits and prescriptions.',
    'legal.p2p': 'Who accesses the information',
    'legal.p2r': 'The staff of the clinic that registered the patient and the owner of the pet, each with their corresponding permissions. Clinical information is not shared with third parties.',
    'legal.inicio': 'Home'
  };

  const ALMACEN = 'vetpass.idioma';
  const original = new Map();
  let idiomaActual = 'es';

  /** Guarda el español del HTML la primera vez, para poder volver a él. */
  const recordar = (elemento, tipo, valor) => {
    if (!original.has(elemento)) original.set(elemento, {});
    const guardado = original.get(elemento);
    if (!(tipo in guardado)) guardado[tipo] = valor;
  };

  const traducir = (idioma) => {
    const esIngles = idioma === 'en';

    document.querySelectorAll('[data-i18n]').forEach((elemento) => {
      const clave = elemento.dataset.i18n;
      recordar(elemento, 'texto', elemento.textContent);
      const texto = esIngles ? EN[clave] : original.get(elemento).texto;
      if (texto) elemento.textContent = texto;
    });

    // Algunas frases llevan marcado dentro —una cifra en negrita, por ejemplo—
    // y se sustituyen como HTML. El contenido proviene de este archivo, nunca
    // de una fuente externa.
    document.querySelectorAll('[data-i18n-html]').forEach((elemento) => {
      const clave = elemento.dataset.i18nHtml;
      recordar(elemento, 'html', elemento.innerHTML);
      const contenido = esIngles ? EN[clave] : original.get(elemento).html;
      if (contenido) elemento.innerHTML = contenido;
    });

    document.querySelectorAll('[data-i18n-aria]').forEach((elemento) => {
      const clave = elemento.dataset.i18nAria;
      recordar(elemento, 'aria', elemento.getAttribute('aria-label'));
      const texto = esIngles ? EN[clave] : original.get(elemento).aria;
      if (texto) elemento.setAttribute('aria-label', texto);
    });

    // El idioma del documento cambia con el contenido: de ello dependen los
    // lectores de pantalla y la separación silábica del navegador.
    document.documentElement.lang = idioma;

    const meta = (selector, clave) => {
      const etiqueta = document.querySelector(selector);
      if (!etiqueta) return;
      recordar(etiqueta, 'contenido', etiqueta.getAttribute('content'));
      const texto = esIngles ? EN[clave] : original.get(etiqueta).contenido;
      if (texto) etiqueta.setAttribute('content', texto);
    };

    if (!original.has(document)) original.set(document, { texto: document.title });
    document.title = esIngles ? (EN['meta.titulo'] || document.title) : original.get(document).texto;
    meta('meta[name="description"]', 'meta.descripcion');
    meta('meta[property="og:title"]', 'meta.ogTitulo');
    meta('meta[property="og:description"]', 'meta.ogDescripcion');

    document.querySelectorAll('.lang__btn').forEach((boton) => {
      boton.setAttribute('aria-pressed', String(boton.dataset.lang === idioma));
    });

    idiomaActual = idioma;
    try { localStorage.setItem(ALMACEN, idioma); } catch { /* navegación privada */ }

    document.dispatchEvent(new CustomEvent('vetpass:idioma', { detail: { idioma } }));
  };

  /** Idioma inicial: el de la URL, el recordado, o el del navegador. */
  const idiomaInicial = () => {
    const enUrl = new URLSearchParams(location.search).get('lang');
    if (enUrl === 'en' || enUrl === 'es') return enUrl;

    try {
      const guardado = localStorage.getItem(ALMACEN);
      if (guardado === 'en' || guardado === 'es') return guardado;
    } catch { /* sin almacenamiento disponible */ }

    return (navigator.language || 'es').toLowerCase().startsWith('en') ? 'en' : 'es';
  };

  document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.lang__btn').forEach((boton) => {
      boton.addEventListener('click', () => traducir(boton.dataset.lang));
    });
    traducir(idiomaInicial());
  });

  return {
    get idioma() { return idiomaActual; },
    texto: (clave, respaldo) => (idiomaActual === 'en' ? (EN[clave] || respaldo) : respaldo),
    cambiar: traducir
  };
})();
