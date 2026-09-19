/**
 * Etiquetas en español. Los términos siguen la tabla de la sección 4.2.2 del
 * informe, de modo que la interfaz hable el mismo idioma que el dominio.
 */
export default {
  marca: { nombre: 'VetPass', estudio: 'por PawCode Studio' },

  nav: {
    pacientes: 'Pacientes',
    clientes: 'Clientes',
    cartillas: 'Cartillas',
    buscarPaciente: 'Buscar paciente',
    cerrarSesion: 'Cerrar sesión',
    idioma: 'Idioma'
  },

  acceso: {
    titulo: 'El expediente completo de tu paciente, apenas entra por la puerta',
    subtitulo: 'Cartilla de vacunación e historial veterinario de perros y gatos, en un solo lugar y sin depender del papel.',
    encabezado: 'Ingresa a tu clínica',
    nota: 'Acceso exclusivo para el personal de la clínica.',
    correo: 'Correo electrónico',
    contrasena: 'Contraseña',
    ingresar: 'Ingresar',
    ayuda: '¿Olvidaste tu contraseña? Escríbenos a soporte.',
    credencialesInvalidas: 'El correo o la contraseña no coinciden.',
    sinConexion: 'No fue posible contactar al servidor. Verifica que la API esté en ejecución.'
  },

  pacientes: {
    titulo: 'Pacientes',
    registrar: 'Registrar mascota',
    especie: 'Especie',
    estadoCartilla: 'Estado de cartilla',
    todas: 'Todas',
    todos: 'Todos',
    conteo: 'paciente | pacientes',
    mascota: 'Mascota',
    dueno: 'Dueño',
    ultimaAtencion: 'Última atención',
    sinResultados: 'No se hallaron pacientes con ese criterio.',
    sinResultadosAccion: 'Si es un paciente nuevo, regístralo.',
    sinAtenciones: 'Sin atenciones'
  },

  registro: {
    titulo: 'Registrar mascota',
    datosCliente: 'Datos del cliente',
    datosMascota: 'Datos de la mascota',
    clienteExistente: 'Cliente ya registrado',
    clienteNuevo: 'Cliente nuevo',
    seleccionaCliente: 'Selecciona un cliente',
    nombreCompleto: 'Nombres y apellidos',
    telefono: 'Teléfono',
    correo: 'Correo electrónico',
    nombre: 'Nombre',
    raza: 'Raza',
    fechaNacimiento: 'Fecha de nacimiento',
    sexo: 'Sexo',
    avisoTitulo: 'La cartilla se generará automáticamente',
    aviso: 'Al guardar, VetPass creará la cartilla de {nombre} con el esquema de vacunación {especie} y calculará la fecha esperada de cada dosis.',
    avisoSinNombre: 'Al guardar, VetPass creará la cartilla con el esquema de vacunación de su especie y calculará la fecha esperada de cada dosis.',
    guardar: 'Guardar paciente',
    guardado: '{nombre} quedó registrada con su cartilla.'
  },

  ficha: {
    cartilla: 'Cartilla',
    historial: 'Historial',
    dueno: 'Dueño',
    semanas: '{n} semanas',
    meses: '{n} meses',
    anos: '{n} años | {n} año | {n} años'
  },

  cartilla: {
    esquema: 'Esquema de vacunación {especie}',
    proximaDosis: 'Próxima dosis esperada: {fecha}',
    hoy: 'hoy, {fecha}',
    sinPendientes: 'No quedan dosis pendientes en el esquema.',
    vacuna: 'Vacuna',
    fecha: 'Fecha',
    lote: 'Lote',
    responsable: 'Responsable',
    estado: 'Estado',
    esperada: 'Esperada: {fecha}',
    registrarDosis: 'Registrar dosis',
    nota: 'Las dosis se ordenan según la secuencia del esquema. La fila destacada es la que corresponde aplicar hoy.'
  },

  dosis: {
    titulo: 'Registrar dosis aplicada',
    vacuna: 'Vacuna',
    fechaAplicacion: 'Fecha de aplicación',
    lote: 'Lote',
    veterinario: 'Veterinario responsable',
    valida: 'Cumple la edad mínima y el intervalo desde la dosis anterior.',
    aun_no: 'El esquema espera esta dosis el {fecha}. Antes de esa fecha no puede registrarse.',
    futura: 'La fecha de aplicación no puede ser posterior a hoy, {fecha}.',
    registrar: 'Registrar dosis',
    otra: 'Registrar otra dosis del esquema',
    registrada: 'Dosis registrada en la cartilla de {nombre}.'
  },

  historial: {
    titulo: 'Atenciones registradas',
    nueva: 'Nueva atención',
    conReceta: 'Con receta',
    vacio: 'Esta mascota no tiene atenciones previas registradas.',
    nota: 'Se muestran primero las atenciones más recientes.',
    motivo: 'Motivo de consulta',
    hallazgos: 'Hallazgos',
    diagnostico: 'Diagnóstico',
    tratamiento: 'Tratamiento indicado',
    peso: 'Peso',
    receta: 'Receta'
  },

  atencion: {
    titulo: 'Nueva atención',
    fecha: 'Fecha',
    veterinario: 'Veterinario responsable',
    peso: 'Peso (kg)',
    motivo: 'Motivo de consulta',
    hallazgos: 'Hallazgos',
    diagnostico: 'Diagnóstico',
    tratamiento: 'Tratamiento indicado',
    receta: 'Receta',
    medicamento: 'Medicamento',
    dosificacion: 'Dosificación',
    duracion: 'Duración',
    agregar: 'Agregar medicamento',
    quitar: 'Quitar medicamento',
    avisoReceta: 'La receta quedará asociada a esta atención y visible para {dueno} en su aplicación móvil.',
    guardar: 'Guardar atención',
    guardada: 'Atención registrada en el historial de {nombre}.'
  },

  clientes: {
    titulo: 'Clientes',
    nombre: 'Nombre',
    telefono: 'Teléfono',
    correo: 'Correo electrónico',
    mascotas: 'Mascotas',
    sinCorreo: 'Sin correo'
  },

  estado: {
    UpToDate: 'Al día',
    Pending: 'Pendiente',
    Overdue: 'Vencida',
    Applied: 'Aplicada',
    cartilla: { UpToDate: 'Cartilla al día', Pending: 'Cartilla pendiente', Overdue: 'Cartilla vencida' }
  },

  especie: { Canine: 'Canina', Feline: 'Felina', canino: 'canino', felino: 'felino' },

  // Nombres comerciales de las vacunas del esquema de SP01. Se traducen porque
  // en inglés se conocen por su sigla.
  vacunas: {
    'Quíntuple': 'Quíntuple',
    'Antirrábica': 'Antirrábica',
    'Triple felina': 'Triple felina',
    'Leucemia felina': 'Leucemia felina'
  },
  dosisOrdinal: '{n}.ª dosis',
  dosisRefuerzo: 'refuerzo anual',
  sexo: { Male: 'Macho', Female: 'Hembra' },

  comun: {
    cancelar: 'Cancelar',
    guardar: 'Guardar',
    cerrar: 'Cerrar',
    cargando: 'Cargando…',
    obligatorio: 'Este campo es obligatorio.',
    errorInesperado: 'Ocurrió un problema al procesar la solicitud.',
    volver: 'Volver'
  }
};
