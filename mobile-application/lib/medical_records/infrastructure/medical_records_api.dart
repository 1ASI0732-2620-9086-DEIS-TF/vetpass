import '../../shared/infrastructure/api_client.dart';
import '../domain/visit.dart';

class MedicalRecordsApi {
  MedicalRecordsApi(this._api);

  final ApiClient _api;

  Future<List<Visit>> historyOf(String petId) async {
    final details = await _api.get('/pets/$petId/visits') as List;
    return details.map((v) => Visit.fromJson(v as Map<String, dynamic>)).toList();
  }
}
