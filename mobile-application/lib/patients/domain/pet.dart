/// Mascota del dueño, tal como la entrega la API.
class Pet {
  Pet({
    required this.id,
    required this.name,
    required this.species,
    required this.breed,
    required this.birthDate,
    required this.ageInWeeks,
    required this.cardStatus,
  });

  final String id;
  final String name;
  final String species;
  final String? breed;
  final String birthDate;
  final int ageInWeeks;

  /// Estado de la cartilla de esta mascota y no de otra: cada animal lo tiene
  /// por separado, que es la confusión que las entrevistas recogieron
  /// (US12-E2).
  final String? cardStatus;

  factory Pet.desdeJson(Map<String, dynamic> json) => Pet(
        id: json['id'] as String,
        name: json['name'] as String,
        species: json['species'] as String,
        breed: json['breed'] as String?,
        birthDate: json['birthDate'] as String,
        ageInWeeks: json['ageInWeeks'] as int,
        cardStatus: json['cardStatus'] as String?,
      );
}
