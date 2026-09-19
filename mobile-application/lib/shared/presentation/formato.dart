/// Fecha en el formato del país. La cartilla en papel se lee así, y el
/// producto replica lo que el usuario ya conoce.
String formatearFecha(String? iso, {required bool esIngles}) {
  if (iso == null || iso.length < 10) return '—';
  final ano = iso.substring(0, 4);
  final mes = iso.substring(5, 7);
  final dia = iso.substring(8, 10);
  return esIngles ? '$mes/$dia/$ano' : '$dia/$mes/$ano';
}

/// Fecha larga para el encabezado del detalle de una atención.
String formatearFechaLarga(String? iso, {required bool esIngles}) {
  if (iso == null || iso.length < 10) return '—';
  final ano = int.parse(iso.substring(0, 4));
  final mes = int.parse(iso.substring(5, 7));
  final dia = int.parse(iso.substring(8, 10));

  const meses = {
    'es': ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio',
           'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'],
    'en': ['January', 'February', 'March', 'April', 'May', 'June', 'July',
           'August', 'September', 'October', 'November', 'December'],
  };

  return esIngles
      ? '${meses['en']![mes - 1]} $dia, $ano'
      : '$dia de ${meses['es']![mes - 1]} de $ano';
}
