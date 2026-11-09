import 'package:flutter/foundation.dart';

/// Etiquetas en español e inglés, conforme a la sección 4.2.2 del informe.
/// La terminología sigue su tabla: Cartilla → Vaccination Card, Historial →
/// Record, Atención → Visit, Receta → Prescription.
class AppStrings {
  AppStrings(this.language);

  final String language;

  bool get isEnglish => language == 'en';

  String _(String es, String en) => isEnglish ? en : es;

  // Marca y accesos
  String get brand => 'VetPass';
  String get signInTitle => _('Ingresa a tu cuenta', 'Sign in to your account');
  String get signInNote => _(
      'Usa el correo y la contraseña que te entregó tu veterinaria.',
      'Use the email and password your clinic gave you.');
  String get email => _('Correo electrónico', 'Email address');
  String get password => _('Contraseña', 'Password');
  String get signIn => _('Ingresar', 'Sign in');
  String get signInHelp => _(
      '¿No tienes acceso? Solicítalo en la recepción de tu veterinaria.',
      'No access yet? Ask for it at your clinic’s reception desk.');
  String get invalidCredentials =>
      _('El correo o la contraseña no coinciden.', 'The email or password do not match.');
  String get sessionExpired => _(
      'Tu sesión expiró. Vuelve a ingresar.', 'Your session expired. Please sign in again.');
  String get terms => _('Términos y condiciones', 'Terms and conditions');
  String get offline => _(
      'No fue posible contactar al servidor.', 'The server could not be reached.');

  // Navegación
  String get myPets => _('Mis mascotas', 'My Pets');
  String get profile => _('Perfil', 'Profile');
  String get card => _('Cartilla', 'Vaccination Card');
  String get history => _('Historial', 'Record');
  String get visit => _('Atención', 'Visit');

  // Mis mascotas
  String get noPets => _(
      'Tu veterinaria todavía no registró ninguna mascota a tu nombre.',
      'Your clinic has not registered any pet under your name yet.');

  // Cartilla
  String get nextDose => _('Próxima dosis', 'Next dose');
  String get nothingPending =>
      _('No quedan dosis pendientes.', 'No doses are pending.');
  String get batch => _('Lote', 'Batch');
  String get expectedDate => _('Fecha esperada', 'Due date');
  String issuedRecord(String clinic) => _(
      'Registro emitido por $clinic.', 'Record issued by $clinic.');

  // Historial
  String get noVisits => _(
      'Todavía no hay atenciones registradas.', 'No visits have been recorded yet.');
  String get withPrescription => _('Con receta', 'With prescription');
  String get historyNote => _('Se muestran primero las atenciones más recientes.',
      'The most recent visits are shown first.');
  String get reason => _('Motivo de consulta', 'Reason for the visit');
  String get findings => _('Hallazgos', 'Findings');
  String get diagnosis => _('Diagnóstico', 'Diagnosis');
  String get treatment => _('Tratamiento indicado', 'Treatment');
  String get prescription => _('Receta', 'Prescription');
  String get weight => _('Peso', 'Weight');

  // Perfil
  String get linkedClinic => _('Veterinaria vinculada', 'Linked clinic');
  String get languageLabel => _('Idioma', 'Language');
  String get help => _('Ayuda', 'Help');
  String get signOut => _('Cerrar sesión', 'Sign out');

  // Contraseña (US17)
  String get changePassword => _('Cambiar contraseña', 'Change password');
  String get changePasswordNote => _(
      'Elige una contraseña que solo tú conozcas.',
      'Choose a password only you know.');
  String get requiredPasswordTitle =>
      _('Elige tu contraseña', 'Choose your password');
  String get requiredPasswordNote => _(
      'Tu veterinaria te entregó una contraseña temporal. Para continuar, reemplázala por una que solo tú conozcas.',
      'Your clinic gave you a temporary password. To continue, replace it with one only you know.');
  String get currentPassword => _('Contraseña actual', 'Current password');
  String get temporaryPassword => _('Contraseña temporal', 'Temporary password');
  String get newPassword => _('Contraseña nueva', 'New password');
  String get confirmPassword =>
      _('Repite la contraseña nueva', 'Repeat the new password');
  String get passwordPolicy => _(
      'Al menos 8 caracteres, con letras y números.',
      'At least 8 characters, with letters and digits.');
  String get passwordsDoNotMatch =>
      _('Las contraseñas no coinciden.', 'The passwords do not match.');
  String get currentPasswordIncorrect =>
      _('La contraseña actual no es correcta.', 'The current password is not correct.');
  String get weakPassword => _(
      'La contraseña nueva debe tener al menos 8 caracteres, con letras y números.',
      'The new password must have at least 8 characters, with letters and digits.');
  String get samePassword => _(
      'La contraseña nueva debe ser distinta de la actual.',
      'The new password must be different from the current one.');
  String get savePassword => _('Guardar contraseña', 'Save password');
  String get passwordUpdated =>
      _('Tu contraseña quedó actualizada.', 'Your password was updated.');
  String get show => _('Mostrar', 'Show');
  String get readOnly => _(
      'Esta aplicación es de consulta: la información clínica la registra tu veterinaria.',
      'This application is read-only: clinical information is recorded by your clinic.');

  // Estados de cartilla y de dosis
  String status(String value) => switch (value) {
        'UpToDate' => _('Al día', 'Up to date'),
        'Pending' => _('Pendiente', 'Pending'),
        'Overdue' => _('Vencida', 'Overdue'),
        'Applied' => _('Aplicada', 'Applied'),
        _ => value,
      };

  String cardStatus(String value) => switch (value) {
        'UpToDate' => _('Cartilla al día', 'Card up to date'),
        'Pending' => _('Cartilla pendiente', 'Card pending'),
        'Overdue' => _('Cartilla vencida', 'Card overdue'),
        _ => value,
      };

  String species(String value) => switch (value) {
        'Canine' => _('Canina', 'Canine'),
        'Feline' => _('Felina', 'Feline'),
        _ => value,
      };

  /// Nombres de vacuna: en inglés se conocen por su sigla.
  String vaccine(String name) => switch (name) {
        'Quíntuple' => _('Quíntuple', 'DHPP'),
        'Antirrábica' => _('Antirrábica', 'Rabies'),
        'Triple felina' => _('Triple felina', 'FVRCP'),
        'Leucemia felina' => _('Leucemia felina', 'FeLV'),
        _ => name,
      };

  /// Etiqueta completa de una dosis: «Quíntuple · 2.ª dosis» o «DHPP · 2nd dose».
  String doseLabel(String vaccineName, int sequence, bool isBooster) {
    final name = vaccine(vaccineName);
    if (isBooster) return '$name · ${_('refuerzo anual', 'annual booster')}';
    return '$name · ${isEnglish ? '${_ordinal(sequence)} dose' : '$sequence.ª dosis'}';
  }

  String _ordinal(int n) {
    if (n % 100 >= 11 && n % 100 <= 13) return '${n}th';
    return switch (n % 10) { 1 => '${n}st', 2 => '${n}nd', 3 => '${n}rd', _ => '${n}th' };
  }

  /// Edad en la unidad que el dueño usa para hablar de su mascota: semanas
  /// mientras es cachorro —que es cuando el esquema de vacunación se mide así—,
  /// meses durante el primer par de años y años a partir de ahí.
  String age(int weeks) {
    if (weeks < 16) return _('$weeks semanas', '$weeks weeks');

    if (weeks < 104) {
      final months = (weeks / 4.345).round();
      if (months == 1) return _('1 mes', '1 month');
      return _('$months meses', '$months months');
    }

    final years = weeks ~/ 52;
    if (years == 1) return _('1 año', '1 year');
    return _('$years años', '$years years');
  }
}

/// Idioma activo de la aplicación. Se recuerda entre sesiones y, la primera
/// vez, se toma del sistema operativo.
class LanguageController extends ValueNotifier<String> {
  LanguageController(super.value);

  AppStrings get texts => AppStrings(value);
}
