import 'package:flutter/material.dart';
import '../../iam/application/session.dart';
import '../../iam/presentation/profile_page.dart';
import '../../patients/presentation/my_pets_page.dart';
import '../i18n/app_strings.dart';

/// Navegación inferior con dos destinos, conforme a la sección 4.2.5:
/// Mis mascotas y Perfil.
class HomeShell extends StatefulWidget {
  const HomeShell({super.key, required this.session, required this.language});

  final Session session;
  final LanguageController language;

  @override
  State<HomeShell> createState() => _HomeShellState();
}

class _HomeShellState extends State<HomeShell> {
  int _destination = 0;

  @override
  Widget build(BuildContext context) {
    final texts = widget.language.texts;

    return Scaffold(
      body: IndexedStack(
        index: _destination,
        children: [
          MyPetsPage(session: widget.session, language: widget.language),
          ProfilePage(session: widget.session, language: widget.language),
        ],
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _destination,
        onDestinationSelected: (index) => setState(() => _destination = index),
        destinations: [
          NavigationDestination(
            icon: const Icon(Icons.pets_outlined),
            selectedIcon: const Icon(Icons.pets),
            label: texts.myPets,
          ),
          NavigationDestination(
            icon: const Icon(Icons.person_outline),
            selectedIcon: const Icon(Icons.person),
            label: texts.profile,
          ),
        ],
      ),
    );
  }
}
