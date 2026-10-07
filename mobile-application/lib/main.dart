import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'iam/application/session.dart';
import 'iam/presentation/change_password_page.dart';
import 'iam/presentation/sign_in_page.dart';
import 'shared/i18n/app_strings.dart';
import 'shared/infrastructure/api_client.dart';
import 'shared/presentation/home_shell.dart';
import 'shared/presentation/theme.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  final preferencias = await SharedPreferences.getInstance();
  final guardado = preferencias.getString('vetpass.idioma');
  final delSistema = PlatformDispatcher.instance.locale.languageCode == 'en' ? 'en' : 'es';

  final idioma = IdiomaController(guardado ?? delSistema);
  idioma.addListener(() => preferencias.setString('vetpass.idioma', idioma.value));

  final session = Session(ApiClient());
  await session.restaurar();

  runApp(VetPassApp(session: session, idioma: idioma));
}

class VetPassApp extends StatelessWidget {
  const VetPassApp({super.key, required this.session, required this.idioma});

  final Session session;
  final IdiomaController idioma;

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(
      listenable: Listenable.merge([session, idioma]),
      builder: (context, _) => MaterialApp(
        title: 'VetPass',
        debugShowCheckedModeBanner: false,
        theme: buildVetPassTheme(),
        // Con una contraseña temporal, el dueño elige la suya antes de entrar.
        home: !session.autenticado
            ? SignInPage(session: session, idioma: idioma)
            : session.debeCambiarContrasena
                ? ChangePasswordPage(session: session, idioma: idioma, obligatorio: true)
                : HomeShell(session: session, idioma: idioma),
      ),
    );
  }
}
