<script setup>
/**
 * Estado de una cartilla o de una dosis. El color nunca viaja solo: siempre
 * acompañado de su etiqueta textual y de un icono distintivo, para que resulte
 * legible con deficiencias en la percepción cromática (sección 4.1.1).
 */
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';

const props = defineProps({
  status: { type: String, required: true },
  card: { type: Boolean, default: false }
});

const { t } = useI18n();

const icons = {
  UpToDate: 'pi pi-check-circle',
  Applied: 'pi pi-check-circle',
  Pending: 'pi pi-clock',
  Overdue: 'pi pi-exclamation-triangle'
};

const icon = computed(() => icons[props.status] ?? 'pi pi-circle');
const text = computed(() => (props.card
  ? t(`status.card.${props.status}`)
  : t(`status.${props.status}`)));
</script>

<template>
  <span class="vp-status" :class="`vp-status--${status}`">
    <i :class="icon" aria-hidden="true" style="font-size: 12px" />
    {{ text }}
  </span>
</template>
