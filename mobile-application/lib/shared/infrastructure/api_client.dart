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
  ApiClient({String? baseUrl})
      : baseUrl = baseUrl ??
            const String.fromEnvironment('VETPASS_API',
                defaultValue: 'http://localhost:5199/api/v1');

  final String baseUrl;
  String? _token;

  set token(String? valor) => _token = valor;

  Map<String, String> get _cabeceras => {
        'Content-Type': 'application/json',
        if (_token != null) 'Authorization': 'Bearer $_token',
      };

  Future<dynamic> get(String ruta) => _enviar(() =>
      http.get(Uri.parse('$baseUrl$ruta'), headers: _cabeceras));

  Future<dynamic> post(String ruta, Map<String, dynamic> cuerpo) => _enviar(() =>
      http.post(Uri.parse('$baseUrl$ruta'),
          headers: _cabeceras, body: jsonEncode(cuerpo)));

  Future<dynamic> _enviar(Future<http.Response> Function() peticion) async {
    late http.Response respuesta;
    try {
      respuesta = await peticion();
    } catch (_) {
      throw ApiException(0);
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
}
