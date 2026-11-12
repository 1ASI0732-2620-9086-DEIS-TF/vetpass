## Qué cambia
<!-- Historia de usuario (USxx) o tarea, y en una o dos frases qué hace este cambio. -->

## Cómo se probó
<!-- Pruebas agregadas o ejecutadas, y pasos para revisarlo a mano si aplica. -->

## Lista de revisión
Quien revisa marca cada punto antes de aprobar (sección 6.2.2 del informe).

- [ ] El CI está en verde: compilación, lint, formato, pruebas y duplicación.
- [ ] Los nombres siguen la guía de la sección 5.1.3: identificadores en inglés y textos visibles en i18n.
- [ ] Cada regla nueva tiene su prueba: unitaria, de integración o un escenario Gherkin.
- [ ] No hay secretos, contraseñas reales ni datos personales en el código.
- [ ] Los errores de la API siguen respondiendo con ProblemDetails y el código correcto.
- [ ] Si hay una migración, se aplicará antes de publicar el código que la necesita.
