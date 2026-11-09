<script setup>
/**
 * Historial de atenciones de un paciente (US14). Cada atención se presenta
 * como una entrada resumida que el usuario expande para acceder a su detalle
 * y a su receta asociada (sección 4.2.1), en orden cronológico descendente.
 */
import { useI18n } from 'vue-i18n';
import Button from 'primevue/button';
import Tag from 'primevue/tag';
import { formatDate } from '../../shared/i18n';

defineProps({
  visits: { type: Array, default: () => [] },
  owner: { type: String, default: '' }
});

const emit = defineEmits(['new-visit']);
const { t } = useI18n();
</script>

<template>
  <section class="vp-card">
    <header class="header">
      <h3>{{ t('history.title') }}</h3>
      <Button
        :label="t('history.new')"
        icon="pi pi-plus"
        size="small"
        @click="emit('new-visit')"
      />
    </header>

    <p v-if="!visits.length" class="empty vp-muted">{{ t('history.empty') }}</p>

    <ul v-else class="list">
      <li v-for="visit in visits" :key="visit.id" class="visit">
        <details>
          <summary>
            <span class="visit__date">{{ formatDate(visit.visitDate) }}</span>
            <span class="visit__reason">{{ visit.reason }}</span>
            <Tag
              v-if="visit.hasPrescription"
              :value="t('history.withPrescription')"
              severity="secondary"
              class="visit__chip"
            />
            <i class="pi pi-chevron-down visit__arrow" aria-hidden="true" />
          </summary>

          <div class="detail">
            <dl>
              <div v-if="visit.findings">
                <dt>{{ t('history.findings') }}</dt>
                <dd>{{ visit.findings }}</dd>
              </div>
              <div>
                <dt>{{ t('history.diagnosis') }}</dt>
                <dd>{{ visit.diagnosis }}</dd>
              </div>
              <div v-if="visit.treatment">
                <dt>{{ t('history.treatment') }}</dt>
                <dd>{{ visit.treatment }}</dd>
              </div>
              <div v-if="visit.weightKg">
                <dt>{{ t('history.weight') }}</dt>
                <dd>{{ visit.weightKg }} kg</dd>
              </div>
            </dl>

            <div v-if="visit.prescription" class="prescription">
              <p class="prescription__title">{{ t('history.prescription') }}</p>
              <ul>
                <li v-for="(item, index) in visit.prescription.items" :key="index">
                  <span class="prescription__medication">{{ item.medication }}</span>
                  <span class="vp-small vp-muted">{{ item.dosage }} · {{ item.duration }}</span>
                </li>
              </ul>
            </div>
          </div>
        </details>
      </li>
    </ul>

    <footer v-if="visits.length" class="footer vp-caption vp-muted">
      {{ t('history.note') }}
    </footer>
  </section>
</template>

<style scoped>
.header {
  display: flex; align-items: center; justify-content: space-between; gap: var(--vp-space-3);
  padding: var(--vp-space-4); border-bottom: 1px solid var(--vp-neutral-200);
}
.empty { padding: var(--vp-space-5); text-align: center; }
.list { list-style: none; margin: 0; padding: 0; }
.visit + .visit { border-top: 1px solid var(--vp-neutral-200); }

summary {
  display: flex; align-items: center; gap: var(--vp-space-3);
  padding: var(--vp-space-3) var(--vp-space-4); min-height: 56px;
  cursor: pointer; list-style: none;
}
summary::-webkit-details-marker { display: none; }
.visit__date { font-variant-numeric: tabular-nums; color: var(--vp-neutral-600); font-size: 14px; }
.visit__reason { font-weight: 600; flex: 1; }
.visit__arrow { color: var(--vp-neutral-600); font-size: 12px; transition: transform .15s ease; }
details[open] .visit__arrow { transform: rotate(180deg); }

.detail { padding: 0 var(--vp-space-4) var(--vp-space-4); display: grid; gap: var(--vp-space-3); }
dl { display: grid; gap: var(--vp-space-2); margin: 0; }
dt { font-size: 12px; line-height: 16px; font-weight: 600; color: var(--vp-neutral-600); text-transform: uppercase; letter-spacing: .04em; }
dd { margin: 2px 0 0; }

.prescription { padding: var(--vp-space-3); background: var(--vp-primary-50); border-radius: var(--vp-radius); }
.prescription__title { font-weight: 600; margin-bottom: var(--vp-space-2); }
.prescription ul { list-style: none; margin: 0; padding: 0; display: grid; gap: var(--vp-space-2); }
.prescription li { display: grid; }
.prescription__medication { font-weight: 500; }
.footer { padding: var(--vp-space-3) var(--vp-space-4); border-top: 1px solid var(--vp-neutral-200); }
</style>
