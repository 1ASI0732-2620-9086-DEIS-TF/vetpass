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
  });

  final String id;
  final String email;
  final String fullName;
  final String role;
  final String? clientId;
  final String? clinicName;

  factory Usuario.desdeJson(Map<String, dynamic> json) => Usuario(
        id: json['id'] as String,
        email: json['email'] as String,
        fullName: json['fullName'] as String,
        role: json['role'] as String,
        clientId: json['clientId'] as String?,
        clinicName: json['clinicName'] as String?,
      );

  Map<String, dynamic> aJson() => {
        'id': id,
        'email': email,
        'fullName': fullName,
        'role': role,
        'clientId': clientId,
        'clinicName': clinicName,
      };
}

/// Sesión del dueño. La aplicación solo consulta: el rol que la API concede a
/// esta cuenta no permite registrar información clínica (US05-E1).
class Session extends ChangeNotifier {
  Session(this._api);

  static const _clave = 'vetpass.sesion';

  final ApiClient _api;
  String? _token;
  Usuario? _usuario;
  bool _cargando = false;

  ApiClient get api => _api;
  Usuario? get usuario => _usuario;
  bool get autenticado => _token != null;
  bool get cargando => _cargando;

  Future<void> restaurar() async {
    final preferencias = await SharedPreferences.getInstance();
    final guardado = preferencias.getString(_clave);
    if (guardado == null) return;

    try {
      final datos = jsonDecode(guardado) as Map<String, dynamic>;
      _token = datos['token'] as String?;
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
          {'email': correo, 'password': contrasena}) as Map<String, dynamic>;

      _token = datos['accessToken'] as String;
      _usuario = Usuario.desdeJson(datos['user'] as Map<String, dynamic>);
      _api.token = _token;

      final preferencias = await SharedPreferences.getInstance();
      await preferencias.setString(
          _clave, jsonEncode({'token': _token, 'usuario': _usuario!.aJson()}));
    } finally {
      _cargando = false;
      notifyListeners();
    }
  }

  Future<void> cerrarSesion() async {
    _token = null;
    _usuario = null;
    _api.token = null;
    final preferencias = await SharedPreferences.getInstance();
    await preferencias.remove(_clave);
    notifyListeners();
  }
}
