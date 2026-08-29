import { defineStore } from 'pinia'
import { ref } from 'vue'

export type AlertType = 'success' | 'error' | 'warning' | 'confirm' | 'info'

export const useAlertStore = defineStore('alert', () => {
  const isOpen = ref(false)
  const title = ref('')
  const message = ref('')
  const type = ref<AlertType>('info')
  const confirmText = ref('')
  const cancelText = ref('')

  let resolvePromise: ((value: boolean) => void) | null = null

  function showAlert(options: {
    title?: string
    message: string
    type?: AlertType
    confirmText?: string
    cancelText?: string
  }): Promise<boolean> {
    isOpen.value = true
    title.value = options.title || ''
    message.value = options.message
    type.value = options.type || 'info'
    confirmText.value = options.confirmText || ''
    cancelText.value = options.cancelText || ''

    return new Promise<boolean>((resolve) => {
      resolvePromise = resolve
    })
  }

  function handleConfirm() {
    isOpen.value = false
    if (resolvePromise) {
      resolvePromise(true)
      resolvePromise = null
    }
  }

  function handleCancel() {
    isOpen.value = false
    if (resolvePromise) {
      resolvePromise(false)
      resolvePromise = null
    }
  }

  return {
    isOpen,
    title,
    message,
    type,
    confirmText,
    cancelText,
    showAlert,
    handleConfirm,
    handleCancel
  }
})
