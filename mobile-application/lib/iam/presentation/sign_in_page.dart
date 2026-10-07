import 'package:flutter/material.dart';
import '../../shared/i18n/app_strings.dart';
import '../../shared/infrastructure/api_client.dart';
import '../../shared/presentation/theme.dart';
import '../application/session.dart';

/// Inicio de sesión del dueño (US05). No hay registro: las credenciales las
/// entrega la clínica, de modo que la pantalla no ofrece crear una cuenta.
class SignInPage extends StatefulWidget {
  const SignInPage({super.key, required this.session, required this.idioma});

  final Session session;
  final IdiomaController idioma;

  @override
  State<SignInPage> createState() => _SignInPageState();
}

class _SignInPageState extends State<SignInPage> {
  final _correo = TextEditingController();
  final _contrasena = TextEditingController();
  String? _error;
  bool _oculta = true;

  @override
  void dispose() {
    _correo.dispose();
    _contrasena.dispose();
    super.dispose();
  }

  Future<void> _ingresar(AppStrings textos) async {
    setState(() => _error = null);
    try {
      await widget.session.iniciarSesion(_correo.text.trim(), _contrasena.text);
    } on ApiException catch (fallo) {
      setState(() => _error = fallo.sinRed
          ? textos.sinConexion
          : (fallo.noAutorizado ? textos.credencialesInvalidas : fallo.toString()));
    }
  }

  @override
  Widget build(BuildContext context) {
    final textos = widget.idioma.textos;

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
                    child: _SelectorIdioma(idioma: widget.idioma),
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
                      Text(textos.marca,
                          style: Theme.of(context).textTheme.headlineSmall),
                    ],
                  ),
                  const SizedBox(height: 40),
                  Text(textos.accesoTitulo,
                      style: Theme.of(context).textTheme.titleLarge),
                  const SizedBox(height: 8),
                  Text(textos.accesoNota,
                      style: Theme.of(context)
                          .textTheme
                          .bodyMedium
                          ?.copyWith(color: VetPassColors.neutral600)),
                  const SizedBox(height: 24),
                  if (_error == null && widget.session.expirada) ...[
                    Text(textos.sesionExpirada,
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
                    controller: _correo,
                    keyboardType: TextInputType.emailAddress,
                    autofillHints: const [AutofillHints.username],
                    decoration: InputDecoration(labelText: textos.correo),
                  ),
                  const SizedBox(height: 16),
                  TextField(
                    controller: _contrasena,
                    obscureText: _oculta,
                    autofillHints: const [AutofillHints.password],
                    onSubmitted: (_) => _ingresar(textos),
                    decoration: InputDecoration(
                      labelText: textos.contrasena,
                      suffixIcon: IconButton(
                        icon: Icon(_oculta ? Icons.visibility_off : Icons.visibility),
                        onPressed: () => setState(() => _oculta = !_oculta),
                        tooltip: textos.contrasena,
                      ),
                    ),
                  ),
                  const SizedBox(height: 24),
                  ListenableBuilder(
                    listenable: widget.session,
                    builder: (context, _) => FilledButton(
                      onPressed: widget.session.cargando ? null : () => _ingresar(textos),
                      child: widget.session.cargando
                          ? const SizedBox(
                              width: 20, height: 20,
                              child: CircularProgressIndicator(
                                  strokeWidth: 2, color: Colors.white))
                          : Text(textos.ingresar),
                    ),
                  ),
                  const SizedBox(height: 16),
                  Text(textos.accesoAyuda,
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

class _SelectorIdioma extends StatelessWidget {
  const _SelectorIdioma({required this.idioma});

  final IdiomaController idioma;

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(
      listenable: idioma,
      builder: (context, _) => SegmentedButton<String>(
        segments: const [
          ButtonSegment(value: 'es', label: Text('ES')),
          ButtonSegment(value: 'en', label: Text('EN')),
        ],
        selected: {idioma.value},
        onSelectionChanged: (seleccion) => idioma.value = seleccion.first,
        showSelectedIcon: false,
        style: const ButtonStyle(visualDensity: VisualDensity.compact),
      ),
    );
  }
}
