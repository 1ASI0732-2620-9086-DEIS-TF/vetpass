import 'package:flutter/material.dart';
import '../../shared/i18n/app_strings.dart';
import '../../shared/presentation/formato.dart';
import '../../shared/presentation/theme.dart';
import '../domain/visit.dart';

/// Detalle de una atención con su receta (US16-E2). Solo lectura: el dueño no
/// edita ningún campo clínico.
class VisitDetailPage extends StatelessWidget {
  const VisitDetailPage({
    super.key,
    required this.atencion,
    required this.textos,
    required this.clinica,
  });

  final Visit atencion;
  final AppStrings textos;
  final String? clinica;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(textos.atencion)),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Text(
            formatearFechaLarga(atencion.visitDate, esIngles: textos.esIngles),
            style: Theme.of(context).textTheme.headlineSmall,
          ),
          if (clinica != null) ...[
            const SizedBox(height: 4),
            Text(clinica!,
                style: Theme.of(context)
                    .textTheme
                    .bodyMedium
                    ?.copyWith(color: VetPassColors.neutral600)),
          ],
          const SizedBox(height: 20),
          _Bloque(titulo: textos.motivo, texto: atencion.reason),
          if (atencion.findings != null)
            _Bloque(titulo: textos.hallazgos, texto: atencion.findings!),
          _Bloque(titulo: textos.diagnostico, texto: atencion.diagnosis),
          if (atencion.treatment != null)
            _Bloque(titulo: textos.tratamiento, texto: atencion.treatment!),
          if (atencion.weightKg != null)
            _Bloque(titulo: textos.peso, texto: '${atencion.weightKg} kg'),
          if (atencion.prescription.isNotEmpty) ...[
            const SizedBox(height: 8),
            Card(
              color: VetPassColors.primarySurface,
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        const Icon(Icons.receipt_long,
                            size: 18, color: VetPassColors.primaryDark),
                        const SizedBox(width: 8),
                        Text(textos.receta,
                            style: Theme.of(context)
                                .textTheme
                                .titleMedium
                                ?.copyWith(color: VetPassColors.primaryDark)),
                      ],
                    ),
                    const SizedBox(height: 12),
                    for (final item in atencion.prescription) ...[
                      Text(item.medication,
                          style: Theme.of(context).textTheme.titleMedium),
                      const SizedBox(height: 2),
                      Text('${item.dosage} · ${item.duration}',
                          style: Theme.of(context)
                              .textTheme
                              .bodyMedium
                              ?.copyWith(color: VetPassColors.neutral600)),
                      if (item != atencion.prescription.last) const SizedBox(height: 12),
                    ],
                  ],
                ),
              ),
            ),
          ],
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
}

class _Bloque extends StatelessWidget {
  const _Bloque({required this.titulo, required this.texto});

  final String titulo;
  final String texto;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(titulo.toUpperCase(),
              style: Theme.of(context).textTheme.labelSmall?.copyWith(
                    color: VetPassColors.neutral600,
                    letterSpacing: .6,
                    fontWeight: FontWeight.w600,
                  )),
          const SizedBox(height: 4),
          Text(texto, style: Theme.of(context).textTheme.bodyLarge),
        ],
      ),
    );
  }
}
