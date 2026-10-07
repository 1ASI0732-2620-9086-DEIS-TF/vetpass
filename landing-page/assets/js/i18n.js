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
    'validacion.telefono': 'Enter a Peruvian number: a 9-digit mobile or a landline with its area code.',
    'form.exito': 'Thank you. We will contact you to arrange the demo.',

    'legal.volver': 'Back to home',
    'legal.terminosTitulo': 'Terms and conditions',
    'legal.privacidadTitulo': 'Privacy policy',
    'legal.pendiente': 'Document pending legal drafting. Its final content will be incorporated before the platform goes into production.',
    'legal.saasIntro': "This agreement governs the use of VetPass, a platform that PawCode Studio offers as software as a service (SaaS). By using the VetPass web application, mobile application or API, the veterinary clinic and every user accept these conditions.",
    'legal.saasFecha': "Last updated: October 7, 2026.",
    'legal.s1p': "1. Parties",
    'legal.s1r': "PawCode Studio provides the platform. The veterinary clinic is the subscriber: it contracts the service and registers its staff and its clients. Users are the clinic staff, who record clinical information from the web application, and the pet owner, who consults it from the mobile application.",
    'legal.s2p': "2. Scope of the service",
    'legal.s2r': "VetPass lets clinics register clients and pets, generate the vaccination card according to the schedule of each species, record doses, visits and prescriptions, and lets the owner consult that information. It does not provide veterinary services and does not replace the professional judgement of the treating veterinarian.",
    'legal.s3p': "3. Accounts and credentials",
    'legal.s3r': "The clinic creates the accounts of its staff and the access of each owner, who receives a temporary password that must be replaced with their own on first sign-in. Credentials are personal and non-transferable. VetPass does not store passwords in readable form: if an owner forgets theirs, the clinic can reset it but cannot see it.",
    'legal.s4p': "4. Rights of the users",
    'legal.s4r': "The owner can consult the vaccination card, record and prescriptions of their pets at any time, and ask their clinic to correct inaccurate data. Every holder of personal data may exercise their rights of access, rectification, cancellation and objection under Peruvian Law No. 29733, the Personal Data Protection Law, before the clinic that registered their data.",
    'legal.s5p': "5. Obligations of the clinic",
    'legal.s5r': "To record truthful and complete information, with the actual date of each application, its batch and the responsible veterinarian; to inform its clients about the processing of their data and obtain their consent; to safeguard the credentials of its staff; and to use the platform only for its veterinary practice.",
    'legal.s6p': "6. Obligations of the pet owner",
    'legal.s6r': "To keep their password confidential, to review the information about their pets and to report to their clinic any error they find.",
    'legal.s7p': "7. Restrictions of use",
    'legal.s7r': "It is not allowed to access or attempt to access information of another clinic or of other people's pets, to record false information, to circumvent security or permission controls, to extract information by automated means, to reverse engineer the platform, or to use it for purposes other than veterinary care.",
    'legal.s8p': "8. Vaccination schedule rules",
    'legal.s8r': "The platform validates every dose against the schedule of its species, based on the WSAVA guidelines: minimum age, interval between doses and order of each series. These validations support, but do not replace, the judgement of the veterinarian, who decides the application of each vaccine.",
    'legal.s9p': "9. Personal data and its location",
    'legal.s9r': "The clinic is the owner of the database of its clients and PawCode Studio acts as data processor, only to provide the service. Data is hosted on third-party cloud services located in the United States (Supabase and Vercel), which constitutes a cross-border flow of personal data. Data is neither sold nor transferred to third parties.",
    'legal.s10p': "10. Availability of the service",
    'legal.s10r': "VetPass is in a pilot stage. PawCode Studio seeks its continuous availability but does not guarantee a service level: there may be interruptions for maintenance or for reasons beyond its control.",
    'legal.s11p': "11. Intellectual property",
    'legal.s11r': "The VetPass software, brand and designs belong to PawCode Studio. The clinical information recorded belongs to the clinic and its clients; PawCode Studio does not use it for its own purposes.",
    'legal.s12p': "12. Suspension and termination",
    'legal.s12r': "PawCode Studio may suspend the access of a user who breaches this agreement. The clinic may terminate the service at any time and request a copy of its information before it is deleted.",
    'legal.s13p': "13. Limitation of liability",
    'legal.s13r': "To the extent permitted by law, PawCode Studio is not liable for clinical decisions, for the accuracy of the information recorded by the clinic or for damages arising from interruptions of the service.",
    'legal.s14p': "14. Changes",
    'legal.s14r': "Changes to this agreement are published on this page with their update date. If a change is substantial, clinics will be informed before it takes effect.",
    'legal.s15p': "15. Governing law and inquiries",
    'legal.s15r': "This agreement is governed by the laws of the Republic of Peru; for pet owners, the Consumer Protection and Defense Code, Law No. 29571, also applies. Owners address their inquiries to their veterinary clinic, and the clinic to PawCode Studio through the channel agreed when contracting the service.",
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
