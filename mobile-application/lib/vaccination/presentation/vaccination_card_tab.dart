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
    required this.card,
    required this.texts,
    required this.clinic,
  });

  final VaccinationCard card;
  final AppStrings texts;
  final String? clinic;

  @override
  Widget build(BuildContext context) {
    final next = card.nextDose;

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Card(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                StatusChip(status: card.status, texts: texts, ofCard: true),
                const SizedBox(height: 12),
                Text(
                  next == null
                      ? texts.nothingPending
                      : '${texts.nextDose}: '
                          '${texts.doseLabel(next.vaccineName, next.sequenceNumber, next.isBooster)} · '
                          '${formatDate(next.expectedDate, isEnglish: texts.isEnglish)}',
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
              for (var i = 0; i < card.doses.length; i++) ...[
                if (i > 0) const Divider(height: 1),
                _DoseRow(dose: card.doses[i], texts: texts),
              ],
            ],
          ),
        ),
        const SizedBox(height: 16),
        if (clinic != null)
          Text(
            texts.issuedRecord(clinic!),
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

class _DoseRow extends StatelessWidget {
  const _DoseRow({required this.dose, required this.texts});

  final Dose dose;
  final AppStrings texts;

  @override
  Widget build(BuildContext context) {
    final detail = dose.applied
        ? '${formatDate(dose.applicationDate, isEnglish: texts.isEnglish)}'
            '${dose.batchCode != null ? ' · ${texts.batch} ${dose.batchCode}' : ''}'
        : '${texts.expectedDate}: '
            '${formatDate(dose.expectedDate, isEnglish: texts.isEnglish)}';

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  texts.doseLabel(
                      dose.vaccineName, dose.sequenceNumber, dose.isBooster),
                  style: Theme.of(context).textTheme.titleMedium,
                ),
                const SizedBox(height: 2),
                Text(detail,
                    style: Theme.of(context)
                        .textTheme
                        .bodyMedium
                        ?.copyWith(color: VetPassColors.neutral600)),
              ],
            ),
          ),
          const SizedBox(width: 12),
          StatusChip(
            status: dose.applied ? 'Applied' : (dose.isOverdue ? 'Overdue' : 'Pending'),
            texts: texts,
            compact: true,
          ),
        ],
      ),
    );
  }
}
