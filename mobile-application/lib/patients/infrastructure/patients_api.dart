import '../../shared/infrastructure/api_client.dart';
import '../domain/pet.dart';

class PatientsApi {
  PatientsApi(this._api);

  final ApiClient _api;

  /// Las mascotas del cliente en sesión. La API resuelve de quién son a partir
  /// del token, de modo que esta pantalla no puede alcanzar las de otro
  /// cliente (US05-E2).
  Future<List<Pet>> myPets() async {
    final details = await _api.get('/me/pets') as List;
    return details.map((p) => Pet.fromJson(p as Map<String, dynamic>)).toList();
  }
}
