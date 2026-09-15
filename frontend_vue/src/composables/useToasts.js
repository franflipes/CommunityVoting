import { readonly, ref } from 'vue'

const toasts = ref([])
let nextId = 1

function push(type, message) {
  const id = nextId++
  toasts.value = [...toasts.value, { id, type, message }]
  window.setTimeout(() => dismiss(id), type === 'error' ? 5500 : 3500)
}

function dismiss(id) {
  toasts.value = toasts.value.filter((toast) => toast.id !== id)
}

export function useToasts() {
  return {
    toasts: readonly(toasts),
    dismiss,
    success: (message) => push('success', message),
    error: (message) => push('error', message),
    warning: (message) => push('warning', message),
    info: (message) => push('info', message)
  }
}
