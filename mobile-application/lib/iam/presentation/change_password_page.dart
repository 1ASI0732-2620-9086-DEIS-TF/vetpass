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
    required this.idioma,
    this.obligatorio = false,
  });

  final Session session;
  final IdiomaController idioma;
  final bool obligatorio;

  @override
  State<ChangePasswordPage> createState() => _ChangePasswordPageState();
}

class _ChangePasswordPageState extends State<ChangePasswordPage> {
  final _actual = TextEditingController();
  final _nueva = TextEditingController();
  final _confirmacion = TextEditingController();

  bool _visible = false;
  bool _guardando = false;
  String? _errorActual;
  String? _errorNueva;
  String? _errorConfirmacion;
  String? _error;

  @override
  void dispose() {
    _actual.dispose();
    _nueva.dispose();
    _confirmacion.dispose();
    super.dispose();
  }

  bool get _completo =>
      _actual.text.isNotEmpty && _nueva.text.isNotEmpty && _confirmacion.text.isNotEmpty;

  Future<void> _guardar(AppStrings textos) async {
    setState(() {
      _errorActual = _errorNueva = _errorConfirmacion = _error = null;
    });

    if (_nueva.text != _confirmacion.text) {
      setState(() => _errorConfirmacion = textos.contrasenasNoCoinciden);
      return;
    }

    setState(() => _guardando = true);
    try {
      await widget.session.cambiarContrasena(_actual.text, _nueva.text);
      if (!mounted) return;

      final mensajero = ScaffoldMessenger.of(context);
      // En modo obligatorio la sesión cambia de estado y la aplicación pasa
      // sola a Mis mascotas; desde el Perfil se vuelve atrás.
      if (!widget.obligatorio) Navigator.of(context).pop();
      mensajero.showSnackBar(SnackBar(content: Text(textos.contrasenaActualizada)));
    } on ApiException catch (fallo) {
      if (fallo.noAutorizado) {
        await widget.session.cerrarSesion();
        return;
      }
      setState(() {
        switch (fallo.code) {
          case 'invalid-current-password':
            _errorActual = textos.contrasenaActualIncorrecta;
          case 'weak-password':
            _errorNueva = textos.contrasenaDebil;
          case 'password-not-changed':
            _errorNueva = textos.contrasenaIgual;
          default:
            _error = fallo.sinRed ? textos.sinConexion : fallo.toString();
        }
      });
    } finally {
      if (mounted) setState(() => _guardando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(
      listenable: widget.idioma,
      builder: (context, _) => _construir(context, widget.idioma.textos),
    );
  }

  Widget _construir(BuildContext context, AppStrings textos) {
    final tema = Theme.of(context).textTheme;

    return PopScope(
      canPop: !widget.obligatorio,
      child: Scaffold(
        appBar: AppBar(
          automaticallyImplyLeading: !widget.obligatorio,
          title: Text(widget.obligatorio
              ? textos.contrasenaObligatoriaTitulo
              : textos.cambiarContrasena),
        ),
        body: SafeArea(
          child: ListView(
            padding: const EdgeInsets.all(16),
            children: [
              if (widget.obligatorio)
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
                          child: Text(textos.contrasenaObligatoriaNota,
                              style: tema.bodyMedium)),
                    ],
                  ),
                )
              else
                Padding(
                  padding: const EdgeInsets.only(bottom: 16),
                  child: Text(textos.cambiarContrasenaNota,
                      style: tema.bodyMedium?.copyWith(color: VetPassColors.neutral600)),
                ),
              if (_error != null)
                Padding(
                  padding: const EdgeInsets.only(bottom: 16),
                  child: Text(_error!,
                      style: const TextStyle(color: VetPassColors.dangerText)),
                ),
              TextField(
                controller: _actual,
                obscureText: !_visible,
                autofillHints: const [AutofillHints.password],
                onChanged: (_) => setState(() => _errorActual = null),
                decoration: InputDecoration(
                  labelText: widget.obligatorio
                      ? textos.contrasenaTemporal
                      : textos.contrasenaActual,
                  errorText: _errorActual,
                ),
              ),
              const SizedBox(height: 16),
              TextField(
                controller: _nueva,
                obscureText: !_visible,
                autofillHints: const [AutofillHints.newPassword],
                onChanged: (_) => setState(() => _errorNueva = null),
                decoration: InputDecoration(
                  labelText: textos.contrasenaNueva,
                  helperText: textos.contrasenaPolitica,
                  errorText: _errorNueva,
                  errorMaxLines: 2,
                ),
              ),
              const SizedBox(height: 16),
              TextField(
                controller: _confirmacion,
                obscureText: !_visible,
                autofillHints: const [AutofillHints.newPassword],
                onChanged: (_) => setState(() => _errorConfirmacion = null),
                onSubmitted: (_) => _completo && !_guardando ? _guardar(textos) : null,
                decoration: InputDecoration(
                  labelText: textos.contrasenaConfirmar,
                  errorText: _errorConfirmacion,
                ),
              ),
              const SizedBox(height: 8),
              CheckboxListTile(
                value: _visible,
                onChanged: (v) => setState(() => _visible = v ?? false),
                title: Text(textos.mostrar, style: tema.bodyMedium),
                controlAffinity: ListTileControlAffinity.leading,
                contentPadding: EdgeInsets.zero,
                dense: true,
              ),
              const SizedBox(height: 16),
              FilledButton(
                onPressed: _completo && !_guardando ? () => _guardar(textos) : null,
                child: _guardando
                    ? const SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                    : Text(textos.guardarContrasena),
              ),
              if (widget.obligatorio) ...[
                const SizedBox(height: 8),
                TextButton(
                  onPressed: widget.session.cerrarSesion,
                  child: Text(textos.cerrarSesion),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}
