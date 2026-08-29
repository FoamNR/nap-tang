<script setup lang="ts">
import { computed } from 'vue'
import { useAlertStore } from '../stores/alert'
import { useLangStore } from '../stores/lang'
import { 
  CheckCircle2, 
  AlertCircle, 
  AlertTriangle, 
  Info, 
  HelpCircle 
} from '@lucide/vue'

const alertStore = useAlertStore()
const langStore = useLangStore()

// Map alert types to colors and icons
const config = computed(() => {
  switch (alertStore.type) {
    case 'success':
      return {
        icon: CheckCircle2,
        iconClass: 'text-emerald-500 bg-emerald-50',
        borderClass: 'border-emerald-100',
        shadowClass: 'shadow-emerald-100/40',
        title: alertStore.title || langStore.t('success') || 'สำเร็จ'
      }
    case 'error':
      return {
        icon: AlertCircle,
        iconClass: 'text-rose-500 bg-rose-50',
        borderClass: 'border-rose-100',
        shadowClass: 'shadow-rose-100/40',
        title: alertStore.title || langStore.t('error') || 'ข้อผิดพลาด'
      }
    case 'warning':
      return {
        icon: AlertTriangle,
        iconClass: 'text-amber-500 bg-amber-50',
        borderClass: 'border-amber-100',
        shadowClass: 'shadow-amber-100/40',
        title: alertStore.title || langStore.t('warning') || 'คำเตือน'
      }
    case 'confirm':
      return {
        icon: HelpCircle,
        iconClass: 'text-sky-500 bg-sky-50',
        borderClass: 'border-sky-100',
        shadowClass: 'shadow-sky-100/40',
        title: alertStore.title || langStore.t('confirm') || 'ยืนยัน'
      }
    case 'info':
    default:
      return {
        icon: Info,
        iconClass: 'text-blue-500 bg-blue-50',
        borderClass: 'border-blue-100',
        shadowClass: 'shadow-blue-100/40',
        title: alertStore.title || langStore.t('info') || 'ข้อมูล'
      }
  }
})
</script>

<template>
  <Teleport to="body">
    <!-- Back-drop overlay -->
    <div 
      v-if="alertStore.isOpen" 
      class="fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-[99] flex items-center justify-center p-4"
      @click.self="alertStore.type !== 'confirm' ? alertStore.handleCancel() : null"
    >
      <!-- Alert Container Card -->
      <div 
        class="bg-white border-2 rounded-[2rem] w-full max-w-sm p-6 flex flex-col items-center gap-5 text-center shadow-xl animate-in zoom-in-95 duration-200"
        :class="[config.borderClass, config.shadowClass]"
      >
        <!-- Category styled Icon wrapper -->
        <div 
          class="w-16 h-16 rounded-2xl flex items-center justify-center border-2 border-white shadow-sm"
          :class="config.iconClass"
        >
          <component :is="config.icon" class="w-8 h-8" />
        </div>

        <!-- Title & Content Message -->
        <div class="flex flex-col gap-1.5 w-full">
          <h4 class="font-bold text-slate-800 text-lg cartoon-font">
            {{ config.title }}
          </h4>
          <p class="text-xs text-slate-500 font-bold leading-relaxed whitespace-pre-line px-2">
            {{ alertStore.message }}
          </p>
        </div>

        <!-- Action Dialog Buttons -->
        <div class="flex gap-2.5 w-full mt-1.5">
          <!-- Cancel Button (Confirm Type only) -->
          <button 
            v-if="alertStore.type === 'confirm'"
            type="button" 
            @click="alertStore.handleCancel()"
            class="flex-1 py-3 text-slate-500 hover:text-slate-800 bg-slate-100 hover:bg-slate-200 border-2 border-slate-200/80 rounded-2xl text-xs font-extrabold transition-all duration-200 active:scale-95 shadow-sm shadow-slate-100"
          >
            {{ alertStore.cancelText || langStore.t('cancel') }}
          </button>

          <!-- Confirm / OK Button -->
          <button 
            type="button" 
            @click="alertStore.handleConfirm()"
            class="flex-1 py-3 text-white rounded-2xl text-xs font-extrabold transition-all duration-200 active:scale-95 shadow-md border-2 border-white"
            :class="[
              alertStore.type === 'error' ? 'bg-rose-500 hover:bg-rose-600 shadow-rose-200/50' :
              alertStore.type === 'success' ? 'bg-emerald-500 hover:bg-emerald-600 shadow-emerald-200/50' :
              alertStore.type === 'warning' ? 'bg-amber-500 hover:bg-amber-600 shadow-amber-200/50' :
              'bg-blue-600 hover:bg-blue-700 shadow-blue-200/50'
            ]"
          >
            {{ alertStore.confirmText || (alertStore.type === 'confirm' ? langStore.t('confirm') : langStore.t('ok')) }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
