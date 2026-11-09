import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../shared/i18n/app_strings.dart';
import '../../shared/presentation/theme.dart';
import '../application/session.dart';
import 'change_password_page.dart';

/// Perfil y cierre de sesión. Recoge también el idioma, que es la decisión
/// que la sección 4.2.2 del informe exige ofrecer en ambas aplicaciones.
final _terms = Uri.parse('https://vetpass-landing.vercel.app/terminos.html');

class ProfilePage extends StatelessWidget {
  const ProfilePage({super.key, required this.session, required this.language});

  final Session session;
  final LanguageController language;

  @override
  Widget build(BuildContext context) {
    final texts = language.texts;
    final user = session.user;

    return Scaffold(
      appBar: AppBar(title: Text(texts.profile)),
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
                      _initials(user?.fullName ?? ''),
                      style: const TextStyle(
                          color: Colors.white, fontWeight: FontWeight.w600, fontSize: 18),
                    ),
                  ),
                  const SizedBox(width: 16),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(user?.fullName ?? '',
                            style: Theme.of(context).textTheme.titleLarge),
                        const SizedBox(height: 2),
                        Text(user?.email ?? '',
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
          if (user?.clinicName != null) ...[
            const SizedBox(height: 16),
            Card(
              child: ListTile(
                leading: const Icon(Icons.local_hospital_outlined,
                    color: VetPassColors.primary),
                title: Text(texts.linkedClinic,
                    style: Theme.of(context).textTheme.labelSmall),
                subtitle: Text(user!.clinicName!,
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
                  Text(texts.languageLabel,
                      style: Theme.of(context).textTheme.titleMedium),
                  ListenableBuilder(
                    listenable: language,
                    builder: (context, _) => SegmentedButton<String>(
                      segments: const [
                        ButtonSegment(value: 'es', label: Text('ES')),
                        ButtonSegment(value: 'en', label: Text('EN')),
                      ],
                      selected: {language.value},
                      onSelectionChanged: (s) => language.value = s.first,
                      showSelectedIcon: false,
                      style: const ButtonStyle(visualDensity: VisualDensity.compact),
                    ),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),
          Card(
            child: ListTile(
              leading: const Icon(Icons.lock_outline, color: VetPassColors.primary),
              title: Text(texts.changePassword,
                  style: Theme.of(context).textTheme.titleMedium),
              trailing: const Icon(Icons.chevron_right),
              onTap: () => Navigator.of(context).push(MaterialPageRoute<void>(
                builder: (_) => ChangePasswordPage(session: session, language: language),
              )),
            ),
          ),
          const SizedBox(height: 8),
          Card(
            child: ListTile(
              leading: const Icon(Icons.description_outlined, color: VetPassColors.primary),
              title: Text(texts.terms, style: Theme.of(context).textTheme.titleMedium),
              trailing: const Icon(Icons.open_in_new, size: 20),
              // El acuerdo de servicio vive en la landing page; la app enlaza
              // a él, como exige el enunciado para todas las aplicaciones.
              onTap: () => launchUrl(_terms, mode: LaunchMode.externalApplication),
            ),
          ),
          const SizedBox(height: 24),
          OutlinedButton.icon(
            onPressed: session.signOut,
            icon: const Icon(Icons.logout),
            label: Text(texts.signOut),
            style: OutlinedButton.styleFrom(
              minimumSize: const Size.fromHeight(48),
              foregroundColor: VetPassColors.dangerText,
              side: const BorderSide(color: VetPassColors.neutral200),
            ),
          ),
          const SizedBox(height: 24),
          Text(texts.readOnly,
              textAlign: TextAlign.center,
              style: Theme.of(context)
                  .textTheme
                  .labelSmall
                  ?.copyWith(color: VetPassColors.neutral600)),
        ],
      ),
    );
  }

  String _initials(String name) => name
      .split(' ')
      .where((p) => p.isNotEmpty)
      .take(2)
      .map((p) => p[0].toUpperCase())
      .join();
}
