import '../../shared/infrastructure/api_client.dart';
import '../domain/vaccination_card.dart';

class VaccinationApi {
  VaccinationApi(this._api);

  final ApiClient _api;

  Future<VaccinationCard> cartillaDe(String petId) async {
    final datos = await _api.get('/pets/$petId/vaccination-card') as Map<String, dynamic>;
    return VaccinationCard.desdeJson(datos);
  }
}
