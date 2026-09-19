import 'package:flutter/material.dart';

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

  // Poppins para titulares y elementos de marca, Inter para cuerpo e interfaz,
  // con la escala tipográfica de la sección 4.1.1.
  final texto = base.textTheme.copyWith(
    headlineMedium: _poppins(28, 36, FontWeight.w600),
    headlineSmall: _poppins(24, 32, FontWeight.w600),
    titleLarge: _poppins(20, 28, FontWeight.w600),
    titleMedium: _inter(16, 24, FontWeight.w600),
    bodyLarge: _inter(16, 24, FontWeight.w400),
    bodyMedium: _inter(14, 20, FontWeight.w400),
    labelLarge: _inter(15, 20, FontWeight.w600),
    labelMedium: _inter(14, 20, FontWeight.w500),
    labelSmall: _inter(12, 16, FontWeight.w500),
  ).apply(bodyColor: VetPassColors.neutral900, displayColor: VetPassColors.neutral900);

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
        textStyle: _inter(16, 24, FontWeight.w600),
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
      labelTextStyle: WidgetStateProperty.all(_inter(12, 16, FontWeight.w500)),
    ),
    dividerTheme: const DividerThemeData(color: VetPassColors.neutral200, space: 1),
  );
}

TextStyle _poppins(double tamano, double alto, FontWeight peso) => TextStyle(
      fontFamily: 'Poppins',
      fontSize: tamano,
      height: alto / tamano,
      fontWeight: peso,
    );

/// Inter se empaqueta como fuente variable: el peso se pide por el eje `wght`,
/// además de por `fontWeight`, que es lo que la fuente interpola.
TextStyle _inter(double tamano, double alto, FontWeight peso) => TextStyle(
      fontFamily: 'Inter',
      fontSize: tamano,
      height: alto / tamano,
      fontWeight: peso,
      fontVariations: [FontVariation('wght', peso.value.toDouble())],
    );
