<script setup>
/**
 * Registro de una dosis aplicada (US10), en sus dos resultados posibles.
 *
 * La validación corre al ingresar la fecha y no al confirmar, conforme al
 * criterio de la sección 4.1.2: el usuario no descubre el error al final. La
 * comprobación que hace la interfaz se apoya en la fecha esperada que la API
 * ya calculó para esa dosis, de modo que no duplica la regla del dominio; la
 * autoridad sigue siendo la API, que responde 422 con la regla incumplida, su
 * valor exigido y la fecha más temprana admisible.
 */
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import Dialog from 'primevue/dialog';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import DatePicker from 'primevue/datepicker';
import Message from 'primevue/message';
import { vaccinationApi } from '../infrastructure/vaccination.api';
import { problemOf } from '../../shared/infrastructure/http';
import { formatearFecha, etiquetaDosis } from '../../shared/i18n';

const props = defineProps({
  visible: { type: Boolean, default: false },
  petId: { type: String, required: true },
  paciente: { type: Object, default: null },
  dosis: { type: Object, default: null }
});

const emit = defineEmits(['update:visible', 'registrada', 'otra-dosis']);

const { t, locale } = useI18n();

const fecha = ref(new Date());
const lote = ref('');
const enviando = ref(false);
const rechazo = ref(null);

watch(() => props.visible, (abierto) => {
  if (!abierto) return;
  fecha.value = new Date();
  lote.value = '';
  rechazo.value = null;
});

const fechaISO = computed(() => {
  const f = fecha.value;
  if (!f) return null;
  return new Date(f.getTime() - f.getTimezoneOffset() * 60000).toISOString().slice(0, 10);
});

const hoyISO = new Date().toLocaleDateString('sv-SE');

/**
 * Aviso previo al envío. La fecha esperada de la dosis es la más temprana en
 * que el esquema la admite, porque la API la recalcula cuando se aplica la
 * dosis anterior.
 */
const avisoLocal = computed(() => {
  if (!props.dosis || !fechaISO.value) return null;

  if (fechaISO.value > hoyISO) {
    return { severidad: 'error', texto: t('dosis.futura', { fecha: formatearFecha(hoyISO) }) };
  }

  if (fechaISO.value < props.dosis.expectedDate) {
    return {
      severidad: 'warn',
      texto: t('dosis.aun_no', { fecha: formatearFecha(props.dosis.expectedDate) })
    };
  }

  return { severidad: 'success', texto: t('dosis.valida') };
});

const puedeRegistrar = computed(() =>
  Boolean(lote.value.trim()) && avisoLocal.value?.severidad === 'success' && !rechazo.value);

async function registrar() {
  enviando.value = true;
  rechazo.value = null;
  try {
    await vaccinationApi.registrarDosis(props.petId, props.dosis.id, {
      applicationDate: fechaISO.value,
      batchCode: lote.value.trim()
    });
    emit('registrada');
    cerrar();
  } catch (fallo) {
    rechazo.value = problemOf(fallo);
  } finally {
    enviando.value = false;
  }
}

function cerrar() { emit('update:visible', false); }

function otraDosis() { emit('otra-dosis'); cerrar(); }
</script>

<template>
  <Dialog
    :visible="visible"
    modal
    :header="t('dosis.titulo')"
    :style="{ width: '32rem' }"
    :breakpoints="{ '640px': '95vw' }"
    @update:visible="emit('update:visible', $event)"
  >
    <p v-if="paciente" class="vp-small vp-muted paciente">
      {{ paciente.name }} · {{ t(`especie.${paciente.species}`) }} ·
      {{ t('ficha.semanas', { n: paciente.ageInWeeks }) }}
    </p>

    <form class="formulario" @submit.prevent="registrar">
      <div class="vp-field">
        <label for="d-vacuna">{{ t('dosis.vacuna') }}</label>
        <InputText id="d-vacuna" :value="dosis ? etiquetaDosis(dosis, t) : ''" readonly />
      </div>

      <div class="vp-field">
        <label for="d-fecha">{{ t('dosis.fechaAplicacion') }}</label>
        <DatePicker
          input-id="d-fecha"
          v-model="fecha"
          date-format="dd/mm/yy"
          :max-date="new Date()"
          show-icon
        />
      </div>

      <!-- Resultado de la validación: el mensaje nombra la regla y su valor
           exigido, no un aviso genérico (sección 4.6.3). -->
      <Message
        v-if="rechazo"
        severity="error"
        :closable="false"
        class="resultado"
      >
        <strong>{{ rechazo.title }}</strong>
        <span class="vp-small">{{ rechazo.detail }}</span>
      </Message>

      <Message
        v-else-if="avisoLocal"
        :severity="avisoLocal.severidad"
        :closable="false"
        class="resultado"
      >
        <span class="vp-small">{{ avisoLocal.texto }}</span>
      </Message>

      <div class="vp-field">
        <label for="d-lote">{{ t('dosis.lote') }}</label>
        <InputText id="d-lote" v-model="lote" required />
      </div>
    </form>

    <template #footer>
      <Button
        v-if="rechazo"
        :label="t('dosis.otra')"
        text
        @click="otraDosis"
      />
      <Button :label="t('comun.cancelar')" text @click="cerrar" />
      <Button
        :label="t('dosis.registrar')"
        :disabled="!puedeRegistrar"
        :loading="enviando"
        @click="registrar"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.paciente { margin-bottom: var(--vp-space-3); }
.formulario { display: grid; gap: var(--vp-space-3); }
.resultado :deep(.p-message-text) { display: grid; gap: 2px; }
</style>
