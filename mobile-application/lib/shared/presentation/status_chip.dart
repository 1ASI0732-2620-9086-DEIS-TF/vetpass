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
    required this.estado,
    required this.textos,
    this.deCartilla = false,
    this.compacto = false,
  });

  final String estado;
  final AppStrings textos;
  final bool deCartilla;
  final bool compacto;

  @override
  Widget build(BuildContext context) {
    final (color, fondo, icono) = switch (estado) {
      'UpToDate' || 'Applied' => (
          VetPassColors.successText, VetPassColors.successSurface, Icons.check_circle_outline),
      'Pending' => (
          VetPassColors.warningText, VetPassColors.warningSurface, Icons.schedule),
      'Overdue' => (
          VetPassColors.dangerText, VetPassColors.dangerSurface, Icons.error_outline),
      _ => (VetPassColors.neutral600, VetPassColors.surface, Icons.circle_outlined),
    };

    final etiqueta = deCartilla ? textos.estadoCartilla(estado) : textos.estado(estado);

    return Container(
      padding: EdgeInsets.symmetric(horizontal: compacto ? 8 : 10, vertical: compacto ? 2 : 4),
      decoration: BoxDecoration(
        color: fondo,
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: color),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icono, size: compacto ? 13 : 15, color: color),
          const SizedBox(width: 6),
          Text(
            etiqueta,
            style: TextStyle(
              color: color,
              fontSize: compacto ? 12 : 13,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }
}
