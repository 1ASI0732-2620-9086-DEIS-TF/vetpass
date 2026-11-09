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

  final preferences = await SharedPreferences.getInstance();
  final saved = preferences.getString('vetpass.idioma');
  final fromSystem = PlatformDispatcher.instance.locale.languageCode == 'en' ? 'en' : 'es';

  final language = LanguageController(saved ?? fromSystem);
  language.addListener(() => preferences.setString('vetpass.idioma', language.value));

  final session = Session(ApiClient());
  await session.restore();

  // Al terminar la sesión se descartan las pantallas abiertas encima —una
  // mascota, una atención—, para que el inicio de sesión quede a la vista.
  session.addListener(() {
    if (!session.authenticated) navigator.currentState?.popUntil((path) => path.isFirst);
  });

  runApp(VetPassApp(session: session, language: language));
}

final navigator = GlobalKey<NavigatorState>();

class VetPassApp extends StatelessWidget {
  const VetPassApp({super.key, required this.session, required this.language});

  final Session session;
  final LanguageController language;

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(
      listenable: Listenable.merge([session, language]),
      builder: (context, _) => MaterialApp(
        navigatorKey: navigator,
        title: 'VetPass',
        debugShowCheckedModeBanner: false,
        theme: buildVetPassTheme(),
        // Con una contraseña temporal, el dueño elige la suya antes de entrar.
        home: !session.authenticated
            ? SignInPage(session: session, language: language)
            : session.mustChangePassword
                ? ChangePasswordPage(session: session, language: language, required: true)
                : HomeShell(session: session, language: language),
      ),
    );
  }
}
