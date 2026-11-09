/**
 * Etiquetas en español. Los términos siguen la tabla de la sección 4.2.2 del
 * informe, de modo que la interfaz hable el mismo idioma que el dominio.
 */
export default {
  brand: { name: 'VetPass', studio: 'por PawCode Studio' },

  nav: {
    patients: 'Pacientes',
    clients: 'Clientes',
    cards: 'Cartillas',
    searchPatient: 'Buscar paciente',
    signOut: 'Cerrar sesión',
    language: 'Idioma'
  },

  access: {
    title: 'El expediente completo de tu paciente, apenas entra por la puerta',
    subtitle: 'Cartilla de vacunación e historial veterinario de perros y gatos, en un solo lugar y sin depender del papel.',
    header: 'Ingresa a tu clínica',
    note: 'Acceso exclusivo para el personal de la clínica.',
    email: 'Correo electrónico',
    password: 'Contraseña',
    signIn: 'Ingresar',
    help: '¿Olvidaste tu contraseña? Escríbenos a soporte.',
    invalidCredentials: 'El correo o la contraseña no coinciden.',
    offline: 'No fue posible contactar al servidor. Verifica que la API esté en ejecución.'
  },

  patients: {
    title: 'Pacientes',
    register: 'Registrar mascota',
    species: 'Especie',
    cardStatus: 'Estado de cartilla',
    allSpecies: 'Todas',
    allStatuses: 'Todos',
    count: 'paciente | pacientes',
    pet: 'Mascota',
    owner: 'Dueño',
    lastVisit: 'Última atención',
    noResults: 'No se hallaron pacientes con ese criterio.',
    noResultsAction: 'Si es un paciente nuevo, regístralo.',
    noVisits: 'Sin atenciones',
    age: 'Edad'
  },

  registration: {
    title: 'Registrar mascota',
    clientDetails: 'Datos del cliente',
    petDetails: 'Datos de la mascota',
    existingClient: 'Cliente ya registrado',
    newClient: 'Cliente nuevo',
    selectClient: 'Selecciona un cliente',
    fullName: 'Nombres y apellidos',
    phone: 'Teléfono',
    email: 'Correo electrónico',
    name: 'Nombre',
    breed: 'Raza',
    birthDate: 'Fecha de nacimiento',
    sex: 'Sexo',
    noticeTitle: 'La cartilla se generará automáticamente',
    notice: 'Al guardar, VetPass creará la cartilla de {name} con el esquema de vacunación {species} y calculará la fecha esperada de cada dosis.',
    noticeWithoutName: 'Al guardar, VetPass creará la cartilla con el esquema de vacunación de su especie y calculará la fecha esperada de cada dosis.',
    save: 'Guardar paciente',
    saved: '{name} quedó registrada con su cartilla.',
    phoneHelp: 'Celular de 9 dígitos, o fijo con su código de área.',
    invalidPhone: 'Ingresa un número peruano: un celular de 9 dígitos que empieza con 9, o un fijo con su código de área (01 para Lima).',
    futureBirth: 'La fecha de nacimiento no puede ser posterior a hoy.',
    implausibleBirth: 'Con esa fecha tendría {age} años, por encima de los {max} que se admiten para la especie {species}. Revisa el año.',
    clientAlreadyCreated: 'El cliente quedó registrado. Corrige los datos de la mascota y vuelve a guardar: se asociará al mismo cliente.',
    documentType: 'Documento',
    documentNumber: 'Número de documento',
    documentHelp: { Dni: 'DNI de 8 dígitos.', ForeignerCard: 'Entre 9 y 12 letras o dígitos.' },
    invalidDocument: { Dni: 'El DNI debe tener exactamente 8 dígitos.', ForeignerCard: 'El carné de extranjería debe tener entre 9 y 12 letras o dígitos.' },
    duplicateClient: 'Este documento ya pertenece a {name}, registrado en la clínica.',
    useClient: 'Usar este cliente'
  },

  profile: {
    card: 'Cartilla',
    history: 'Historial',
    owner: 'Dueño',
    weeks: '{n} semanas',
    months: '{n} meses',
    years: '{n} años | {n} año | {n} años'
  },

  // Edad de calendario: «2 meses, 9 días», «3 años, 1 mes».
  age: {
    days: '{n} días | {n} día | {n} días',
    months: '{n} meses | {n} mes | {n} meses',
    years: '{n} años | {n} año | {n} años',
    join: '{a}, {b}',
    newborn: 'Recién nacido'
  },

  document: {
    short: { Dni: 'DNI', ForeignerCard: 'CE' },
    long: { Dni: 'DNI', ForeignerCard: 'Carné de extranjería' }
  },

  card: {
    schedule: 'Esquema de vacunación {species}',
    nextDose: 'Próxima dosis esperada: {date}',
    today: 'hoy, {date}',
    nothingPending: 'No quedan dosis pendientes en el esquema.',
    vaccine: 'Vacuna',
    date: 'Fecha',
    batch: 'Lote',
    veterinarian: 'Responsable',
    status: 'Estado',
    expected: 'Esperada: {date}',
    registerDose: 'Registrar dosis',
    note: 'Las dosis se ordenan según la secuencia del esquema. La fila destacada es la que corresponde aplicar hoy. Cada vacuna se registra en orden; vacunas distintas pueden aplicarse el mismo día.',
    beforeTheDose: 'Primero, la {dose}'
  },

  dose: {
    title: 'Registrar dosis aplicada',
    vaccine: 'Vacuna',
    applicationDate: 'Fecha de aplicación',
    batch: 'Lote',
    veterinarian: 'Veterinario responsable',
    valid: 'Cumple la edad mínima y el intervalo desde la dosis anterior.',
    notYet: 'Antes del {date} no puede registrarse: aún no se cumple la edad mínima o el intervalo desde la dosis anterior.',
    future: 'La fecha de aplicación no puede ser posterior a hoy, {date}.',
    register: 'Registrar dosis',
    another: 'Registrar otra dosis del esquema',
    registered: 'Dosis registrada en la cartilla de {name}.',
    outOfOrder: 'Antes de esta dosis debe registrarse la {dose}: cada vacuna se aplica en orden.',
    historical: 'Si la dosis se aplicó en otra fecha, ingrésala: las siguientes de la serie se programarán desde hoy.'
  },

  history: {
    title: 'Atenciones registradas',
    new: 'Nueva atención',
    withPrescription: 'Con receta',
    empty: 'Esta mascota no tiene atenciones previas registradas.',
    note: 'Se muestran primero las atenciones más recientes.',
    reason: 'Motivo de consulta',
    findings: 'Hallazgos',
    diagnosis: 'Diagnóstico',
    treatment: 'Tratamiento indicado',
    weight: 'Peso',
    prescription: 'Receta'
  },

  visit: {
    title: 'Nueva atención',
    date: 'Fecha',
    veterinarian: 'Veterinario responsable',
    weight: 'Peso (kg)',
    reason: 'Motivo de consulta',
    findings: 'Hallazgos',
    diagnosis: 'Diagnóstico',
    treatment: 'Tratamiento indicado',
    prescription: 'Receta',
    medication: 'Medicamento',
    dosage: 'Dosificación',
    duration: 'Duración',
    add: 'Agregar medicamento',
    remove: 'Quitar medicamento',
    prescriptionNotice: 'La receta quedará asociada a esta atención y visible para {owner} en su aplicación móvil.',
    save: 'Guardar atención',
    saved: 'Atención registrada en el historial de {name}.'
  },

  clients: {
    title: 'Clientes',
    name: 'Nombre',
    phone: 'Teléfono',
    email: 'Correo electrónico',
    pets: 'Mascotas',
    noEmail: 'Sin correo',
    access: 'Acceso a la aplicación',
    withAccess: 'Con acceso',
    grantAccess: 'Dar acceso',
    dialogTitle: 'Dar acceso a la aplicación móvil',
    dialogNote: 'Se creará la cuenta de {name} y el sistema generará una contraseña temporal. Entrégasela en recepción: solo se muestra una vez.',
    emailRequired: 'Este cliente no tiene correo registrado. Ingresa uno para crear su cuenta.',
    create: 'Crear cuenta',
    created: 'Cuenta creada para {name}',
    temporaryPassword: 'Contraseña temporal',
    handOver: 'Anótala o cópiala ahora: no volverá a mostrarse.',
    copy: 'Copiar',
    copied: 'Copiada',
    document: 'Documento',
    password: 'Contraseña',
    noAccess: 'Sin acceso',
    reset: 'Restablecer',
    resetTitle: 'Restablecer contraseña',
    resetNote: 'Se generará una contraseña temporal nueva para {name} y la actual dejará de funcionar. Al ingresar, la aplicación le pedirá elegir una propia.',
    resetConfirm: 'Restablecer contraseña',
    resetDone: 'Contraseña restablecida para {name}',
    privacy: 'VetPass no guarda ni muestra la contraseña de los clientes: solo puede reemplazarse por una temporal.'
  },

  status: {
    UpToDate: 'Al día',
    Pending: 'Pendiente',
    Overdue: 'Vencida',
    Applied: 'Aplicada',
    card: { UpToDate: 'Cartilla al día', Pending: 'Cartilla pendiente', Overdue: 'Cartilla vencida' }
  },

  species: { Canine: 'Canina', Feline: 'Felina', canineMasculine: 'canino', felineMasculine: 'felino', canineFeminine: 'canina', felineFeminine: 'felina' },

  // Nombres comerciales de las vacunas del esquema de SP01. Se traducen porque
  // en inglés se conocen por su sigla.
  vaccines: {
    'Quíntuple': 'Quíntuple',
    'Antirrábica': 'Antirrábica',
    'Triple felina': 'Triple felina',
    'Leucemia felina': 'Leucemia felina'
  },
  doseOrdinal: '{n}.ª dosis',
  doseBooster: 'refuerzo anual',
  sex: { Male: 'Macho', Female: 'Hembra' },

  common: {
    cancel: 'Cancelar',
    save: 'Guardar',
    close: 'Cerrar',
    loading: 'Cargando…',
    required: 'Este campo es obligatorio.',
    unexpectedError: 'Ocurrió un problema al procesar la solicitud.',
    back: 'Volver',
    terms: 'Términos y condiciones'
  }
};
