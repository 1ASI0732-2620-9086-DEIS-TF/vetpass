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
    required this.visit,
    required this.texts,
    required this.clinic,
  });

  final Visit visit;
  final AppStrings texts;
  final String? clinic;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(texts.visit)),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Text(
            formatLongDate(visit.visitDate, isEnglish: texts.isEnglish),
            style: Theme.of(context).textTheme.headlineSmall,
          ),
          if (clinic != null) ...[
            const SizedBox(height: 4),
            Text(clinic!,
                style: Theme.of(context)
                    .textTheme
                    .bodyMedium
                    ?.copyWith(color: VetPassColors.neutral600)),
          ],
          const SizedBox(height: 20),
          _Block(title: texts.reason, text: visit.reason),
          if (visit.findings != null)
            _Block(title: texts.findings, text: visit.findings!),
          _Block(title: texts.diagnosis, text: visit.diagnosis),
          if (visit.treatment != null)
            _Block(title: texts.treatment, text: visit.treatment!),
          if (visit.weightKg != null)
            _Block(title: texts.weight, text: '${visit.weightKg} kg'),
          if (visit.prescription.isNotEmpty) ...[
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
                        Text(texts.prescription,
                            style: Theme.of(context)
                                .textTheme
                                .titleMedium
                                ?.copyWith(color: VetPassColors.primaryDark)),
                      ],
                    ),
                    const SizedBox(height: 12),
                    for (final item in visit.prescription) ...[
                      Text(item.medication,
                          style: Theme.of(context).textTheme.titleMedium),
                      const SizedBox(height: 2),
                      Text('${item.dosage} · ${item.duration}',
                          style: Theme.of(context)
                              .textTheme
                              .bodyMedium
                              ?.copyWith(color: VetPassColors.neutral600)),
                      if (item != visit.prescription.last) const SizedBox(height: 12),
                    ],
                  ],
                ),
              ),
            ),
          ],
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
}

class _Block extends StatelessWidget {
  const _Block({required this.title, required this.text});

  final String title;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(title.toUpperCase(),
              style: Theme.of(context).textTheme.labelSmall?.copyWith(
                    color: VetPassColors.neutral600,
                    letterSpacing: .6,
                    fontWeight: FontWeight.w600,
                  )),
          const SizedBox(height: 4),
          Text(text, style: Theme.of(context).textTheme.bodyLarge),
        ],
      ),
    );
  }
}
