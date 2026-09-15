<template>
  <div class="toast-host" aria-live="polite">
    <v-alert
      v-for="toast in toasts"
      :key="toast.id"
      :type="toast.type"
      variant="elevated"
      closable
      @click:close="dismiss(toast.id)"
    >
      {{ toast.message }}
    </v-alert>
  </div>
</template>

<script setup>
import { useToasts } from '@/composables/useToasts'

const { toasts, dismiss } = useToasts()
</script>
