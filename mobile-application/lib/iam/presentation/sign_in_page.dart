import 'package:flutter/material.dart';
import '../../shared/i18n/app_strings.dart';
import '../../shared/infrastructure/api_client.dart';
import '../../shared/presentation/theme.dart';
import '../application/session.dart';

/// Inicio de sesión del dueño (US05). No hay registro: las credenciales las
/// entrega la clínica, de modo que la pantalla no ofrece crear una cuenta.
class SignInPage extends StatefulWidget {
  const SignInPage({super.key, required this.session, required this.language});

  final Session session;
  final LanguageController language;

  @override
  State<SignInPage> createState() => _SignInPageState();
}

class _SignInPageState extends State<SignInPage> {
  final _email = TextEditingController();
  final _password = TextEditingController();
  String? _error;
  bool _hidden = true;

  @override
  void dispose() {
    _email.dispose();
    _password.dispose();
    super.dispose();
  }

  Future<void> _signIn(AppStrings texts) async {
    setState(() => _error = null);
    try {
      await widget.session.signIn(_email.text.trim(), _password.text);
    } on ApiException catch (failure) {
      setState(() => _error = failure.noNetwork
          ? texts.offline
          : (failure.unauthorized ? texts.invalidCredentials : failure.toString()));
    }
  }

  @override
  Widget build(BuildContext context) {
    final texts = widget.language.texts;

    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 420),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Align(
                    alignment: Alignment.centerRight,
                    child: _LanguageSelector(language: widget.language),
                  ),
                  const SizedBox(height: 24),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Container(
                        width: 44,
                        height: 44,
                        decoration: BoxDecoration(
                          color: VetPassColors.primary,
                          borderRadius: BorderRadius.circular(12),
                        ),
                        child: const Icon(Icons.pets, color: Colors.white, size: 24),
                      ),
                      const SizedBox(width: 12),
                      Text(texts.brand,
                          style: Theme.of(context).textTheme.headlineSmall),
                    ],
                  ),
                  const SizedBox(height: 40),
                  Text(texts.signInTitle,
                      style: Theme.of(context).textTheme.titleLarge),
                  const SizedBox(height: 8),
                  Text(texts.signInNote,
                      style: Theme.of(context)
                          .textTheme
                          .bodyMedium
                          ?.copyWith(color: VetPassColors.neutral600)),
                  const SizedBox(height: 24),
                  if (_error == null && widget.session.expired) ...[
                    Text(texts.sessionExpired,
                        style: Theme.of(context)
                            .textTheme
                            .bodyMedium
                            ?.copyWith(color: VetPassColors.primary)),
                    const SizedBox(height: 16),
                  ],
                  if (_error != null) ...[
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: VetPassColors.dangerSurface,
                        borderRadius: BorderRadius.circular(8),
                        border: Border.all(color: VetPassColors.dangerText),
                      ),
                      child: Row(
                        children: [
                          const Icon(Icons.error_outline,
                              size: 18, color: VetPassColors.dangerText),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(_error!,
                                style: const TextStyle(
                                    color: VetPassColors.dangerText, fontSize: 14)),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 16),
                  ],
                  TextField(
                    controller: _email,
                    keyboardType: TextInputType.emailAddress,
                    autofillHints: const [AutofillHints.username],
                    decoration: InputDecoration(labelText: texts.email),
                  ),
                  const SizedBox(height: 16),
                  TextField(
                    controller: _password,
                    obscureText: _hidden,
                    autofillHints: const [AutofillHints.password],
                    onSubmitted: (_) => _signIn(texts),
                    decoration: InputDecoration(
                      labelText: texts.password,
                      suffixIcon: IconButton(
                        icon: Icon(_hidden ? Icons.visibility_off : Icons.visibility),
                        onPressed: () => setState(() => _hidden = !_hidden),
                        tooltip: texts.password,
                      ),
                    ),
                  ),
                  const SizedBox(height: 24),
                  ListenableBuilder(
                    listenable: widget.session,
                    builder: (context, _) => FilledButton(
                      onPressed: widget.session.loading ? null : () => _signIn(texts),
                      child: widget.session.loading
                          ? const SizedBox(
                              width: 20, height: 20,
                              child: CircularProgressIndicator(
                                  strokeWidth: 2, color: Colors.white))
                          : Text(texts.signIn),
                    ),
                  ),
                  const SizedBox(height: 16),
                  Text(texts.signInHelp,
                      textAlign: TextAlign.center,
                      style: Theme.of(context)
                          .textTheme
                          .labelSmall
                          ?.copyWith(color: VetPassColors.neutral600)),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _LanguageSelector extends StatelessWidget {
  const _LanguageSelector({required this.language});

  final LanguageController language;

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(
      listenable: language,
      builder: (context, _) => SegmentedButton<String>(
        segments: const [
          ButtonSegment(value: 'es', label: Text('ES')),
          ButtonSegment(value: 'en', label: Text('EN')),
        ],
        selected: {language.value},
        onSelectionChanged: (selection) => language.value = selection.first,
        showSelectedIcon: false,
        style: const ButtonStyle(visualDensity: VisualDensity.compact),
      ),
    );
  }
}
