<script setup>
/**
 * Historial de atenciones de un paciente (US14). Cada atención se presenta
 * como una entrada resumida que el usuario expande para acceder a su detalle
 * y a su receta asociada (sección 4.2.1), en orden cronológico descendente.
 */
import { useI18n } from 'vue-i18n';
import Button from 'primevue/button';
import Tag from 'primevue/tag';
import { formatearFecha } from '../../shared/i18n';

defineProps({
  atenciones: { type: Array, default: () => [] },
  dueno: { type: String, default: '' }
});

const emit = defineEmits(['nueva-atencion']);
const { t } = useI18n();
</script>

<template>
  <section class="vp-card">
    <header class="encabezado">
      <h3>{{ t('historial.titulo') }}</h3>
      <Button
        :label="t('historial.nueva')"
        icon="pi pi-plus"
        size="small"
        @click="emit('nueva-atencion')"
      />
    </header>

    <p v-if="!atenciones.length" class="vacio vp-muted">{{ t('historial.vacio') }}</p>

    <ul v-else class="lista">
      <li v-for="atencion in atenciones" :key="atencion.id" class="atencion">
        <details>
          <summary>
            <span class="atencion__fecha">{{ formatearFecha(atencion.visitDate) }}</span>
            <span class="atencion__motivo">{{ atencion.reason }}</span>
            <Tag
              v-if="atencion.hasPrescription"
              :value="t('historial.conReceta')"
              severity="secondary"
              class="atencion__chip"
            />
            <i class="pi pi-chevron-down atencion__flecha" aria-hidden="true" />
          </summary>

          <div class="detalle">
            <dl>
              <div v-if="atencion.findings">
                <dt>{{ t('historial.hallazgos') }}</dt>
                <dd>{{ atencion.findings }}</dd>
              </div>
              <div>
                <dt>{{ t('historial.diagnostico') }}</dt>
                <dd>{{ atencion.diagnosis }}</dd>
              </div>
              <div v-if="atencion.treatment">
                <dt>{{ t('historial.tratamiento') }}</dt>
                <dd>{{ atencion.treatment }}</dd>
              </div>
              <div v-if="atencion.weightKg">
                <dt>{{ t('historial.peso') }}</dt>
                <dd>{{ atencion.weightKg }} kg</dd>
              </div>
            </dl>

            <div v-if="atencion.prescription" class="receta">
              <p class="receta__titulo">{{ t('historial.receta') }}</p>
              <ul>
                <li v-for="(item, indice) in atencion.prescription.items" :key="indice">
                  <span class="receta__medicamento">{{ item.medication }}</span>
                  <span class="vp-small vp-muted">{{ item.dosage }} · {{ item.duration }}</span>
                </li>
              </ul>
            </div>
          </div>
        </details>
      </li>
    </ul>

    <footer v-if="atenciones.length" class="pie vp-caption vp-muted">
      {{ t('historial.nota') }}
    </footer>
  </section>
</template>

<style scoped>
.encabezado {
  display: flex; align-items: center; justify-content: space-between; gap: var(--vp-space-3);
  padding: var(--vp-space-4); border-bottom: 1px solid var(--vp-neutral-200);
}
.vacio { padding: var(--vp-space-5); text-align: center; }
.lista { list-style: none; margin: 0; padding: 0; }
.atencion + .atencion { border-top: 1px solid var(--vp-neutral-200); }

summary {
  display: flex; align-items: center; gap: var(--vp-space-3);
  padding: var(--vp-space-3) var(--vp-space-4); min-height: 56px;
  cursor: pointer; list-style: none;
}
summary::-webkit-details-marker { display: none; }
.atencion__fecha { font-variant-numeric: tabular-nums; color: var(--vp-neutral-600); font-size: 14px; }
.atencion__motivo { font-weight: 600; flex: 1; }
.atencion__flecha { color: var(--vp-neutral-600); font-size: 12px; transition: transform .15s ease; }
details[open] .atencion__flecha { transform: rotate(180deg); }

.detalle { padding: 0 var(--vp-space-4) var(--vp-space-4); display: grid; gap: var(--vp-space-3); }
dl { display: grid; gap: var(--vp-space-2); margin: 0; }
dt { font-size: 12px; line-height: 16px; font-weight: 600; color: var(--vp-neutral-600); text-transform: uppercase; letter-spacing: .04em; }
dd { margin: 2px 0 0; }

.receta { padding: var(--vp-space-3); background: var(--vp-primary-50); border-radius: var(--vp-radius); }
.receta__titulo { font-weight: 600; margin-bottom: var(--vp-space-2); }
.receta ul { list-style: none; margin: 0; padding: 0; display: grid; gap: var(--vp-space-2); }
.receta li { display: grid; }
.receta__medicamento { font-weight: 500; }
.pie { padding: var(--vp-space-3) var(--vp-space-4); border-top: 1px solid var(--vp-neutral-200); }
</style>
