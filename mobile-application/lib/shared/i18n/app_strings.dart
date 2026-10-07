import 'package:flutter/foundation.dart';

/// Etiquetas en español e inglés, conforme a la sección 4.2.2 del informe.
/// La terminología sigue su tabla: Cartilla → Vaccination Card, Historial →
/// Record, Atención → Visit, Receta → Prescription.
class AppStrings {
  AppStrings(this.idioma);

  final String idioma;

  bool get esIngles => idioma == 'en';

  String _(String es, String en) => esIngles ? en : es;

  // Marca y accesos
  String get marca => 'VetPass';
  String get accesoTitulo => _('Ingresa a tu cuenta', 'Sign in to your account');
  String get accesoNota => _(
      'Usa el correo y la contraseña que te entregó tu veterinaria.',
      'Use the email and password your clinic gave you.');
  String get correo => _('Correo electrónico', 'Email address');
  String get contrasena => _('Contraseña', 'Password');
  String get ingresar => _('Ingresar', 'Sign in');
  String get accesoAyuda => _(
      '¿No tienes acceso? Solicítalo en la recepción de tu veterinaria.',
      'No access yet? Ask for it at your clinic’s reception desk.');
  String get credencialesInvalidas =>
      _('El correo o la contraseña no coinciden.', 'The email or password do not match.');
  String get sinConexion => _(
      'No fue posible contactar al servidor.', 'The server could not be reached.');

  // Navegación
  String get misMascotas => _('Mis mascotas', 'My Pets');
  String get perfil => _('Perfil', 'Profile');
  String get cartilla => _('Cartilla', 'Vaccination Card');
  String get historial => _('Historial', 'Record');
  String get atencion => _('Atención', 'Visit');

  // Mis mascotas
  String get sinMascotas => _(
      'Tu veterinaria todavía no registró ninguna mascota a tu nombre.',
      'Your clinic has not registered any pet under your name yet.');

  // Cartilla
  String get proximaDosis => _('Próxima dosis', 'Next dose');
  String get sinPendientes =>
      _('No quedan dosis pendientes.', 'No doses are pending.');
  String get lote => _('Lote', 'Batch');
  String get fechaEsperada => _('Fecha esperada', 'Due date');
  String registroEmitido(String clinica) => _(
      'Registro emitido por $clinica.', 'Record issued by $clinica.');

  // Historial
  String get sinAtenciones => _(
      'Todavía no hay atenciones registradas.', 'No visits have been recorded yet.');
  String get conReceta => _('Con receta', 'With prescription');
  String get notaHistorial => _('Se muestran primero las atenciones más recientes.',
      'The most recent visits are shown first.');
  String get motivo => _('Motivo de consulta', 'Reason for the visit');
  String get hallazgos => _('Hallazgos', 'Findings');
  String get diagnostico => _('Diagnóstico', 'Diagnosis');
  String get tratamiento => _('Tratamiento indicado', 'Treatment');
  String get receta => _('Receta', 'Prescription');
  String get peso => _('Peso', 'Weight');

  // Perfil
  String get veterinariaVinculada => _('Veterinaria vinculada', 'Linked clinic');
  String get idiomaEtiqueta => _('Idioma', 'Language');
  String get ayuda => _('Ayuda', 'Help');
  String get cerrarSesion => _('Cerrar sesión', 'Sign out');

  // Contraseña (US17)
  String get cambiarContrasena => _('Cambiar contraseña', 'Change password');
  String get cambiarContrasenaNota => _(
      'Elige una contraseña que solo tú conozcas.',
      'Choose a password only you know.');
  String get contrasenaObligatoriaTitulo =>
      _('Elige tu contraseña', 'Choose your password');
  String get contrasenaObligatoriaNota => _(
      'Tu veterinaria te entregó una contraseña temporal. Para continuar, reemplázala por una que solo tú conozcas.',
      'Your clinic gave you a temporary password. To continue, replace it with one only you know.');
  String get contrasenaActual => _('Contraseña actual', 'Current password');
  String get contrasenaTemporal => _('Contraseña temporal', 'Temporary password');
  String get contrasenaNueva => _('Contraseña nueva', 'New password');
  String get contrasenaConfirmar =>
      _('Repite la contraseña nueva', 'Repeat the new password');
  String get contrasenaPolitica => _(
      'Al menos 8 caracteres, con letras y números.',
      'At least 8 characters, with letters and digits.');
  String get contrasenasNoCoinciden =>
      _('Las contraseñas no coinciden.', 'The passwords do not match.');
  String get contrasenaActualIncorrecta =>
      _('La contraseña actual no es correcta.', 'The current password is not correct.');
  String get contrasenaDebil => _(
      'La contraseña nueva debe tener al menos 8 caracteres, con letras y números.',
      'The new password must have at least 8 characters, with letters and digits.');
  String get contrasenaIgual => _(
      'La contraseña nueva debe ser distinta de la actual.',
      'The new password must be different from the current one.');
  String get guardarContrasena => _('Guardar contraseña', 'Save password');
  String get contrasenaActualizada =>
      _('Tu contraseña quedó actualizada.', 'Your password was updated.');
  String get mostrar => _('Mostrar', 'Show');
  String get soloConsulta => _(
      'Esta aplicación es de consulta: la información clínica la registra tu veterinaria.',
      'This application is read-only: clinical information is recorded by your clinic.');

  // Estados de cartilla y de dosis
  String estado(String valor) => switch (valor) {
        'UpToDate' => _('Al día', 'Up to date'),
        'Pending' => _('Pendiente', 'Pending'),
        'Overdue' => _('Vencida', 'Overdue'),
        'Applied' => _('Aplicada', 'Applied'),
        _ => valor,
      };

  String estadoCartilla(String valor) => switch (valor) {
        'UpToDate' => _('Cartilla al día', 'Card up to date'),
        'Pending' => _('Cartilla pendiente', 'Card pending'),
        'Overdue' => _('Cartilla vencida', 'Card overdue'),
        _ => valor,
      };

  String especie(String valor) => switch (valor) {
        'Canine' => _('Canina', 'Canine'),
        'Feline' => _('Felina', 'Feline'),
        _ => valor,
      };

  /// Nombres de vacuna: en inglés se conocen por su sigla.
  String vacuna(String nombre) => switch (nombre) {
        'Quíntuple' => _('Quíntuple', 'DHPP'),
        'Antirrábica' => _('Antirrábica', 'Rabies'),
        'Triple felina' => _('Triple felina', 'FVRCP'),
        'Leucemia felina' => _('Leucemia felina', 'FeLV'),
        _ => nombre,
      };

  /// Etiqueta completa de una dosis: «Quíntuple · 2.ª dosis» o «DHPP · 2nd dose».
  String etiquetaDosis(String nombreVacuna, int secuencia, bool esRefuerzo) {
    final nombre = vacuna(nombreVacuna);
    if (esRefuerzo) return '$nombre · ${_('refuerzo anual', 'annual booster')}';
    return '$nombre · ${esIngles ? '${_ordinal(secuencia)} dose' : '$secuencia.ª dosis'}';
  }

  String _ordinal(int n) {
    if (n % 100 >= 11 && n % 100 <= 13) return '${n}th';
    return switch (n % 10) { 1 => '${n}st', 2 => '${n}nd', 3 => '${n}rd', _ => '${n}th' };
  }

  /// Edad en la unidad que el dueño usa para hablar de su mascota: semanas
  /// mientras es cachorro —que es cuando el esquema de vacunación se mide así—,
  /// meses durante el primer par de años y años a partir de ahí.
  String edad(int semanas) {
    if (semanas < 16) return _('$semanas semanas', '$semanas weeks');

    if (semanas < 104) {
      final meses = (semanas / 4.345).round();
      if (meses == 1) return _('1 mes', '1 month');
      return _('$meses meses', '$meses months');
    }

    final anos = semanas ~/ 52;
    if (anos == 1) return _('1 año', '1 year');
    return _('$anos años', '$anos years');
  }
}

/// Idioma activo de la aplicación. Se recuerda entre sesiones y, la primera
/// vez, se toma del sistema operativo.
class IdiomaController extends ValueNotifier<String> {
  IdiomaController(super.value);

  AppStrings get textos => AppStrings(value);
}
