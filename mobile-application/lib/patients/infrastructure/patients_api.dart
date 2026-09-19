import '../../shared/infrastructure/api_client.dart';
import '../domain/pet.dart';

class PatientsApi {
  PatientsApi(this._api);

  final ApiClient _api;

  /// Las mascotas del cliente en sesión. La API resuelve de quién son a partir
  /// del token, de modo que esta pantalla no puede alcanzar las de otro
  /// cliente (US05-E2).
  Future<List<Pet>> misMascotas() async {
    final datos = await _api.get('/me/pets') as List;
    return datos.map((p) => Pet.desdeJson(p as Map<String, dynamic>)).toList();
  }
}
