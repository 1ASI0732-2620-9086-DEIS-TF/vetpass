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
    required this.visits,
    required this.texts,
    required this.clinic,
    required this.petName,
  });

  final List<Visit> visits;
  final AppStrings texts;
  final String? clinic;
  final String petName;

  @override
  Widget build(BuildContext context) {
    if (visits.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(32),
          child: Text(texts.noVisits,
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
              for (var i = 0; i < visits.length; i++) ...[
                if (i > 0) const Divider(height: 1),
                _VisitRow(
                  visit: visits[i],
                  texts: texts,
                  onTap: () => Navigator.of(context).push(MaterialPageRoute(
                    builder: (_) => VisitDetailPage(
                      visit: visits[i],
                      texts: texts,
                      clinic: clinic,
                    ),
                  )),
                ),
              ],
            ],
          ),
        ),
        const SizedBox(height: 16),
        Text(texts.historyNote,
            textAlign: TextAlign.center,
            style: Theme.of(context)
                .textTheme
                .labelSmall
                ?.copyWith(color: VetPassColors.neutral600)),
      ],
    );
  }
}

class _VisitRow extends StatelessWidget {
  const _VisitRow({required this.visit, required this.texts, required this.onTap});

  final Visit visit;
  final AppStrings texts;
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
                        formatDate(visit.visitDate, isEnglish: texts.isEnglish),
                        style: Theme.of(context)
                            .textTheme
                            .bodyMedium
                            ?.copyWith(color: VetPassColors.neutral600),
                      ),
                      if (visit.hasPrescription) ...[
                        const SizedBox(width: 8),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                          decoration: BoxDecoration(
                            color: VetPassColors.primarySurface,
                            borderRadius: BorderRadius.circular(999),
                          ),
                          child: Text(
                            texts.withPrescription,
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
                  Text(visit.reason, style: Theme.of(context).textTheme.titleMedium),
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
