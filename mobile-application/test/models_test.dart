import 'package:flutter_test/flutter_test.dart';

import 'package:vetpass_mobile/patients/domain/pet.dart';
import 'package:vetpass_mobile/shared/presentation/formato.dart';
import 'package:vetpass_mobile/vaccination/domain/vaccination_card.dart';

/// The app reads the resources of the API as they are published: these tests
/// use the shape of real responses of `GET /me/pets` and the vaccination card.
void main() {
  test('Pet reads the resource of the API', () {
    final pet = Pet.desdeJson({
      'id': 'p1',
      'name': 'Kiara',
      'species': 'Canine',
      'breed': 'Shih tzu',
      'birthDate': '2025-10-22',
      'ageInWeeks': 50,
      'cardStatus': 'UpToDate',
    });

    expect(pet.name, 'Kiara');
    expect(pet.cardStatus, 'UpToDate');
  });

  test('VaccinationCard reads its doses and the next one', () {
    final card = VaccinationCard.desdeJson({
      'status': 'Pending',
      'nextDose': {
        'id': 'd2', 'vaccineName': 'Quíntuple', 'sequenceNumber': 2, 'isBooster': false,
        'expectedDate': '2026-10-07', 'applicationDate': null, 'batchCode': null,
        'status': 'Pending', 'isOverdue': false,
      },
      'doses': [
        {
          'id': 'd1', 'vaccineName': 'Quíntuple', 'sequenceNumber': 1, 'isBooster': false,
          'expectedDate': '2026-09-16', 'applicationDate': '2026-09-16', 'batchCode': 'A-4471',
          'status': 'Applied', 'isOverdue': false, 'isNextInSequence': false,
        },
      ],
    });

    expect(card.status, 'Pending');
    expect(card.nextDose!.sequenceNumber, 2);
    expect(card.doses.single.aplicada, isTrue);
    expect(card.doses.single.batchCode, 'A-4471');
  });

  test('dates follow the format of each language', () {
    expect(formatearFecha('2026-10-07', esIngles: false), '07/10/2026');
    expect(formatearFecha('2026-10-07', esIngles: true), '10/07/2026');
    expect(formatearFecha(null, esIngles: false), '—');
    expect(formatearFechaLarga('2026-09-23', esIngles: false), '23 de septiembre de 2026');
    expect(formatearFechaLarga('2026-09-23', esIngles: true), 'September 23, 2026');
  });
}
