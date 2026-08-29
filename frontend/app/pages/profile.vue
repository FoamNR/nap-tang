<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useLangStore } from '../stores/lang'
import { useAlertStore } from '../stores/alert'
import { User, Lock, LogOut } from '@lucide/vue'

const authStore = useAuthStore()
const langStore = useLangStore()
const alertStore = useAlertStore()

const displayName = ref(authStore.user?.displayName || '')
const newPassword = ref('')
const confirmPassword = ref('')
const isSubmitting = ref(false)

async function handleSave() {
  if (!displayName.value.trim()) {
    await alertStore.showAlert({
      message: langStore.t('enter_display_name'),
      type: 'warning'
    })
    return
  }

  // If password change is requested
  if (newPassword.value) {
    if (newPassword.value.length < 8) {
      await alertStore.showAlert({
        message: langStore.t('password') + ' must be at least 8 characters.',
        type: 'warning'
      })
      return
    }

    if (newPassword.value !== confirmPassword.value) {
      await alertStore.showAlert({
        message: langStore.t('password_mismatch'),
        type: 'warning'
      })
      return
    }
  }

  isSubmitting.value = true

  try {
    await authStore.updateProfile(displayName.value.trim(), newPassword.value || undefined, confirmPassword.value || undefined)
    
    // Clear passwords
    newPassword.value = ''
    confirmPassword.value = ''
    
    await alertStore.showAlert({
      message: langStore.t('profile_updated'),
      type: 'success'
    })
  } catch (e: any) {
    await alertStore.showAlert({
      message: e.data?.message || langStore.t('profile_update_failed'),
      type: 'error'
    })
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="flex flex-col gap-6 w-full max-w-lg mx-auto">
    <!-- Header -->
    <div class="flex flex-col">
      <span class="text-[11px] text-pink-500 font-bold tracking-wider uppercase">{{ langStore.t('edit_account') }}</span>
      <h1 class="text-2xl font-bold text-slate-800 mt-0.5 cartoon-font">{{ langStore.t('edit_account') }}</h1>
    </div>

    <!-- Edit Form Card -->
    <div class="glass-card p-6 md:p-8 flex flex-col gap-6 border-2 border-pink-100/60 shadow-lg shadow-pink-100/10">
      
      <!-- User Avatar graphic -->
      <div class="flex flex-col items-center gap-3 py-2">
        <div class="w-20 h-20 rounded-3xl bg-gradient-to-tr from-pink-400 to-sky-400 border-4 border-white text-white flex items-center justify-center font-extrabold text-3xl shadow-md shadow-pink-200/50">
          {{ displayName.substring(0, 1).toUpperCase() || 'U' }}
        </div>
        <span class="text-xs text-slate-400 font-bold uppercase tracking-wider">{{ authStore.user?.email }}</span>
      </div>

      <form @submit.prevent="handleSave()" class="flex flex-col gap-5">
        <!-- Display Name Field -->
        <div class="flex flex-col gap-1.5">
          <label class="text-xs text-slate-500 font-bold flex items-center gap-1.5">
            <component :is="User" class="w-4 h-4 text-slate-400" />
            {{ langStore.t('display_name') }}
          </label>
          <input 
            v-model="displayName" 
            type="text" 
            class="glass-input" 
            required 
            placeholder="e.g. John Doe"
          />
        </div>

        <!-- New Password Field -->
        <div class="flex flex-col gap-1.5">
          <label class="text-xs text-slate-500 font-bold flex items-center gap-1.5">
            <component :is="Lock" class="w-4 h-4 text-slate-400" />
            {{ langStore.t('new_password') }}
          </label>
          <input 
            v-model="newPassword" 
            type="password" 
            class="glass-input" 
            placeholder="••••••••"
          />
        </div>

        <!-- Confirm Password Field -->
        <div class="flex flex-col gap-1.5" v-if="newPassword">
          <label class="text-xs text-slate-500 font-bold flex items-center gap-1.5">
            <component :is="Lock" class="w-4 h-4 text-slate-400" />
            {{ langStore.t('confirm_password') }}
          </label>
          <input 
            v-model="confirmPassword" 
            type="password" 
            class="glass-input" 
            placeholder="••••••••"
            required
          />
        </div>

        <!-- Submit Button -->
        <button 
          type="submit" 
          class="btn-primary py-3.5 mt-2 flex items-center justify-center gap-2"
          :disabled="isSubmitting"
        >
          <span v-if="isSubmitting" class="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
          <span v-else>{{ langStore.t('save_transaction') }}</span>
        </button>

        <!-- Logout Button (Mobile Only / Secondary) -->
        <button 
          type="button"
          @click="authStore.logout()"
          class="md:hidden flex items-center justify-center gap-2 mt-2 py-3 text-rose-600 hover:text-white border-2 border-rose-100 hover:bg-rose-600 rounded-2xl text-xs font-extrabold transition-all duration-200 active:scale-95 shadow-sm w-full"
        >
          <component :is="LogOut" class="w-4 h-4" />
          {{ langStore.t('logout') }}
        </button>
      </form>

    </div>
  </div>
</template>
