<script setup>
import { useI18n } from 'vue-i18n';
import { changeLanguage } from '../i18n';

const { locale, t } = useI18n();
</script>

<template>
  <div class="lang" role="group" :aria-label="t('nav.language')">
    <button
      v-for="language in ['es', 'en']"
      :key="language"
      type="button"
      class="lang__btn"
      :aria-pressed="locale === language"
      :lang="language"
      @click="changeLanguage(language)"
    >
      {{ language.toUpperCase() }}
    </button>
  </div>
</template>

<style scoped>
.lang {
  display: inline-flex; gap: 2px; padding: 2px;
  background: var(--vp-surface-50);
  border: 1px solid var(--vp-neutral-200); border-radius: 999px;
}
.lang__btn {
  min-width: 36px; min-height: 28px; padding: 0 8px;
  font-family: inherit; font-size: 12px; font-weight: 600;
  color: var(--vp-neutral-600); background: none; border: 0;
  border-radius: 999px; cursor: pointer;
}
.lang__btn:hover { color: var(--vp-neutral-900); }
.lang__btn[aria-pressed="true"] { color: #fff; background: var(--vp-primary-700); }
</style>
