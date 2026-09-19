import 'package:flutter/material.dart';
import '../../shared/i18n/app_strings.dart';
import '../../shared/presentation/formato.dart';
import '../../shared/presentation/theme.dart';
import '../domain/visit.dart';
import 'visit_detail_page.dart';

/// Historial de atenciones de la mascota (US16-E1), en orden cronológico
/// descendente: la atención más reciente encabeza el listado.
class RecordTab extends StatelessWidget {
  const RecordTab({
    super.key,
    required this.atenciones,
    required this.textos,
    required this.clinica,
    required this.nombreMascota,
  });

  final List<Visit> atenciones;
  final AppStrings textos;
  final String? clinica;
  final String nombreMascota;

  @override
  Widget build(BuildContext context) {
    if (atenciones.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(32),
          child: Text(textos.sinAtenciones,
              textAlign: TextAlign.center,
              style: Theme.of(context)
                  .textTheme
                  .bodyLarge
                  ?.copyWith(color: VetPassColors.neutral600)),
        ),
      );
    }

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Card(
          child: Column(
            children: [
              for (var i = 0; i < atenciones.length; i++) ...[
                if (i > 0) const Divider(height: 1),
                _FilaAtencion(
                  atencion: atenciones[i],
                  textos: textos,
                  onTap: () => Navigator.of(context).push(MaterialPageRoute(
                    builder: (_) => VisitDetailPage(
                      atencion: atenciones[i],
                      textos: textos,
                      clinica: clinica,
                    ),
                  )),
                ),
              ],
            ],
          ),
        ),
        const SizedBox(height: 16),
        Text(textos.notaHistorial,
            textAlign: TextAlign.center,
            style: Theme.of(context)
                .textTheme
                .labelSmall
                ?.copyWith(color: VetPassColors.neutral600)),
      ],
    );
  }
}

class _FilaAtencion extends StatelessWidget {
  const _FilaAtencion({required this.atencion, required this.textos, required this.onTap});

  final Visit atencion;
  final AppStrings textos;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        child: Row(
          children: [
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Text(
                        formatearFecha(atencion.visitDate, esIngles: textos.esIngles),
                        style: Theme.of(context)
                            .textTheme
                            .bodyMedium
                            ?.copyWith(color: VetPassColors.neutral600),
                      ),
                      if (atencion.hasPrescription) ...[
                        const SizedBox(width: 8),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                          decoration: BoxDecoration(
                            color: VetPassColors.primarySurface,
                            borderRadius: BorderRadius.circular(999),
                          ),
                          child: Text(
                            textos.conReceta,
                            style: const TextStyle(
                              fontSize: 11,
                              fontWeight: FontWeight.w600,
                              color: VetPassColors.primaryDark,
                            ),
                          ),
                        ),
                      ],
                    ],
                  ),
                  const SizedBox(height: 4),
                  Text(atencion.reason, style: Theme.of(context).textTheme.titleMedium),
                ],
              ),
            ),
            const Icon(Icons.chevron_right, color: VetPassColors.neutral600),
          ],
        ),
      ),
    );
  }
}
