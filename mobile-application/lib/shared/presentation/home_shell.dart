import 'package:flutter/material.dart';
import '../../iam/application/session.dart';
import '../../iam/presentation/profile_page.dart';
import '../../patients/presentation/my_pets_page.dart';
import '../i18n/app_strings.dart';

/// Navegación inferior con dos destinos, conforme a la sección 4.2.5:
/// Mis mascotas y Perfil.
class HomeShell extends StatefulWidget {
  const HomeShell({super.key, required this.session, required this.idioma});

  final Session session;
  final IdiomaController idioma;

  @override
  State<HomeShell> createState() => _HomeShellState();
}

class _HomeShellState extends State<HomeShell> {
  int _destino = 0;

  @override
  Widget build(BuildContext context) {
    final textos = widget.idioma.textos;

    return Scaffold(
      body: IndexedStack(
        index: _destino,
        children: [
          MyPetsPage(session: widget.session, idioma: widget.idioma),
          ProfilePage(session: widget.session, idioma: widget.idioma),
        ],
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _destino,
        onDestinationSelected: (indice) => setState(() => _destino = indice),
        destinations: [
          NavigationDestination(
            icon: const Icon(Icons.pets_outlined),
            selectedIcon: const Icon(Icons.pets),
            label: textos.misMascotas,
          ),
          NavigationDestination(
            icon: const Icon(Icons.person_outline),
            selectedIcon: const Icon(Icons.person),
            label: textos.perfil,
          ),
        ],
      ),
    );
  }
}
