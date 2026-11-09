import 'dart:convert';
import 'package:http/http.dart' as http;

/// Error devuelto por la API en formato ProblemDetails.
class ApiException implements Exception {
  ApiException(this.status, {this.code, this.detail});

  final int status;
  final String? code;
  final String? detail;

  bool get unauthorized => status == 401;
  bool get forbidden => status == 403;
  bool get noNetwork => status == 0;

  @override
  String toString() => detail ?? 'HTTP $status';
}

/// Cliente de la RESTful API. La aplicación móvil no habla con ningún otro
/// servicio: ni siquiera con el proveedor de identidad, que queda detrás de
/// la API.
class ApiClient {
  ApiClient({String? baseUrl, http.Client? client})
      : baseUrl = baseUrl ??
            const String.fromEnvironment('VETPASS_API',
                defaultValue: 'http://localhost:5199/api/v1'),
        _http = client ?? http.Client();

  final String baseUrl;
  final http.Client _http;
  String? _token;

  /// Se invoca cuando la API rechaza el token de la sesión (401). Devuelve
  /// true si consiguió renovarla; entonces la petición se repite una vez.
  Future<bool> Function()? renewSession;

  set token(String? value) => _token = value;

  Map<String, String> _cabeceras({bool conToken = true}) => {
        'Content-Type': 'application/json',
        if (conToken && _token != null) 'Authorization': 'Bearer $_token',
      };

  Future<dynamic> get(String path) => _enviar(() =>
      _http.get(Uri.parse('$baseUrl$path'), headers: _cabeceras()));

  /// [sinSesion] es para las operaciones que no dependen de la sesión
  /// —iniciarla o renovarla—: no envían el token ni intentan renovarlo.
  Future<dynamic> post(String path, Map<String, dynamic> body, {bool signedOut = false}) =>
      _enviar(
        () => _http.post(Uri.parse('$baseUrl$path'),
            headers: _cabeceras(conToken: !signedOut), body: jsonEncode(body)),
        renovable: !signedOut,
      );

  Future<dynamic> _enviar(Future<http.Response> Function() request,
      {bool renovable = true}) async {
    var response = await _run(request);

    // El token de acceso dura una hora. Vencido, se renueva con el de
    // refresco y se repite la petición, sin que el dueño lo note.
    if (response.statusCode == 401 && renovable && _token != null && renewSession != null) {
      if (await renewSession!()) response = await _run(request);
    }

    if (response.statusCode >= 200 && response.statusCode < 300) {
      if (response.body.isEmpty) return null;
      return jsonDecode(utf8.decode(response.bodyBytes));
    }

    String? code;
    String? detail;
    try {
      final problem = jsonDecode(utf8.decode(response.bodyBytes));
      code = problem['code'] as String?;
      detail = problem['detail'] as String?;
    } catch (_) {
      // Una respuesta sin cuerpo legible deja solo el código de estado.
    }

    throw ApiException(response.statusCode, code: code, detail: detail);
  }

  Future<http.Response> _run(Future<http.Response> Function() request) async {
    try {
      return await request();
    } catch (_) {
      throw ApiException(0);
    }
  }
}
