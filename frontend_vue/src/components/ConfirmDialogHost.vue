<template>
  <v-dialog v-model="visible" max-width="460" persistent>
    <v-card>
      <v-card-title>{{ options.title }}</v-card-title>
      <v-card-text>{{ options.message }}</v-card-text>
      <v-card-actions>
        <v-spacer />
        <v-btn variant="text" @click="cancel">{{ options.cancelText }}</v-btn>
        <v-btn :color="options.color" variant="flat" @click="confirm">{{ options.confirmText }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { useConfirmDialog } from '@/composables/useConfirmDialog'

const { visible, options, confirm, cancel } = useConfirmDialog()
</script>
