# VetPass

Plataforma de cartilla de vacunación digital e historial veterinario para perros
y gatos. Producto de **PawCode Studio** para el curso 1ASI0732 — Diseño de
Experimentos de Ingeniería de Software (UPC, NRC 9086).

La clínica registra la información clínica desde una aplicación web y el dueño de
la mascota la consulta desde una aplicación móvil. La cartilla se genera de forma
automática según la especie y el sistema valida las reglas del esquema de
vacunación —edad mínima e intervalo entre dosis— antes de aceptar cada registro.

## En línea

| Pieza | Dirección |
|---|---|
| Landing page | https://vetpass-landing.vercel.app |
| Aplicación web de la clínica | https://vetpass-web.vercel.app |
| RESTful API | https://vetpass-api.vercel.app/api/v1 (estado en `/api/v1/health`) |
| Aplicación móvil (APK para Android) | [Descargar la última versión](https://github.com/1ASI0732-2620-9086-DEIS-TF/vetpass/releases/latest/download/vetpass.apk) |

Todo se despliega en Vercel desde este repositorio; la base de datos y la
autenticación están en Supabase. Detalles, variables de entorno y migraciones
en [`docs/despliegue.md`](docs/despliegue.md).

## Estructura del repositorio

| Carpeta | Contenido | Estado |
|---|---|---|
| `backend/` | RESTful API en ASP.NET Core (C#) con los cuatro bounded contexts | Construido |
| `landing-page/` | Sitio estático en HTML5, CSS3 y JavaScript | Construido |
| `web-application/` | Aplicación web para el personal de la clínica (Vue + PrimeVue) | Construida |
| `mobile-application/` | Aplicación móvil para el dueño de la mascota (Flutter) | Construida |
| `docs/design/` | Wireframes, mock-ups y prototipos navegables | Anexos E, F y G |
| `docs/spikes/` | Resultados de las spike stories | |
| `VetPass-Informe.md` | Informe del proyecto, capítulos I a IV | |

## Arquitectura

Cuatro bounded contexts según Domain-Driven Design, todos dentro de la misma
RESTful API, cada uno con cuatro capas (Interfaces, Application, Domain,
Infrastructure):

| Bounded Context | Tipo | Responsabilidad |
|---|---|---|
| Identity and Access | Soporte | Identidad y permisos por rol, sobre Supabase Auth |
| Patients | Core | Clientes, mascotas y su localización |
| Vaccination | Core | Cartilla, reglas del esquema y estado |
| Medical Records | Core | Atenciones veterinarias y recetas |

Las tres interfaces de usuario no contienen reglas de negocio: toda validación
del esquema de vacunación reside en la API, de modo que el comportamiento del
producto sea verificable sin depender de la interfaz.

**Persistencia.** PostgreSQL gestionado en Supabase. Las tablas de la aplicación
viven en el esquema `vetpass`, que no se expone a través de PostgREST: el único
cliente de la base de datos es la API. Las credenciales de los usuarios las
custodia Supabase Auth en el esquema `auth`.

## Puesta en marcha

### Requisitos

- SDK de .NET 10. En Arch Linux: `sudo pacman -Syu dotnet-sdk aspnet-runtime`
- Herramienta de EF Core: `dotnet tool install --global dotnet-ef`

El SDK empaquetado por Arch no incluye los datos de *pruning* del framework de
ASP.NET Core, por lo que el proyecto declara `AllowMissingPrunePackageData`.

### Configuración

Los secretos no se versionan. Se cargan con `dotnet user-secrets` desde
`backend/src/VetPass.API`:

```bash
dotnet user-secrets set "Supabase:Url"              "https://<ref>.supabase.co"
dotnet user-secrets set "Supabase:AnonKey"          "<clave publicable>"
dotnet user-secrets set "Supabase:ServiceRoleKey"   "<clave secreta>"
dotnet user-secrets set "ConnectionStrings:VetPassDb" "<cadena del session pooler>"
```

### Ejecución

```bash
cd backend
dotnet ef database update --project src/VetPass.API   # aplica el esquema
dotnet run --project src/VetPass.API
```

La documentación de la API queda disponible en `/swagger`.

El catálogo de vacunas y la plantilla del esquema de vacunación se cargan al
arrancar junto con las migraciones (`Database:AutoMigrate`, activo por
omisión; en producción se desactiva). Para incorporar además el caso de demostración de los mock-ups
—Veterinaria San Miguel y sus pacientes, entre ellos el cachorro cuya segunda
dosis vence hoy— se activa `Seed:Demo`, que ya viene habilitado en el entorno
de desarrollo.

## Landing page

Sitio estático, sin dependencias ni proceso de compilación. Para verlo basta con
servir la carpeta:

```bash
cd landing-page
python3 -m http.server 5180
```

El acceso a la aplicación web (US03) se resuelve con la constante `WEB_APP_URL`
de `assets/js/main.js`: abierta en la propia máquina lleva al servidor de
desarrollo, y publicada, a la aplicación desplegada.

**Idiomas.** La página está en español e inglés, conforme a la sección 4.2.2 del
informe. El español vive en el HTML —es lo que ve quien llega sin JavaScript o
con un rastreador— y el inglés en el diccionario de `assets/js/i18n.js`. El
idioma inicial sale del parámetro `?lang=`, de la elección recordada o del
navegador, en ese orden.

## Aplicación web

```bash
cd web-application
npm install
npm run dev        # http://localhost:5173, requiere la API en ejecución
```

Detalles de estructura y decisiones en `web-application/README.md`.

## Aplicación móvil

```bash
cd mobile-application
flutter pub get
flutter run        # teléfono o emulador; requiere la API en ejecución
```

Detalles en `mobile-application/README.md`.
