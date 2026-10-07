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
    sinAtenciones: 'Sin atenciones',
    edad: 'Edad'
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
    guardado: '{nombre} quedó registrada con su cartilla.',
    telefonoAyuda: 'Celular de 9 dígitos, o fijo con su código de área.',
    telefonoInvalido: 'Ingresa un número peruano: un celular de 9 dígitos que empieza con 9, o un fijo con su código de área (01 para Lima).',
    nacimientoFuturo: 'La fecha de nacimiento no puede ser posterior a hoy.',
    nacimientoImplausible: 'Con esa fecha tendría {edad} años, por encima de los {maximo} que se admiten para la especie {especie}. Revisa el año.',
    clienteYaCreado: 'El cliente quedó registrado. Corrige los datos de la mascota y vuelve a guardar: se asociará al mismo cliente.',
    tipoDocumento: 'Documento',
    numeroDocumento: 'Número de documento',
    documentoAyuda: { Dni: 'DNI de 8 dígitos.', ForeignerCard: 'Entre 9 y 12 letras o dígitos.' },
    documentoInvalido: { Dni: 'El DNI debe tener exactamente 8 dígitos.', ForeignerCard: 'El carné de extranjería debe tener entre 9 y 12 letras o dígitos.' },
    clienteDuplicado: 'Este documento ya pertenece a {nombre}, registrado en la clínica.',
    usarCliente: 'Usar este cliente'
  },

  ficha: {
    cartilla: 'Cartilla',
    historial: 'Historial',
    dueno: 'Dueño',
    semanas: '{n} semanas',
    meses: '{n} meses',
    anos: '{n} años | {n} año | {n} años'
  },

  // Edad de calendario: «2 meses, 9 días», «3 años, 1 mes».
  edad: {
    dias: '{n} días | {n} día | {n} días',
    meses: '{n} meses | {n} mes | {n} meses',
    anos: '{n} años | {n} año | {n} años',
    union: '{a}, {b}',
    recienNacido: 'Recién nacido'
  },

  documento: {
    corto: { Dni: 'DNI', ForeignerCard: 'CE' },
    largo: { Dni: 'DNI', ForeignerCard: 'Carné de extranjería' }
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
    nota: 'Las dosis se ordenan según la secuencia del esquema. La fila destacada es la que corresponde aplicar hoy. Cada vacuna se registra en orden; vacunas distintas pueden aplicarse el mismo día.',
    antesLaDosis: 'Primero, la {dosis}'
  },

  dosis: {
    titulo: 'Registrar dosis aplicada',
    vacuna: 'Vacuna',
    fechaAplicacion: 'Fecha de aplicación',
    lote: 'Lote',
    veterinario: 'Veterinario responsable',
    valida: 'Cumple la edad mínima y el intervalo desde la dosis anterior.',
    aun_no: 'Antes del {fecha} no puede registrarse: aún no se cumple la edad mínima o el intervalo desde la dosis anterior.',
    futura: 'La fecha de aplicación no puede ser posterior a hoy, {fecha}.',
    registrar: 'Registrar dosis',
    otra: 'Registrar otra dosis del esquema',
    registrada: 'Dosis registrada en la cartilla de {nombre}.',
    fueraDeOrden: 'Antes de esta dosis debe registrarse la {dosis}: cada vacuna se aplica en orden.',
    historica: 'Si la dosis se aplicó en otra fecha, ingrésala: las siguientes de la serie se programarán desde hoy.'
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
    sinCorreo: 'Sin correo',
    acceso: 'Acceso a la aplicación',
    conAcceso: 'Con acceso',
    darAcceso: 'Dar acceso',
    dialogoTitulo: 'Dar acceso a la aplicación móvil',
    dialogoNota: 'Se creará la cuenta de {nombre} y el sistema generará una contraseña temporal. Entrégasela en recepción: solo se muestra una vez.',
    correoRequerido: 'Este cliente no tiene correo registrado. Ingresa uno para crear su cuenta.',
    crear: 'Crear cuenta',
    creada: 'Cuenta creada para {nombre}',
    contrasenaTemporal: 'Contraseña temporal',
    entregar: 'Anótala o cópiala ahora: no volverá a mostrarse.',
    copiar: 'Copiar',
    copiada: 'Copiada',
    documento: 'Documento',
    contrasena: 'Contraseña',
    sinAcceso: 'Sin acceso',
    restablecer: 'Restablecer',
    restablecerTitulo: 'Restablecer contraseña',
    restablecerNota: 'Se generará una contraseña temporal nueva para {nombre} y la actual dejará de funcionar. Al ingresar, la aplicación le pedirá elegir una propia.',
    restablecerConfirmar: 'Restablecer contraseña',
    restablecida: 'Contraseña restablecida para {nombre}',
    privacidad: 'VetPass no guarda ni muestra la contraseña de los clientes: solo puede reemplazarse por una temporal.'
  },

  estado: {
    UpToDate: 'Al día',
    Pending: 'Pendiente',
    Overdue: 'Vencida',
    Applied: 'Aplicada',
    cartilla: { UpToDate: 'Cartilla al día', Pending: 'Cartilla pendiente', Overdue: 'Cartilla vencida' }
  },

  especie: { Canine: 'Canina', Feline: 'Felina', canino: 'canino', felino: 'felino', canina: 'canina', felina: 'felina' },

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
    volver: 'Volver',
    terminos: 'Términos y condiciones'
  }
};
