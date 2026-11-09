/// Fecha en el formato del país. La cartilla en papel se lee así, y el
/// producto replica lo que el usuario ya conoce.
String formatDate(String? iso, {required bool isEnglish}) {
  if (iso == null || iso.length < 10) return '—';
  final year = iso.substring(0, 4);
  final month = iso.substring(5, 7);
  final day = iso.substring(8, 10);
  return isEnglish ? '$month/$day/$year' : '$day/$month/$year';
}

/// Fecha larga para el encabezado del detalle de una atención.
String formatLongDate(String? iso, {required bool isEnglish}) {
  if (iso == null || iso.length < 10) return '—';
  final year = int.parse(iso.substring(0, 4));
  final month = int.parse(iso.substring(5, 7));
  final day = int.parse(iso.substring(8, 10));

  const months = {
    'es': ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio',
           'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'],
    'en': ['January', 'February', 'March', 'April', 'May', 'June', 'July',
           'August', 'September', 'October', 'November', 'December'],
  };

  return isEnglish
      ? '${months['en']![month - 1]} $day, $year'
      : '$day de ${months['es']![month - 1]} de $year';
}
