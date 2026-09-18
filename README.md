# VetPass

Plataforma de cartilla de vacunación digital e historial veterinario para perros
y gatos. Producto de **PawCode Studio** para el curso 1ASI0732 — Diseño de
Experimentos de Ingeniería de Software (UPC, NRC 9086).

La clínica registra la información clínica desde una aplicación web y el dueño de
la mascota la consulta desde una aplicación móvil. La cartilla se genera de forma
automática según la especie y el sistema valida las reglas del esquema de
vacunación —edad mínima e intervalo entre dosis— antes de aceptar cada registro.

## Estructura del repositorio

| Carpeta | Contenido | Estado |
|---|---|---|
| `backend/` | RESTful API en ASP.NET Core (C#) con los cuatro bounded contexts | En desarrollo |
| `landing-page/` | Sitio estático en HTML5, CSS3 y JavaScript | Pendiente |
| `web-application/` | Aplicación web para el personal de la clínica (Vue + PrimeVue) | Pendiente |
| `mobile-application/` | Aplicación móvil para el dueño de la mascota (Flutter) | Pendiente |
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

- SDK de .NET 10 (`sudo pacman -S dotnet-sdk aspnet-runtime` en Arch Linux)
- Herramienta de EF Core: `dotnet tool install --global dotnet-ef`

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
dotnet run --project src/VetPass.API
```

La documentación de la API queda disponible en `/swagger`.
