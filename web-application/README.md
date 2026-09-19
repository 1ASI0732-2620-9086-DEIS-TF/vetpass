# VetPass — Aplicación web

Interfaz del personal de la clínica veterinaria. Concentra todas las
operaciones de registro: clientes, mascotas, dosis del esquema de vacunación,
atenciones y recetas.

No contiene reglas de negocio. Toda validación del esquema reside en la
RESTful API, y esta aplicación se limita a presentar lo que aquella responde
—incluido el detalle de una regla incumplida—, de modo que el comportamiento
del producto sea verificable sin depender de la interfaz.

## Puesta en marcha

```bash
npm install
npm run dev        # http://localhost:5173
```

Requiere la API en ejecución. Si no está en `http://localhost:5199/api/v1`,
se indica con `VITE_API_URL` en un archivo `.env.local` (ver `.env.example`).

## Estructura

Espejo de los bounded contexts del backend, de modo que una historia de usuario
se implemente en la misma carpeta a ambos lados:

| Carpeta | Contenido |
|---|---|
| `src/iam/` | Inicio de sesión y sesión del usuario |
| `src/patients/` | Listado, búsqueda y alta de pacientes, y ficha del paciente |
| `src/vaccination/` | Cartilla y registro de dosis |
| `src/medical-records/` | Historial de atenciones y recetas |
| `src/shared/` | Tema, i18n, cliente HTTP, marco de la aplicación |

Dentro de cada contexto, `infrastructure/` habla con la API, `presentation/`
son las vistas y `application/` el estado que estas comparten.

## Decisiones

**PrimeVue 4, no 5.** La versión 5 pasó a licencia comercial (PrimeUI) y
muestra un aviso de licencia inválida sin una clave registrada. La 4.5.5 es
MIT, que es lo que corresponde a un proyecto académico.

**Español e inglés** (sección 4.2.2 del informe), con la terminología de la
tabla de etiquetas: Cartilla → *Vaccination Card*, Historial → *Record*,
Atención → *Visit*. Las etiquetas de las dosis se componen en el cliente a
partir del nombre de la vacuna, su número de secuencia y el indicador de
refuerzo que entrega la API: el idioma es una decisión de la interfaz, no del
dominio.

**Validación al ingresar, no al confirmar** (sección 4.1.2). El diálogo de
registro de dosis se apoya en la fecha esperada que la API ya calculó para
decidir si habilita el botón; cuando la API rechaza el registro, su mensaje se
presenta tal cual, porque incluye la edad exigida y la fecha más temprana
admisible.
