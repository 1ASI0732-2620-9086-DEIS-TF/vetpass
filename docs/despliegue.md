# Despliegue de VetPass

VetPass se despliega en **Vercel** desde este repositorio público de la
organización del curso en **GitHub**. Cada
`git push` a `main` publica en producción, y cada rama obtiene su propia URL
de vista previa. La base de datos y la autenticación siguen en **Supabase**.

| Pieza | Proyecto de Vercel | Carpeta raíz | URL |
|---|---|---|---|
| Landing page | `vetpass-landing` | `landing-page/` | https://vetpass-landing.vercel.app |
| Aplicación web | `vetpass-web` | `web-application/` | https://vetpass-web.vercel.app |
| RESTful API | `vetpass-api` | `backend/` | https://vetpass-api.vercel.app |
| Base de datos y Auth | Supabase, proyecto VetPass | — | — |
| Aplicación móvil | APK de release | `mobile-application/` | — |

## RESTful API

La API corre como contenedor en Vercel Functions (`runtime: container`, en
beta). La imagen se define en `backend/Dockerfile.vercel` y el servicio en
`backend/vercel.json`.

- Usa `aspnet:10.0-noble-chiseled-extra`. La variante `-extra` trae la base de
  zonas horarias: sin ella el reloj de la clínica no encuentra
  `America/Lima`, pasa a UTC, y desde las 19:00 de Lima «hoy» sería mañana.
- `GET /api/v1/health` responde sin tocar la base e informa la fecha y la zona
  horaria con que la API calcula la cartilla. Debe decir `America/Lima`.
- La API se apaga tras 5 minutos sin tráfico. Antes de una demostración,
  abre `/api/v1/health` para despertarla.

### Variables de entorno del proyecto `vetpass-api`

| Variable | Valor |
|---|---|
| `ConnectionStrings__VetPassDb` | Cadena del *session pooler* de Supabase (secreta) |
| `Supabase__Url` | URL del proyecto de Supabase |
| `Supabase__AnonKey` | Clave pública del proyecto (secreta) |
| `Supabase__ServiceRoleKey` | Clave de servicio (secreta) |
| `Database__AutoMigrate` | `false` |
| `Cors__AllowedOrigins` | URL de la aplicación web; varias, separadas por comas |
| `PORT` | `8080` |

Los valores secretos nunca se escriben en el repositorio. En desarrollo viven
en `dotnet user-secrets`; en producción, solo en Vercel.

### Migraciones

En producción la API no migra al arrancar (`Database__AutoMigrate=false`),
para que cada arranque en frío no repita ese trabajo. Una migración nueva se
aplica una vez, desde la máquina de desarrollo, antes de publicar el código
que la necesita:

```bash
cd backend/src/VetPass.API
dotnet ef database update
```

El comando toma la cadena de conexión de `dotnet user-secrets`.

## Aplicación web

Proyecto Vite. La variable `VITE_API_URL` del proyecto `vetpass-web` apunta a
`https://vetpass-api.vercel.app/api/v1` y se lee al compilar.
`web-application/vercel.json` envía toda ruta a `index.html`, para que
recargar `/pacientes/...` no responda 404.

## Landing page

Sitio estático, sin compilación. Publicada, su botón de acceso lleva a la
aplicación web desplegada; abierta en local, al servidor de desarrollo.

## Aplicación móvil

El APK de release se compila con la dirección pública de la API:

```bash
cd mobile-application
flutter build apk --release --dart-define=VETPASS_API=https://vetpass-api.vercel.app/api/v1
```

Hoy se firma con la clave de depuración: sirve para instalarlo a mano, no
para publicarlo en Google Play.

## Cuentas de demostración

El repositorio es público, así que la contraseña de las cuentas de
demostración no está en él. El valor de `Seed:Password` en
`appsettings.Development.json` solo sirve para sembrar una base local nueva.
Las cuentas de la base compartida usan otra contraseña, guardada en
`dotnet user-secrets` como `Seed:Password`:

```bash
cd backend/src/VetPass.API
dotnet user-secrets list | grep Seed:Password
```

## Supabase

En el plan gratuito, un proyecto se pausa tras 7 días sin actividad. No se
pierde nada y se reactiva desde el panel de Supabase; revísalo antes de una
presentación.
