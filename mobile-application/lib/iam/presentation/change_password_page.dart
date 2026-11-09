import 'package:flutter/material.dart';
import '../../shared/i18n/app_strings.dart';
import '../../shared/infrastructure/api_client.dart';
import '../../shared/presentation/theme.dart';
import '../application/session.dart';

/// Cambio de contraseña del dueño (US17).
///
/// Se llega desde el Perfil o, en modo obligatorio, justo después de ingresar
/// con una contraseña temporal: la que dictó la recepción la conoce alguien
/// más, de modo que la aplicación no deja continuar hasta reemplazarla. En
/// ese modo no hay vuelta atrás, solo la salida de cerrar sesión.
///
/// La política de contraseñas vive en la API; aquí solo se comprueba que la
/// confirmación coincida, que es un error de escritura y no una regla.
class ChangePasswordPage extends StatefulWidget {
  const ChangePasswordPage({
    super.key,
    required this.session,
    required this.language,
    this.required = false,
  });

  final Session session;
  final LanguageController language;
  final bool required;

  @override
  State<ChangePasswordPage> createState() => _ChangePasswordPageState();
}

class _ChangePasswordPageState extends State<ChangePasswordPage> {
  final _current = TextEditingController();
  final _new = TextEditingController();
  final _confirmation = TextEditingController();

  bool _visible = false;
  bool _saving = false;
  String? _currentError;
  String? _newError;
  String? _confirmationError;
  String? _error;

  @override
  void dispose() {
    _current.dispose();
    _new.dispose();
    _confirmation.dispose();
    super.dispose();
  }

  bool get _full =>
      _current.text.isNotEmpty && _new.text.isNotEmpty && _confirmation.text.isNotEmpty;

  Future<void> _save(AppStrings texts) async {
    setState(() {
      _currentError = _newError = _confirmationError = _error = null;
    });

    if (_new.text != _confirmation.text) {
      setState(() => _confirmationError = texts.passwordsDoNotMatch);
      return;
    }

    setState(() => _saving = true);
    try {
      await widget.session.changePassword(_current.text, _new.text);
      if (!mounted) return;

      final messenger = ScaffoldMessenger.of(context);
      // En modo obligatorio la sesión cambia de estado y la aplicación pasa
      // sola a Mis mascotas; desde el Perfil se vuelve atrás.
      if (!widget.required) Navigator.of(context).pop();
      messenger.showSnackBar(SnackBar(content: Text(texts.passwordUpdated)));
    } on ApiException catch (failure) {
      if (failure.unauthorized) {
        await widget.session.signOut(expired: true);
        return;
      }
      setState(() {
        switch (failure.code) {
          case 'invalid-current-password':
            _currentError = texts.currentPasswordIncorrect;
          case 'weak-password':
            _newError = texts.weakPassword;
          case 'password-not-changed':
            _newError = texts.samePassword;
          default:
            _error = failure.noNetwork ? texts.offline : failure.toString();
        }
      });
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(
      listenable: widget.language,
      builder: (context, _) => _build(context, widget.language.texts),
    );
  }

  Widget _build(BuildContext context, AppStrings texts) {
    final theme = Theme.of(context).textTheme;

    return PopScope(
      canPop: !widget.required,
      child: Scaffold(
        appBar: AppBar(
          automaticallyImplyLeading: !widget.required,
          title: Text(widget.required
              ? texts.requiredPasswordTitle
              : texts.changePassword),
        ),
        body: SafeArea(
          child: ListView(
            padding: const EdgeInsets.all(16),
            children: [
              if (widget.required)
                Container(
                  padding: const EdgeInsets.all(12),
                  margin: const EdgeInsets.only(bottom: 16),
                  decoration: BoxDecoration(
                    color: VetPassColors.primary.withValues(alpha: 0.08),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Icon(Icons.lock_reset, color: VetPassColors.primary),
                      const SizedBox(width: 12),
                      Expanded(
                          child: Text(texts.requiredPasswordNote,
                              style: theme.bodyMedium)),
                    ],
                  ),
                )
              else
                Padding(
                  padding: const EdgeInsets.only(bottom: 16),
                  child: Text(texts.changePasswordNote,
                      style: theme.bodyMedium?.copyWith(color: VetPassColors.neutral600)),
                ),
              if (_error != null)
                Padding(
                  padding: const EdgeInsets.only(bottom: 16),
                  child: Text(_error!,
                      style: const TextStyle(color: VetPassColors.dangerText)),
                ),
              TextField(
                controller: _current,
                obscureText: !_visible,
                autofillHints: const [AutofillHints.password],
                onChanged: (_) => setState(() => _currentError = null),
                decoration: InputDecoration(
                  labelText: widget.required
                      ? texts.temporaryPassword
                      : texts.currentPassword,
                  errorText: _currentError,
                ),
              ),
              const SizedBox(height: 16),
              TextField(
                controller: _new,
                obscureText: !_visible,
                autofillHints: const [AutofillHints.newPassword],
                onChanged: (_) => setState(() => _newError = null),
                decoration: InputDecoration(
                  labelText: texts.newPassword,
                  helperText: texts.passwordPolicy,
                  errorText: _newError,
                  errorMaxLines: 2,
                ),
              ),
              const SizedBox(height: 16),
              TextField(
                controller: _confirmation,
                obscureText: !_visible,
                autofillHints: const [AutofillHints.newPassword],
                onChanged: (_) => setState(() => _confirmationError = null),
                onSubmitted: (_) => _full && !_saving ? _save(texts) : null,
                decoration: InputDecoration(
                  labelText: texts.confirmPassword,
                  errorText: _confirmationError,
                ),
              ),
              const SizedBox(height: 8),
              CheckboxListTile(
                value: _visible,
                onChanged: (v) => setState(() => _visible = v ?? false),
                title: Text(texts.show, style: theme.bodyMedium),
                controlAffinity: ListTileControlAffinity.leading,
                contentPadding: EdgeInsets.zero,
                dense: true,
              ),
              const SizedBox(height: 16),
              FilledButton(
                onPressed: _full && !_saving ? () => _save(texts) : null,
                child: _saving
                    ? const SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                    : Text(texts.savePassword),
              ),
              if (widget.required) ...[
                const SizedBox(height: 8),
                TextButton(
                  onPressed: widget.session.signOut,
                  child: Text(texts.signOut),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}
