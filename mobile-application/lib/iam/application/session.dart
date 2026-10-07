import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../shared/infrastructure/api_client.dart';

/// Perfil del dueño de la mascota tal como lo devuelve la API.
class Usuario {
  Usuario({
    required this.id,
    required this.email,
    required this.fullName,
    required this.role,
    this.clientId,
    this.clinicName,
    this.requiresPasswordChange = false,
  });

  final String id;
  final String email;
  final String fullName;
  final String role;
  final String? clientId;
  final String? clinicName;

  /// La cuenta tiene una contraseña temporal que conoce la recepción: el dueño
  /// debe elegir una propia antes de usar la aplicación (US17).
  final bool requiresPasswordChange;

  Usuario conContrasenaPropia() => Usuario(
        id: id,
        email: email,
        fullName: fullName,
        role: role,
        clientId: clientId,
        clinicName: clinicName,
      );

  factory Usuario.desdeJson(Map<String, dynamic> json) => Usuario(
        id: json['id'] as String,
        email: json['email'] as String,
        fullName: json['fullName'] as String,
        role: json['role'] as String,
        clientId: json['clientId'] as String?,
        clinicName: json['clinicName'] as String?,
        requiresPasswordChange: json['requiresPasswordChange'] as bool? ?? false,
      );

  Map<String, dynamic> aJson() => {
        'id': id,
        'email': email,
        'fullName': fullName,
        'role': role,
        'clientId': clientId,
        'clinicName': clinicName,
        'requiresPasswordChange': requiresPasswordChange,
      };
}

/// Sesión del dueño. La aplicación solo consulta: el rol que la API concede a
/// esta cuenta no permite registrar información clínica (US05-E1).
class Session extends ChangeNotifier {
  Session(this._api) {
    _api.renovarSesion = _renovar;
  }

  static const _clave = 'vetpass.sesion';

  final ApiClient _api;
  String? _token;
  String? _refresh;
  Usuario? _usuario;
  bool _cargando = false;
  bool _expirada = false;
  Future<bool>? _renovacion;

  ApiClient get api => _api;
  Usuario? get usuario => _usuario;
  bool get autenticado => _token != null;
  bool get debeCambiarContrasena => _usuario?.requiresPasswordChange ?? false;

  /// La sesión terminó porque venció y no pudo renovarse, no porque el dueño
  /// la cerrara: el inicio de sesión lo explica.
  bool get expirada => _expirada;
  bool get cargando => _cargando;

  Future<void> restaurar() async {
    final preferencias = await SharedPreferences.getInstance();
    final guardado = preferencias.getString(_clave);
    if (guardado == null) return;

    try {
      final datos = jsonDecode(guardado) as Map<String, dynamic>;
      _token = datos['token'] as String?;
      _refresh = datos['refresh'] as String?;
      _usuario = Usuario.desdeJson(datos['usuario'] as Map<String, dynamic>);
      _api.token = _token;
      notifyListeners();
    } catch (_) {
      await cerrarSesion();
    }
  }

  Future<void> iniciarSesion(String correo, String contrasena) async {
    _cargando = true;
    notifyListeners();
    try {
      final datos = await _api.post('/authentication/sign-in',
          {'email': correo, 'password': contrasena}, sinSesion: true) as Map<String, dynamic>;

      _token = datos['accessToken'] as String;
      _refresh = datos['refreshToken'] as String?;
      _expirada = false;
      _usuario = Usuario.desdeJson(datos['user'] as Map<String, dynamic>);
      _api.token = _token;

      await _guardar();
    } finally {
      _cargando = false;
      notifyListeners();
    }
  }

  /// Reemplaza la contraseña por una propia. La API verifica la actual y aplica
  /// la política; aquí solo se registra que la cuenta ya no tiene una temporal.
  Future<void> cambiarContrasena(String actual, String nueva) async {
    await _api.post('/authentication/password',
        {'currentPassword': actual, 'newPassword': nueva});

    _usuario = _usuario?.conContrasenaPropia();
    await _guardar();
    notifyListeners();
  }

  /// Renueva el token de acceso con el de refresco. Si varias peticiones
  /// llegan vencidas a la vez, comparten una sola renovación: el token de
  /// refresco solo sirve una vez.
  Future<bool> _renovar() => _renovacion ??= _renovarAhora().whenComplete(() => _renovacion = null);

  Future<bool> _renovarAhora() async {
    final refresh = _refresh;
    if (refresh == null) {
      await cerrarSesion(expirada: true);
      return false;
    }

    try {
      final datos = await _api.post('/authentication/refresh', {'refreshToken': refresh},
          sinSesion: true) as Map<String, dynamic>;
      _token = datos['accessToken'] as String;
      _refresh = datos['refreshToken'] as String?;
      _api.token = _token;
      await _guardar();
      return true;
    } on ApiException catch (fallo) {
      // Sin red no se cierra la sesión: el dueño reintenta cuando vuelva la
      // conexión. Un rechazo de la API sí la termina.
      if (!fallo.sinRed) await cerrarSesion(expirada: true);
      return false;
    }
  }

  Future<void> _guardar() async {
    final preferencias = await SharedPreferences.getInstance();
    await preferencias.setString(_clave,
        jsonEncode({'token': _token, 'refresh': _refresh, 'usuario': _usuario!.aJson()}));
  }

  Future<void> cerrarSesion({bool expirada = false}) async {
    _expirada = expirada;
    _token = null;
    _refresh = null;
    _usuario = null;
    _api.token = null;
    final preferencias = await SharedPreferences.getInstance();
    await preferencias.remove(_clave);
    notifyListeners();
  }
}
