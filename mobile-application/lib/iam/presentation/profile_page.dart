import 'package:flutter/material.dart';
import '../../shared/i18n/app_strings.dart';
import '../../shared/presentation/theme.dart';
import '../application/session.dart';

/// Perfil y cierre de sesión. Recoge también el idioma, que es la decisión
/// que la sección 4.2.2 del informe exige ofrecer en ambas aplicaciones.
class ProfilePage extends StatelessWidget {
  const ProfilePage({super.key, required this.session, required this.idioma});

  final Session session;
  final IdiomaController idioma;

  @override
  Widget build(BuildContext context) {
    final textos = idioma.textos;
    final usuario = session.usuario;

    return Scaffold(
      appBar: AppBar(title: Text(textos.perfil)),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  CircleAvatar(
                    radius: 28,
                    backgroundColor: VetPassColors.primary,
                    child: Text(
                      _iniciales(usuario?.fullName ?? ''),
                      style: const TextStyle(
                          color: Colors.white, fontWeight: FontWeight.w600, fontSize: 18),
                    ),
                  ),
                  const SizedBox(width: 16),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(usuario?.fullName ?? '',
                            style: Theme.of(context).textTheme.titleLarge),
                        const SizedBox(height: 2),
                        Text(usuario?.email ?? '',
                            style: Theme.of(context)
                                .textTheme
                                .bodyMedium
                                ?.copyWith(color: VetPassColors.neutral600)),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
          if (usuario?.clinicName != null) ...[
            const SizedBox(height: 16),
            Card(
              child: ListTile(
                leading: const Icon(Icons.local_hospital_outlined,
                    color: VetPassColors.primary),
                title: Text(textos.veterinariaVinculada,
                    style: Theme.of(context).textTheme.labelSmall),
                subtitle: Text(usuario!.clinicName!,
                    style: Theme.of(context).textTheme.titleMedium),
              ),
            ),
          ],
          const SizedBox(height: 16),
          Card(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(textos.idiomaEtiqueta,
                      style: Theme.of(context).textTheme.titleMedium),
                  ListenableBuilder(
                    listenable: idioma,
                    builder: (context, _) => SegmentedButton<String>(
                      segments: const [
                        ButtonSegment(value: 'es', label: Text('ES')),
                        ButtonSegment(value: 'en', label: Text('EN')),
                      ],
                      selected: {idioma.value},
                      onSelectionChanged: (s) => idioma.value = s.first,
                      showSelectedIcon: false,
                      style: const ButtonStyle(visualDensity: VisualDensity.compact),
                    ),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 24),
          OutlinedButton.icon(
            onPressed: session.cerrarSesion,
            icon: const Icon(Icons.logout),
            label: Text(textos.cerrarSesion),
            style: OutlinedButton.styleFrom(
              minimumSize: const Size.fromHeight(48),
              foregroundColor: VetPassColors.dangerText,
              side: const BorderSide(color: VetPassColors.neutral200),
            ),
          ),
          const SizedBox(height: 24),
          Text(textos.soloConsulta,
              textAlign: TextAlign.center,
              style: Theme.of(context)
                  .textTheme
                  .labelSmall
                  ?.copyWith(color: VetPassColors.neutral600)),
        ],
      ),
    );
  }

  String _iniciales(String nombre) => nombre
      .split(' ')
      .where((p) => p.isNotEmpty)
      .take(2)
      .map((p) => p[0].toUpperCase())
      .join();
}
