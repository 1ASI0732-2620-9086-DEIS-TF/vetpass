import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../shared/infrastructure/api_client.dart';

/// Perfil del dueño de la mascota tal como lo devuelve la API.
class User {
  User({
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

  User withOwnPassword() => User(
        id: id,
        email: email,
        fullName: fullName,
        role: role,
        clientId: clientId,
        clinicName: clinicName,
      );

  factory User.fromJson(Map<String, dynamic> json) => User(
        id: json['id'] as String,
        email: json['email'] as String,
        fullName: json['fullName'] as String,
        role: json['role'] as String,
        clientId: json['clientId'] as String?,
        clinicName: json['clinicName'] as String?,
        requiresPasswordChange: json['requiresPasswordChange'] as bool? ?? false,
      );

  Map<String, dynamic> toJson() => {
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
    _api.renewSession = _renew;
  }

  static const _key = 'vetpass.sesion';

  final ApiClient _api;
  String? _token;
  String? _refresh;
  User? _user;
  bool _loading = false;
  bool _expired = false;
  Future<bool>? _renewal;

  ApiClient get api => _api;
  User? get user => _user;
  bool get authenticated => _token != null;
  bool get mustChangePassword => _user?.requiresPasswordChange ?? false;

  /// La sesión terminó porque venció y no pudo renovarse, no porque el dueño
  /// la cerrara: el inicio de sesión lo explica.
  bool get expired => _expired;
  bool get loading => _loading;

  Future<void> restore() async {
    final preferences = await SharedPreferences.getInstance();
    final saved = preferences.getString(_key);
    if (saved == null) return;

    try {
      final details = jsonDecode(saved) as Map<String, dynamic>;
      _token = details['token'] as String?;
      _refresh = details['refresh'] as String?;
      _user = User.fromJson(details['user'] as Map<String, dynamic>);
      _api.token = _token;
      notifyListeners();
    } catch (_) {
      await signOut();
    }
  }

  Future<void> signIn(String email, String password) async {
    _loading = true;
    notifyListeners();
    try {
      final details = await _api.post('/authentication/sign-in',
          {'email': email, 'password': password}, signedOut: true) as Map<String, dynamic>;

      _token = details['accessToken'] as String;
      _refresh = details['refreshToken'] as String?;
      _expired = false;
      _user = User.fromJson(details['user'] as Map<String, dynamic>);
      _api.token = _token;

      await _save();
    } finally {
      _loading = false;
      notifyListeners();
    }
  }

  /// Reemplaza la contraseña por una propia. La API verifica la actual y aplica
  /// la política; aquí solo se registra que la cuenta ya no tiene una temporal.
  Future<void> changePassword(String currentPassword, String newPassword) async {
    await _api.post('/authentication/password',
        {'currentPassword': currentPassword, 'newPassword': newPassword});

    _user = _user?.withOwnPassword();
    await _save();
    notifyListeners();
  }

  /// Renueva el token de acceso con el de refresco. Si varias peticiones
  /// llegan vencidas a la vez, comparten una sola renovación: el token de
  /// refresco solo sirve una vez.
  Future<bool> _renew() => _renewal ??= _renewNow().whenComplete(() => _renewal = null);

  Future<bool> _renewNow() async {
    final refresh = _refresh;
    if (refresh == null) {
      await signOut(expired: true);
      return false;
    }

    try {
      final details = await _api.post('/authentication/refresh', {'refreshToken': refresh},
          signedOut: true) as Map<String, dynamic>;
      _token = details['accessToken'] as String;
      _refresh = details['refreshToken'] as String?;
      _api.token = _token;
      await _save();
      return true;
    } on ApiException catch (failure) {
      // Sin red no se cierra la sesión: el dueño reintenta cuando vuelva la
      // conexión. Un rechazo de la API sí la termina.
      if (!failure.noNetwork) await signOut(expired: true);
      return false;
    }
  }

  Future<void> _save() async {
    final preferences = await SharedPreferences.getInstance();
    await preferences.setString(_key,
        jsonEncode({'token': _token, 'refresh': _refresh, 'user': _user!.toJson()}));
  }

  Future<void> signOut({bool expired = false}) async {
    _expired = expired;
    _token = null;
    _refresh = null;
    _user = null;
    _api.token = null;
    final preferences = await SharedPreferences.getInstance();
    await preferences.remove(_key);
    notifyListeners();
  }
}
