<template>
  <v-tooltip :text="tooltip" location="bottom">
    <template #activator="{ props: activatorProps }">
      <v-btn v-bind="{ ...activatorProps, ...$attrs }" :icon="icon || undefined">
        <slot />
      </v-btn>
    </template>
  </v-tooltip>
</template>

<script setup>
defineOptions({ inheritAttrs: false })
defineProps({
  icon: { type: String, default: '' },
  tooltip: { type: String, required: true }
})
</script>
