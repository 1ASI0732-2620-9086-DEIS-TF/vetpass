import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

/// Paleta y tipografía de la sección 4.1 del informe, llevadas a Material
/// Design 3, que es el sistema de diseño nativo de Flutter y el que la
/// sección 4.1.3.2 adopta para Android.
abstract final class VetPassColors {
  static const primary = Color(0xFF0F766E);
  static const primaryDark = Color(0xFF115E59);
  static const primarySurface = Color(0xFFF0FDFA);
  static const secondary = Color(0xFFF59E0B);
  static const neutral900 = Color(0xFF0F172A);
  static const neutral600 = Color(0xFF475569);
  static const neutral200 = Color(0xFFE2E8F0);
  static const surface = Color(0xFFF8FAFC);

  /// Los tonos -600 del informe sirven de relleno; como texto sobre fondo
  /// claro se sustituyen por variantes más oscuras, para sostener la relación
  /// de contraste de 4.5:1 que declara la sección 4.1.1.
  static const successText = Color(0xFF15803D);
  static const successSurface = Color(0xFFECFDF5);
  static const warningText = Color(0xFFB45309);
  static const warningSurface = Color(0xFFFFFBEB);
  static const dangerText = Color(0xFFB91C1C);
  static const dangerSurface = Color(0xFFFEF2F2);
}

ThemeData buildVetPassTheme() {
  final esquema = ColorScheme.fromSeed(
    seedColor: VetPassColors.primary,
    primary: VetPassColors.primary,
    onPrimary: Colors.white,
    surface: Colors.white,
    onSurface: VetPassColors.neutral900,
  );

  final base = ThemeData(colorScheme: esquema, useMaterial3: true);

  // Poppins para titulares y elementos de marca, Inter para cuerpo e interfaz.
  final texto = GoogleFonts.interTextTheme(base.textTheme).copyWith(
    headlineMedium: GoogleFonts.poppins(
        fontSize: 28, height: 36 / 28, fontWeight: FontWeight.w600),
    headlineSmall: GoogleFonts.poppins(
        fontSize: 24, height: 32 / 24, fontWeight: FontWeight.w600),
    titleLarge: GoogleFonts.poppins(
        fontSize: 20, height: 28 / 20, fontWeight: FontWeight.w600),
    titleMedium: GoogleFonts.inter(
        fontSize: 16, height: 24 / 16, fontWeight: FontWeight.w600),
    bodyLarge: GoogleFonts.inter(fontSize: 16, height: 24 / 16),
    bodyMedium: GoogleFonts.inter(fontSize: 14, height: 20 / 14),
    labelSmall: GoogleFonts.inter(
        fontSize: 12, height: 16 / 12, fontWeight: FontWeight.w500),
  );

  return base.copyWith(
    scaffoldBackgroundColor: VetPassColors.surface,
    textTheme: texto,
    appBarTheme: AppBarTheme(
      backgroundColor: Colors.white,
      surfaceTintColor: Colors.transparent,
      foregroundColor: VetPassColors.neutral900,
      elevation: 0,
      scrolledUnderElevation: 1,
      titleTextStyle: texto.titleLarge,
    ),
    // Tarjetas con elevación de nivel 1 (sección 4.1.3.2).
    cardTheme: CardThemeData(
      color: Colors.white,
      elevation: 0,
      margin: EdgeInsets.zero,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
        side: const BorderSide(color: VetPassColors.neutral200),
      ),
    ),
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        // Área táctil mínima de 44 px, conforme a 4.1.3.
        minimumSize: const Size.fromHeight(48),
        textStyle: GoogleFonts.inter(fontSize: 16, fontWeight: FontWeight.w600),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: Colors.white,
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: const BorderSide(color: VetPassColors.neutral200),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: const BorderSide(color: VetPassColors.neutral200),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: const BorderSide(color: VetPassColors.primary, width: 2),
      ),
    ),
    navigationBarTheme: NavigationBarThemeData(
      backgroundColor: Colors.white,
      indicatorColor: VetPassColors.primarySurface,
      surfaceTintColor: Colors.transparent,
      labelTextStyle: WidgetStateProperty.all(
        GoogleFonts.inter(fontSize: 12, fontWeight: FontWeight.w500),
      ),
    ),
    dividerTheme: const DividerThemeData(color: VetPassColors.neutral200, space: 1),
  );
}
