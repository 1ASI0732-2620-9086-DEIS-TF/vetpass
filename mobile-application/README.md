# VetPass — Aplicación móvil

Interfaz del dueño de la mascota, construida con Flutter sobre Material
Design 3. Su alcance es de **consulta**: no permite registrar información
clínica, conforme a la restricción declarada en la sección 1.2.1 del informe.

## Pantallas

Las seis de la sección 4.4 del informe:

| Pantalla | Historia |
|---|---|
| Inicio de sesión | US05 |
| Mis mascotas, cada una con su propio estado de cartilla | US12-E2 |
| Cartilla de vacunación | US12-E1 |
| Historial de atenciones | US16-E1 |
| Detalle de atención con su receta | US16-E2 |
| Perfil, idioma y cierre de sesión | US05 |

## Puesta en marcha

Requiere el SDK de Flutter en el PATH y la API en ejecución.

```bash
flutter pub get
flutter run                      # teléfono conectado o emulador
flutter run -d chrome            # para revisar el diseño sin emulador
```

La dirección de la API se puede fijar al compilar, sin tocar el código:

```bash
flutter run --dart-define=VETPASS_API=http://10.0.2.2:5199/api/v1
```

`10.0.2.2` es la dirección con la que el emulador de Android alcanza el
`localhost` de la máquina anfitriona; un teléfono físico necesita la IP de la
red local.

## Estructura

Espejo de los bounded contexts del backend:

| Carpeta | Contenido |
|---|---|
| `lib/iam/` | Sesión, inicio de sesión y perfil |
| `lib/patients/` | Mis mascotas y la ficha de cada una |
| `lib/vaccination/` | Cartilla de vacunación |
| `lib/medical_records/` | Historial y detalle de atención con receta |
| `lib/shared/` | Tema, textos, cliente de la API y marco de navegación |

## Decisiones

**Sin buscador**, porque el dueño gestiona un número reducido de mascotas y el
listado cabe en una pantalla (sección 4.2.4).

**El estado se comunica con color, etiqueta e icono**, nunca solo con color, y
los tonos del informe se sustituyen por variantes más oscuras cuando son texto,
para sostener la relación de contraste de 4.5:1 (sección 4.1.1).

**Español e inglés**, con la terminología de la tabla de la sección 4.2.2. La
etiqueta de cada dosis se compone en la aplicación a partir del nombre de la
vacuna, su secuencia y el indicador de refuerzo que entrega la API.

**La edad se expresa como la diría el dueño**: semanas mientras es cachorro,
que es la unidad del esquema de vacunación, meses durante el primer par de años
y años a partir de ahí.
