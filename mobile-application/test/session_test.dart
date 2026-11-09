import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'package:vetpass_mobile/iam/application/session.dart';
import 'package:vetpass_mobile/main.dart';
import 'package:vetpass_mobile/shared/i18n/app_strings.dart';
import 'package:vetpass_mobile/shared/infrastructure/api_client.dart';

const _base = 'https://api.test/api/v1';

final _user = {
  'id': 'u1',
  'email': 'valeria.campos@correo.com',
  'fullName': 'Valeria Campos',
  'role': 'PetOwner',
  'clientId': 'c1',
  'clinicName': 'Veterinaria San Miguel',
  'requiresPasswordChange': false,
};

http.Response _json(int status, Object body) => http.Response(jsonEncode(body), status,
    headers: {'content-type': 'application/json; charset=utf-8'});

/// API simulada: el token de acceso «vencido» recibe 401; el de refresco
/// «r1» se canjea una sola vez por el token «nuevo».
class _ApiSimulada {
  int renovaciones = 0;
  bool rejectRenewal = false;
  final peticiones = <String>[];

  late final client = MockClient((request) async {
    final path = request.url.path.replaceFirst('/api/v1', '');
    final token = request.headers['Authorization'];
    peticiones.add('${request.method} $path ${token ?? '-'}');

    switch (path) {
      case '/authentication/sign-in':
        return _json(200, {'accessToken': 'vencido', 'refreshToken': 'r1', 'user': _user});
      case '/authentication/refresh':
        renovaciones++;
        final refresh = (jsonDecode(request.body) as Map)['refreshToken'];
        if (rejectRenewal || refresh != 'r1') {
          return _json(401, {'code': 'invalid-credentials'});
        }
        return _json(200, {'accessToken': 'nuevo', 'refreshToken': 'r2'});
      case '/me/pets':
        return token == 'Bearer nuevo' ? _json(200, [{'name': 'Kiara'}]) : _json(401, {});
    }
    return _json(404, {});
  });
}

void main() {
  setUp(() => SharedPreferences.setMockInitialValues({}));

  test('renueva el token vencido y repite la petición', () async {
    final api = _ApiSimulada();
    final session = Session(ApiClient(baseUrl: _base, client: api.client));
    await session.signIn('valeria.campos@correo.com', 'key');

    final pets = await session.api.get('/me/pets') as List;

    expect(pets.single['name'], 'Kiara');
    expect(api.renovaciones, 1);
    expect(session.authenticated, isTrue);
  });

  test('peticiones simultáneas comparten una sola renovación', () async {
    final api = _ApiSimulada();
    final session = Session(ApiClient(baseUrl: _base, client: api.client));
    await session.signIn('valeria.campos@correo.com', 'key');

    await Future.wait([session.api.get('/me/pets'), session.api.get('/me/pets')]);

    expect(api.renovaciones, 1);
  });

  test('si la renovación es rechazada, cierra la sesión y lo indica', () async {
    final api = _ApiSimulada()..rejectRenewal = true;
    final session = Session(ApiClient(baseUrl: _base, client: api.client));
    await session.signIn('valeria.campos@correo.com', 'key');

    await expectLater(session.api.get('/me/pets'),
        throwsA(isA<ApiException>().having((e) => e.status, 'status', 401)));
    expect(session.authenticated, isFalse);
    expect(session.expired, isTrue);
  });

  test('iniciar y renovar la sesión no envían el token', () async {
    final api = _ApiSimulada();
    final session = Session(ApiClient(baseUrl: _base, client: api.client));
    await session.signIn('valeria.campos@correo.com', 'key');
    await session.api.get('/me/pets');

    expect(api.peticiones.where((p) => p.contains('/authentication/')),
        everyElement(endsWith(' -')));
  });

  testWidgets('sin sesión, la aplicación abre el inicio de sesión', (tester) async {
    final session = Session(ApiClient(baseUrl: _base, client: _ApiSimulada().client));
    await tester.pumpWidget(VetPassApp(session: session, language: LanguageController('es')));

    expect(find.text('Ingresa a tu cuenta'), findsOneWidget);
    expect(find.byType(TextField), findsNWidgets(2));
  });
}
