/// Dosis de la cartilla.
class Dose {
  Dose({
    required this.id,
    required this.vaccineName,
    required this.sequenceNumber,
    required this.isBooster,
    required this.expectedDate,
    required this.applicationDate,
    required this.batchCode,
    required this.status,
    required this.isOverdue,
  });

  final String id;
  final String vaccineName;
  final int sequenceNumber;
  final bool isBooster;
  final String expectedDate;
  final String? applicationDate;
  final String? batchCode;
  final String status;
  final bool isOverdue;

  bool get aplicada => status == 'Applied';

  factory Dose.desdeJson(Map<String, dynamic> json) => Dose(
        id: json['id'] as String,
        vaccineName: json['vaccineName'] as String,
        sequenceNumber: json['sequenceNumber'] as int,
        isBooster: json['isBooster'] as bool? ?? false,
        expectedDate: json['expectedDate'] as String,
        applicationDate: json['applicationDate'] as String?,
        batchCode: json['batchCode'] as String?,
        status: json['status'] as String,
        isOverdue: json['isOverdue'] as bool? ?? false,
      );
}

/// Cartilla de vacunación de una mascota (US12-E1).
class VaccinationCard {
  VaccinationCard({
    required this.status,
    required this.nextDose,
    required this.doses,
  });

  final String status;
  final Dose? nextDose;
  final List<Dose> doses;

  factory VaccinationCard.desdeJson(Map<String, dynamic> json) => VaccinationCard(
        status: json['status'] as String,
        nextDose: json['nextDose'] == null
            ? null
            : Dose.desdeJson(json['nextDose'] as Map<String, dynamic>),
        doses: (json['doses'] as List)
            .map((d) => Dose.desdeJson(d as Map<String, dynamic>))
            .toList(),
      );
}
