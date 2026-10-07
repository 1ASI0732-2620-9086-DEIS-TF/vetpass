/**
 * English labels. The terms come from the table in section 4.2.2 of the
 * report: Pacientes → Patients, Cartilla → Vaccination Card, Historial →
 * Record, Atención → Visit, Receta → Prescription.
 */
export default {
  marca: { nombre: 'VetPass', estudio: 'by PawCode Studio' },

  nav: {
    pacientes: 'Patients',
    clientes: 'Clients',
    cartillas: 'Vaccination Cards',
    buscarPaciente: 'Search patient',
    cerrarSesion: 'Sign out',
    idioma: 'Language'
  },

  acceso: {
    titulo: 'Your patient’s complete file, the moment they walk in',
    subtitulo: 'Vaccination card and veterinary record of dogs and cats, in one place and free of paper.',
    encabezado: 'Sign in to your clinic',
    nota: 'Access restricted to clinic staff.',
    correo: 'Email address',
    contrasena: 'Password',
    ingresar: 'Sign in',
    ayuda: 'Forgot your password? Write to support.',
    credencialesInvalidas: 'The email or the password do not match.',
    sinConexion: 'The server could not be reached. Check that the API is running.'
  },

  pacientes: {
    titulo: 'Patients',
    registrar: 'Add pet',
    especie: 'Species',
    estadoCartilla: 'Card status',
    todas: 'All',
    todos: 'All',
    conteo: 'patient | patients',
    mascota: 'Pet',
    dueno: 'Owner',
    ultimaAtencion: 'Last visit',
    sinResultados: 'No patients match that criterion.',
    sinResultadosAccion: 'If this is a new patient, register them.',
    sinAtenciones: 'No visits'
  },

  registro: {
    titulo: 'Add pet',
    datosCliente: 'Client details',
    datosMascota: 'Pet details',
    clienteExistente: 'Existing client',
    clienteNuevo: 'New client',
    seleccionaCliente: 'Select a client',
    nombreCompleto: 'Full name',
    telefono: 'Phone number',
    correo: 'Email address',
    nombre: 'Name',
    raza: 'Breed',
    fechaNacimiento: 'Date of birth',
    sexo: 'Sex',
    avisoTitulo: 'The vaccination card will be generated automatically',
    aviso: 'On saving, VetPass will create the card for {nombre} with the {especie} vaccination schedule and calculate the due date of every dose.',
    avisoSinNombre: 'On saving, VetPass will create the card with the schedule of its species and calculate the due date of every dose.',
    guardar: 'Save patient',
    guardado: '{nombre} was registered together with their vaccination card.',
    telefonoAyuda: '9-digit mobile, or landline with its area code.',
    telefonoInvalido: 'Enter a Peruvian number: a 9-digit mobile beginning with 9, or a landline with its area code (01 for Lima).',
    nacimientoFuturo: 'The date of birth cannot be later than today.',
    nacimientoImplausible: 'That date would make the pet {edad} years old, above the {maximo} admitted for the {especie} species. Please check the year.',
    clienteYaCreado: 'The client was registered. Correct the pet details and save again: it will be linked to the same client.'
  },

  ficha: {
    cartilla: 'Vaccination Card',
    historial: 'Record',
    dueno: 'Owner',
    semanas: '{n} weeks',
    meses: '{n} months',
    anos: '{n} years | {n} year | {n} years'
  },

  cartilla: {
    esquema: '{especie} vaccination schedule',
    proximaDosis: 'Next dose due: {fecha}',
    hoy: 'today, {fecha}',
    sinPendientes: 'No doses of the schedule are pending.',
    vacuna: 'Vaccine',
    fecha: 'Date',
    lote: 'Batch',
    responsable: 'Administered by',
    estado: 'Status',
    esperada: 'Due: {fecha}',
    registrarDosis: 'Record dose',
    nota: 'Doses follow the sequence of the schedule. The highlighted row is the one due today.'
  },

  dosis: {
    titulo: 'Record an applied dose',
    vacuna: 'Vaccine',
    fechaAplicacion: 'Date of application',
    lote: 'Batch',
    veterinario: 'Administered by',
    valida: 'Meets the minimum age and the interval since the previous dose.',
    aun_no: 'The schedule expects this dose on {fecha}. It cannot be recorded before that date.',
    futura: 'The date of application cannot be later than today, {fecha}.',
    registrar: 'Record dose',
    otra: 'Record another dose of the schedule',
    registrada: 'Dose recorded in the card of {nombre}.'
  },

  historial: {
    titulo: 'Recorded visits',
    nueva: 'New visit',
    conReceta: 'With prescription',
    vacio: 'This pet has no previous visits on record.',
    nota: 'The most recent visits are shown first.',
    motivo: 'Reason for the visit',
    hallazgos: 'Findings',
    diagnostico: 'Diagnosis',
    tratamiento: 'Treatment',
    peso: 'Weight',
    receta: 'Prescription'
  },

  atencion: {
    titulo: 'New visit',
    fecha: 'Date',
    veterinario: 'Attending veterinarian',
    peso: 'Weight (kg)',
    motivo: 'Reason for the visit',
    hallazgos: 'Findings',
    diagnostico: 'Diagnosis',
    tratamiento: 'Treatment',
    receta: 'Prescription',
    medicamento: 'Medication',
    dosificacion: 'Dosage',
    duracion: 'Duration',
    agregar: 'Add medication',
    quitar: 'Remove medication',
    avisoReceta: 'The prescription will be attached to this visit and visible to {dueno} in their mobile application.',
    guardar: 'Save visit',
    guardada: 'Visit recorded in the history of {nombre}.'
  },

  clientes: {
    titulo: 'Clients',
    nombre: 'Name',
    telefono: 'Phone number',
    correo: 'Email address',
    mascotas: 'Pets',
    sinCorreo: 'No email',
    acceso: 'Application access',
    conAcceso: 'Has access',
    darAcceso: 'Grant access',
    dialogoTitulo: 'Grant access to the mobile application',
    dialogoNota: 'The account for {nombre} will be created and the system will generate a temporary password. Hand it over at the reception desk: it is shown only once.',
    correoRequerido: 'This client has no email on record. Enter one to create their account.',
    crear: 'Create account',
    creada: 'Account created for {nombre}',
    contrasenaTemporal: 'Temporary password',
    entregar: 'Write it down or copy it now: it will not be shown again.',
    copiar: 'Copy',
    copiada: 'Copied'
  },

  estado: {
    UpToDate: 'Up to date',
    Pending: 'Pending',
    Overdue: 'Overdue',
    Applied: 'Applied',
    cartilla: { UpToDate: 'Card up to date', Pending: 'Card pending', Overdue: 'Card overdue' }
  },

  especie: { Canine: 'Canine', Feline: 'Feline', canino: 'canine', felino: 'feline', canina: 'canine', felina: 'feline' },

  vacunas: {
    'Quíntuple': 'DHPP',
    'Antirrábica': 'Rabies',
    'Triple felina': 'FVRCP',
    'Leucemia felina': 'FeLV'
  },
  dosisOrdinal: '{n} dose',
  dosisRefuerzo: 'annual booster',
  sexo: { Male: 'Male', Female: 'Female' },

  comun: {
    cancelar: 'Cancel',
    guardar: 'Save',
    cerrar: 'Close',
    cargando: 'Loading…',
    obligatorio: 'This field is required.',
    errorInesperado: 'Something went wrong while processing the request.',
    volver: 'Back'
  }
};
