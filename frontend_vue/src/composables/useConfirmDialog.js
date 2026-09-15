import { readonly, ref } from 'vue'

const visible = ref(false)
const options = ref({})
const queue = []
let resolveCurrent

function show(next) {
  options.value = {
    title: 'Confirmar acción',
    message: '¿Deseas continuar?',
    confirmText: 'Confirmar',
    cancelText: 'Cancelar',
    color: 'error',
    ...next.options
  }
  resolveCurrent = next.resolve
  visible.value = true
}

function finish(value) {
  visible.value = false
  resolveCurrent?.(value)
  resolveCurrent = null
  const next = queue.shift()
  if (next) show(next)
}

export function useConfirmDialog() {
  return {
    visible,
    options: readonly(options),
    confirmAction: (request = {}) => new Promise((resolve) => {
      const next = { options: request, resolve }
      if (resolveCurrent) queue.push(next)
      else show(next)
    }),
    confirm: () => finish(true),
    cancel: () => finish(false)
  }
}
