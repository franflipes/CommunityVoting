<template>
  <v-card-title class="d-flex align-center">
    <v-icon v-if="icon" :icon="icon" class="mr-2" color="primary" />
    <span><slot /></span>
    <v-spacer />
    <v-btn icon="mdi-close" variant="text" aria-label="Cerrar" @click="$emit('close')" />
  </v-card-title>
</template>

<script setup>
defineProps({ icon: { type: String, default: '' } })
defineEmits(['close'])
</script>
