/// Medicamento indicado en una receta.
class PrescriptionItem {
  PrescriptionItem({required this.medication, required this.dosage, required this.duration});

  final String medication;
  final String dosage;
  final String duration;

  factory PrescriptionItem.desdeJson(Map<String, dynamic> json) => PrescriptionItem(
        medication: json['medication'] as String,
        dosage: json['dosage'] as String,
        duration: json['duration'] as String,
      );
}

/// Atención veterinaria del historial (US16).
class Visit {
  Visit({
    required this.id,
    required this.visitDate,
    required this.reason,
    required this.findings,
    required this.diagnosis,
    required this.treatment,
    required this.weightKg,
    required this.hasPrescription,
    required this.prescription,
  });

  final String id;
  final String visitDate;
  final String reason;
  final String? findings;
  final String diagnosis;
  final String? treatment;
  final num? weightKg;
  final bool hasPrescription;
  final List<PrescriptionItem> prescription;

  factory Visit.desdeJson(Map<String, dynamic> json) {
    final receta = json['prescription'] as Map<String, dynamic>?;
    return Visit(
      id: json['id'] as String,
      visitDate: json['visitDate'] as String,
      reason: json['reason'] as String,
      findings: json['findings'] as String?,
      diagnosis: json['diagnosis'] as String,
      treatment: json['treatment'] as String?,
      weightKg: json['weightKg'] as num?,
      hasPrescription: json['hasPrescription'] as bool? ?? false,
      prescription: receta == null
          ? const []
          : (receta['items'] as List)
              .map((i) => PrescriptionItem.desdeJson(i as Map<String, dynamic>))
              .toList(),
    );
  }
}
