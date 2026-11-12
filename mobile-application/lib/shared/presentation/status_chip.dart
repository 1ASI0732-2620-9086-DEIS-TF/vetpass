import 'package:flutter/material.dart';
import '../i18n/app_strings.dart';
import 'theme.dart';

/// Estado de una cartilla o de una dosis.
///
/// El color nunca viaja solo: cada estado se acompaña de su etiqueta textual y
/// de un icono distintivo, para que resulte legible con deficiencias en la
/// percepción cromática (sección 4.1.1 del informe).
class StatusChip extends StatelessWidget {
  const StatusChip({
    super.key,
    required this.status,
    required this.texts,
    this.ofCard = false,
    this.compact = false,
  });

  final String status;
  final AppStrings texts;
  final bool ofCard;
  final bool compact;

  @override
  Widget build(BuildContext context) {
    final (color, background, icon) = switch (status) {
      'UpToDate' || 'Applied' => (
          VetPassColors.successText, VetPassColors.successSurface, Icons.check_circle_outline),
      'Pending' => (
          VetPassColors.warningText, VetPassColors.warningSurface, Icons.schedule),
      'Overdue' => (
          VetPassColors.dangerText, VetPassColors.dangerSurface, Icons.error_outline),
      _ => (VetPassColors.neutral600, VetPassColors.surface, Icons.circle_outlined),
    };

    final label = ofCard ? texts.cardStatus(status) : texts.status(status);

    return Container(
      padding: EdgeInsets.symmetric(horizontal: compact ? 8 : 10, vertical: compact ? 2 : 4),
      decoration: BoxDecoration(
        color: background,
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: color),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: compact ? 13 : 15, color: color),
          const SizedBox(width: 6),
          Text(
            label,
            style: TextStyle(
              color: color,
              fontSize: compact ? 12 : 13,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }
}
