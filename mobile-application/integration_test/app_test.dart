import 'dart:convert';
import 'dart:math';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:integration_test/integration_test.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'package:vetpass_mobile/main.dart' as app;

/// System tests of the mobile application (section 6.1.4): the real app on an
/// Android emulator, against the API and local Supabase. Never production.
const _api = String.fromEnvironment('VETPASS_API', defaultValue: 'http://10.0.2.2:5199/api/v1');
const _demoPassword = String.fromEnvironment('VETPASS_DEMO_PASSWORD', defaultValue: 'VetPass.2026');
const _owner = 'valeria.campos@correo.com';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  Future<void> openApp(WidgetTester tester) async {
    final preferences = await SharedPreferences.getInstance();
    await preferences.clear();
    await preferences.setString('vetpass.idioma', 'es');
    await app.main();
    await waitFor(tester, find.text('Ingresa a tu cuenta'));
  }

  Future<void> signIn(WidgetTester tester, String email, String password) async {
    await tester.enterText(find.byType(TextField).at(0), email);
    await tester.enterText(find.byType(TextField).at(1), password);
    await tester.tap(find.text('Ingresar'));
  }

  testWidgets('the owner consults the card, the record and a prescription', (tester) async {
    await openApp(tester);
    await signIn(tester, _owner, _demoPassword);

    await waitFor(tester, find.text('Kiara'));
    await tester.tap(find.text('Simón'));
    await waitFor(tester, find.text('Cartilla vencida'));

    await tester.pageBack();
    await waitFor(tester, find.text('Kiara'));
    await tester.tap(find.text('Kiara'));
    await waitFor(tester, find.widgetWithText(Tab, 'Historial'));
    await settle(tester);
    await tester.tap(find.widgetWithText(Tab, 'Historial'));
    await tester.pumpAndSettle();
    final visit = find.textContaining('Enrojecimiento');
    await tester.ensureVisible(visit);
    await tester.tap(visit);
    await waitFor(tester, find.text('Cefalexina 250 mg'));
  });

  testWidgets('a temporary password must be replaced before entering', (tester) async {
    final temporary = await giveMobileAccess();

    await openApp(tester);
    await signIn(tester, temporary.email, temporary.password);
    await waitFor(tester, find.text('Elige tu contraseña'));

    await tester.enterText(find.byType(TextField).at(0), temporary.password);
    await tester.enterText(find.byType(TextField).at(1), 'Perrito2026');
    await tester.enterText(find.byType(TextField).at(2), 'Perrito2026');
    // «Listo» en el teclado guarda la contraseña, como lo haría el dueño.
    await tester.testTextInput.receiveAction(TextInputAction.done);

    await waitFor(tester, find.text('Mis mascotas'));
  });

  testWidgets('the owner switches to English and signs out', (tester) async {
    await openApp(tester);
    await signIn(tester, _owner, _demoPassword);
    await waitFor(tester, find.text('Mis mascotas'));

    await tester.tap(find.text('Perfil'));
    await waitFor(tester, find.text('Cambiar contraseña'));
    await tester.tap(find.text('EN'));
    await waitFor(tester, find.text('Change password'));

    await tester.tap(find.text('Sign out'));
    await waitFor(tester, find.text('Sign in to your account'));
  });
}

/// Lets animations —a tab sliding in, the keyboard closing— finish.
Future<void> settle(WidgetTester tester) async {
  for (var i = 0; i < 10; i++) {
    await tester.pump(const Duration(milliseconds: 100));
  }
}

/// Pumps until the finder shows up: the screens wait for the API, and a
/// progress indicator never lets pumpAndSettle finish.
Future<void> waitFor(WidgetTester tester, Finder finder, {Duration timeout = const Duration(seconds: 30)}) async {
  final end = DateTime.now().add(timeout);
  while (DateTime.now().isBefore(end)) {
    await tester.pump(const Duration(milliseconds: 200));
    if (finder.evaluate().isNotEmpty) return;
  }
  throw TestFailure('No apareció: $finder');
}

/// The clinic gives mobile access to a new client, through the API.
Future<({String email, String password})> giveMobileAccess() async {
  Future<Map<String, dynamic>> post(String path, Object body, [String? token]) async {
    final response = await http.post(Uri.parse('$_api$path'),
        headers: {'Content-Type': 'application/json', if (token != null) 'Authorization': 'Bearer $token'},
        body: jsonEncode(body));
    return jsonDecode(utf8.decode(response.bodyBytes)) as Map<String, dynamic>;
  }

  final staff = await post('/authentication/sign-in',
      {'email': 'andrea.quispe@vetsanmiguel.pe', 'password': _demoPassword});
  final token = staff['accessToken'] as String;
  final dni = (70000000 + Random().nextInt(9999999)).toString();
  final email = 'movil.$dni@vetpass.test';

  final client = await post('/clients',
      {'fullName': 'Cliente $dni', 'documentType': 'Dni', 'documentNumber': dni, 'phoneNumber': '912345678'}, token);
  final account = await post('/authentication/owner-accounts',
      {'email': email, 'fullName': 'Cliente $dni', 'clientId': client['id']}, token);

  return (email: email, password: account['temporaryPassword'] as String);
}
