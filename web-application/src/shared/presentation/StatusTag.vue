<script setup>
/**
 * Estado de una cartilla o de una dosis. El color nunca viaja solo: siempre
 * acompañado de su etiqueta textual y de un icono distintivo, para que resulte
 * legible con deficiencias en la percepción cromática (sección 4.1.1).
 */
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';

const props = defineProps({
  estado: { type: String, required: true },
  cartilla: { type: Boolean, default: false }
});

const { t } = useI18n();

const iconos = {
  UpToDate: 'pi pi-check-circle',
  Applied: 'pi pi-check-circle',
  Pending: 'pi pi-clock',
  Overdue: 'pi pi-exclamation-triangle'
};

const icono = computed(() => iconos[props.estado] ?? 'pi pi-circle');
const texto = computed(() => (props.cartilla
  ? t(`estado.cartilla.${props.estado}`)
  : t(`estado.${props.estado}`)));
</script>

<template>
  <span class="vp-status" :class="`vp-status--${estado}`">
    <i :class="icono" aria-hidden="true" style="font-size: 12px" />
    {{ texto }}
  </span>
</template>
