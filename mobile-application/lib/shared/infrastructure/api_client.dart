import 'dart:convert';
import 'package:http/http.dart' as http;

/// Error devuelto por la API en formato ProblemDetails.
class ApiException implements Exception {
  ApiException(this.status, {this.code, this.detail});

  final int status;
  final String? code;
  final String? detail;

  bool get noAutorizado => status == 401;
  bool get prohibido => status == 403;
  bool get sinRed => status == 0;

  @override
  String toString() => detail ?? 'HTTP $status';
}

/// Cliente de la RESTful API. La aplicación móvil no habla con ningún otro
/// servicio: ni siquiera con el proveedor de identidad, que queda detrás de
/// la API.
class ApiClient {
  ApiClient({String? baseUrl, http.Client? cliente})
      : baseUrl = baseUrl ??
            const String.fromEnvironment('VETPASS_API',
                defaultValue: 'http://localhost:5199/api/v1'),
        _http = cliente ?? http.Client();

  final String baseUrl;
  final http.Client _http;
  String? _token;

  /// Se invoca cuando la API rechaza el token de la sesión (401). Devuelve
  /// true si consiguió renovarla; entonces la petición se repite una vez.
  Future<bool> Function()? renovarSesion;

  set token(String? valor) => _token = valor;

  Map<String, String> _cabeceras({bool conToken = true}) => {
        'Content-Type': 'application/json',
        if (conToken && _token != null) 'Authorization': 'Bearer $_token',
      };

  Future<dynamic> get(String ruta) => _enviar(() =>
      _http.get(Uri.parse('$baseUrl$ruta'), headers: _cabeceras()));

  /// [sinSesion] es para las operaciones que no dependen de la sesión
  /// —iniciarla o renovarla—: no envían el token ni intentan renovarlo.
  Future<dynamic> post(String ruta, Map<String, dynamic> cuerpo, {bool sinSesion = false}) =>
      _enviar(
        () => _http.post(Uri.parse('$baseUrl$ruta'),
            headers: _cabeceras(conToken: !sinSesion), body: jsonEncode(cuerpo)),
        renovable: !sinSesion,
      );

  Future<dynamic> _enviar(Future<http.Response> Function() peticion,
      {bool renovable = true}) async {
    var respuesta = await _ejecutar(peticion);

    // El token de acceso dura una hora. Vencido, se renueva con el de
    // refresco y se repite la petición, sin que el dueño lo note.
    if (respuesta.statusCode == 401 && renovable && _token != null && renovarSesion != null) {
      if (await renovarSesion!()) respuesta = await _ejecutar(peticion);
    }

    if (respuesta.statusCode >= 200 && respuesta.statusCode < 300) {
      if (respuesta.body.isEmpty) return null;
      return jsonDecode(utf8.decode(respuesta.bodyBytes));
    }

    String? codigo;
    String? detalle;
    try {
      final problema = jsonDecode(utf8.decode(respuesta.bodyBytes));
      codigo = problema['code'] as String?;
      detalle = problema['detail'] as String?;
    } catch (_) {
      // Una respuesta sin cuerpo legible deja solo el código de estado.
    }

    throw ApiException(respuesta.statusCode, code: codigo, detail: detalle);
  }

  Future<http.Response> _ejecutar(Future<http.Response> Function() peticion) async {
    try {
      return await peticion();
    } catch (_) {
      throw ApiException(0);
    }
  }
}
