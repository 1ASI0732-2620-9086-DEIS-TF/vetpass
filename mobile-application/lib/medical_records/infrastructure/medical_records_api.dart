import '../../shared/infrastructure/api_client.dart';
import '../domain/visit.dart';

class MedicalRecordsApi {
  MedicalRecordsApi(this._api);

  final ApiClient _api;

  Future<List<Visit>> historialDe(String petId) async {
    final datos = await _api.get('/pets/$petId/visits') as List;
    return datos.map((v) => Visit.desdeJson(v as Map<String, dynamic>)).toList();
  }
}
