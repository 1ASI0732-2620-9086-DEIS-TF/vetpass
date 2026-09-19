import 'package:flutter/material.dart';
import '../../shared/i18n/app_strings.dart';
import '../../shared/presentation/formato.dart';
import '../../shared/presentation/status_chip.dart';
import '../../shared/presentation/theme.dart';
import '../domain/vaccination_card.dart';

/// Cartilla de vacunación de una mascota (US12-E1). Hereda la estructura
/// tabular del documento en papel: cada dosis es una fila con su fecha, su
/// lote y su estado.
class VaccinationCardTab extends StatelessWidget {
  const VaccinationCardTab({
    super.key,
    required this.cartilla,
    required this.textos,
    required this.clinica,
  });

  final VaccinationCard cartilla;
  final AppStrings textos;
  final String? clinica;

  @override
  Widget build(BuildContext context) {
    final proxima = cartilla.nextDose;

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Card(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                StatusChip(estado: cartilla.status, textos: textos, deCartilla: true),
                const SizedBox(height: 12),
                Text(
                  proxima == null
                      ? textos.sinPendientes
                      : '${textos.proximaDosis}: '
                          '${textos.etiquetaDosis(proxima.vaccineName, proxima.sequenceNumber, proxima.isBooster)} · '
                          '${formatearFecha(proxima.expectedDate, esIngles: textos.esIngles)}',
                  style: Theme.of(context)
                      .textTheme
                      .bodyMedium
                      ?.copyWith(color: VetPassColors.neutral600),
                ),
              ],
            ),
          ),
        ),
        const SizedBox(height: 16),
        Card(
          child: Column(
            children: [
              for (var i = 0; i < cartilla.doses.length; i++) ...[
                if (i > 0) const Divider(height: 1),
                _FilaDosis(dosis: cartilla.doses[i], textos: textos),
              ],
            ],
          ),
        ),
        const SizedBox(height: 16),
        if (clinica != null)
          Text(
            textos.registroEmitido(clinica!),
            textAlign: TextAlign.center,
            style: Theme.of(context)
                .textTheme
                .labelSmall
                ?.copyWith(color: VetPassColors.neutral600),
          ),
      ],
    );
  }
}

class _FilaDosis extends StatelessWidget {
  const _FilaDosis({required this.dosis, required this.textos});

  final Dose dosis;
  final AppStrings textos;

  @override
  Widget build(BuildContext context) {
    final detalle = dosis.aplicada
        ? '${formatearFecha(dosis.applicationDate, esIngles: textos.esIngles)}'
            '${dosis.batchCode != null ? ' · ${textos.lote} ${dosis.batchCode}' : ''}'
        : '${textos.fechaEsperada}: '
            '${formatearFecha(dosis.expectedDate, esIngles: textos.esIngles)}';

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  textos.etiquetaDosis(
                      dosis.vaccineName, dosis.sequenceNumber, dosis.isBooster),
                  style: Theme.of(context).textTheme.titleMedium,
                ),
                const SizedBox(height: 2),
                Text(detalle,
                    style: Theme.of(context)
                        .textTheme
                        .bodyMedium
                        ?.copyWith(color: VetPassColors.neutral600)),
              ],
            ),
          ),
          const SizedBox(width: 12),
          StatusChip(
            estado: dosis.aplicada ? 'Applied' : (dosis.isOverdue ? 'Overdue' : 'Pending'),
            textos: textos,
            compacto: true,
          ),
        ],
      ),
    );
  }
}
