# SP01 — Definición del esquema de vacunación canino y felino

**Spike Story:** SP01 · **Epic:** EP04 — Cartilla de Vacunación Digital
**Estado:** propuesta implementada, pendiente de validación por médico veterinario

---

## 1. Pregunta del spike

> ¿El esquema de vacunación de perros y gatos puede modelarse como una plantilla
> fija por especie, o requiere ser configurable por cada clínica?

De la respuesta depende una decisión de diseño estructural: si el esquema es
constante, `schedule_items` es una tabla de solo lectura y la cartilla se genera
sola al registrar la mascota (US09); si varía por clínica, el esquema pasa a ser
dato administrable y el alcance del producto crece.

## 2. Método

1. Revisión de las guías de vacunación de la WSAVA (*World Small Animal
   Veterinary Association*), que son la referencia internacional que siguen las
   clínicas de animales de compañía.
2. Contraste con lo declarado por los veterinarios en las entrevistas de la
   sección 2.2 del informe.
3. Verificación de que el modelo resultante reproduce el caso de demostración de
   los mock-ups de las secciones 4.4 y 4.6.

## 3. Hallazgo

El esquema **es constante en su estructura** entre clínicas: las guías definen
las mismas vacunas esenciales, la misma edad mínima de inicio y el mismo
intervalo entre dosis del esquema de cachorro. Lo que varía entre
establecimientos es la marca comercial del producto aplicado y la inclusión de
vacunas no esenciales, no la secuencia.

**Conclusión:** se modela como **plantilla fija por especie**, conforme a la
restricción declarada en la sección 1.2.1 del informe. La plantilla se
materializa como dosis concretas al crear la cartilla, de modo que una futura
modificación del esquema no altere las cartillas ya emitidas.

## 4. Esquema canino

| # | Vacuna | Esencial | Edad mínima | Intervalo mínimo desde la dosis anterior |
|---|---|---|---|---|
| 1 | Quíntuple (moquillo, hepatitis, parvovirus, parainfluenza, leptospirosis) | Sí | 6 semanas | — |
| 2 | Quíntuple · 2.ª dosis | Sí | 9 semanas | 3 semanas |
| 3 | Quíntuple · 3.ª dosis | Sí | 12 semanas | 3 semanas |
| 4 | Antirrábica | Sí | 12 semanas | — |
| 5 | Quíntuple · refuerzo anual | Sí | 52 semanas | 40 semanas |
| 6 | Antirrábica · refuerzo anual | Sí | 64 semanas | 52 semanas |

## 5. Esquema felino

| # | Vacuna | Esencial | Edad mínima | Intervalo mínimo desde la dosis anterior |
|---|---|---|---|---|
| 1 | Triple felina (rinotraqueítis, calicivirus, panleucopenia) | Sí | 6 semanas | — |
| 2 | Triple felina · 2.ª dosis | Sí | 9 semanas | 3 semanas |
| 3 | Triple felina · 3.ª dosis | Sí | 12 semanas | 3 semanas |
| 4 | Leucemia felina | No | 8 semanas | — |
| 5 | Leucemia felina · 2.ª dosis | No | 11 semanas | 3 semanas |
| 6 | Antirrábica | Sí | 12 semanas | — |
| 7 | Triple felina · refuerzo anual | Sí | 52 semanas | 40 semanas |
| 8 | Leucemia felina · refuerzo anual | No | 60 semanas | 49 semanas |
| 9 | Antirrábica · refuerzo anual | Sí | 64 semanas | 52 semanas |

La vacuna contra la leucemia felina se clasifica como no esencial porque su
aplicación depende de la exposición del animal. Se incluye en la plantilla
porque el caso de demostración de la aplicación móvil la utiliza, y porque los
veterinarios entrevistados la aplican de forma habitual en gatos con acceso al
exterior.

## 6. Reglas de cálculo

Estas son las reglas que el dominio implementa y que la suite de pruebas
verificará:

1. **Fecha esperada (US09-E2, US09-E3).** Cada vacuna es una serie ordenada.
   La fecha esperada de cada dosis pendiente es
   `máx(fecha de nacimiento + edad mínima, dosis anterior de la serie + intervalo mínimo, hoy)`,
   donde la dosis anterior cuenta con su fecha de aplicación si ya se aplicó y
   con su fecha esperada si no. El último término es el que permite registrar
   a una mascota adulta o sin historial conocido: su esquema comienza el día
   del registro y no muestra dosis que se esperaban años atrás.
2. **Replanificación al registrar una dosis.** Al aplicarse una dosis, las
   pendientes **de esa misma vacuna** vuelven a calcularse con la regla 1. Así,
   si una dosis se aplica con retraso, o se registra una aplicada antes del
   ingreso de la mascota, el resto de la serie se desplaza a partir de hoy en
   lugar de arrastrar fechas imposibles de cumplir. Las demás vacunas no se
   replanifican: una dosis que nadie aplicó debe seguir figurando como vencida
   (US11-E3).
3. **Orden de la serie (US10-E5).** Se rechaza una dosis mientras quede
   pendiente una anterior de la misma vacuna. El orden rige dentro de cada
   vacuna y no entre vacunas: la 3.ª quíntuple y la 1.ª antirrábica pueden
   registrarse el mismo día, igual que la triple felina, la leucemia y la
   antirrábica en un gato, siempre que cada una cumpla su edad mínima.
4. **Edad mínima (US10-E2).** Se rechaza la dosis si
   `fecha de aplicación < fecha de nacimiento + edad mínima`.
5. **Intervalo mínimo (US10-E3).** Se rechaza la dosis si
   `fecha de aplicación < fecha de la dosis anterior aplicada de la misma vacuna + intervalo mínimo`.
6. **Fecha futura (US10-E4).** Se rechaza toda fecha de aplicación posterior a
   la fecha actual.
7. **Dosis aplicada antes del registro (US10-E6).** La fecha esperada no es la
   fecha mínima admisible. Una dosis aplicada antes de que la mascota llegara
   a la plataforma se acepta con su fecha real si cumple las reglas 4 y 5; la
   fecha más temprana admisible es
   `máx(fecha de nacimiento + edad mínima, dosis anterior aplicada + intervalo mínimo)`.
8. **Estado de la cartilla (US11).**
   - *Al día*: no existe ninguna dosis pendiente con fecha esperada anterior a hoy.
   - *Pendiente*: existen dosis sin aplicar, todas con fecha esperada igual o posterior a hoy.
   - *Vencida*: existe al menos una dosis sin aplicar cuya fecha esperada ya transcurrió.

La fecha actual entra al dominio **como parámetro**, nunca leída del reloj del
sistema, según la decisión de la sección 4.9.1 del informe.

## 7. Verificación contra el caso de demostración

El paciente Rocky de los mock-ups de la sección 4.6 es un beagle de 9 semanas
con la primera dosis de quíntuple aplicada a las 6 semanas:

| Situación | Resultado esperado por el mock-up | Resultado del esquema propuesto |
|---|---|---|
| Registrar la 2.ª dosis de quíntuple hoy | Aceptada | Aceptada: 9 semanas ≥ 9 semanas de edad mínima y 3 semanas desde la 1.ª dosis |
| Registrar la antirrábica hoy | Rechazada, edad mínima 12 semanas | Rechazada: 9 semanas < 12 semanas |
| Fecha más temprana admisible de la antirrábica | 3 semanas después | `nacimiento + 12 semanas` |

El esquema reproduce los dos caminos de la pantalla de registro de dosis, que
son los criterios de aceptación US10-E1 y US10-E2.

## 8. Pendiente de validación

Los valores de esta propuesta provienen de las guías citadas y no de un
protocolo firmado por un profesional. Antes de la defensa del producto, un
médico veterinario debe revisar la tabla de las secciones 4 y 5 y confirmar o
corregir cada edad mínima e intervalo.

La corrección, de ser necesaria, **no requiere cambios de código**: los valores
residen en la tabla `schedule_items` y se cargan desde el seed del proyecto.

Queda también pendiente la **serie reducida de recuperación** para adultos. Las
guías admiten que un perro o un gato adulto sin historial reciba una serie
inicial más corta que la de un cachorro —por ejemplo, dos dosis de quíntuple
separadas por tres o cuatro semanas en lugar de tres—. La plataforma aplica por
ahora la misma serie del cachorro, reprogramada desde el día del registro
(regla 1): es la opción conservadora, porque nunca omite una dosis. Reducir la
serie para adultos exige que un médico veterinario fije el criterio de edad y
el número de dosis, y entonces sí requiere cambios en el dominio.

## 9. Fuentes

- WSAVA Vaccination Guidelines Group. *Guidelines for the Vaccination of Dogs
  and Cats*. World Small Animal Veterinary Association.
- Entrevistas a personal de clínicas veterinarias, sección 2.2 del informe.
