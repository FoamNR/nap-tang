<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useLangStore } from '../stores/lang'
import { useAlertStore } from '../stores/alert'
import { Globe } from '@lucide/vue'

const authStore = useAuthStore()
const langStore = useLangStore()
const alertStore = useAlertStore()

const isRegister = ref(false)
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const displayName = ref('')

const isSubmitting = ref(false)
const errorMessage = ref('')

async function handleSubmit() {
  if (!email.value || !password.value) {
    errorMessage.value = 'fill_all_fields'
    return
  }

  if (isRegister.value) {
    if (!displayName.value) {
      errorMessage.value = 'enter_display_name'
      return
    }
    if (!confirmPassword.value) {
      errorMessage.value = 'fill_all_fields'
      return
    }
    if (password.value !== confirmPassword.value) {
      errorMessage.value = 'password_mismatch'
      return
    }
  }

  isSubmitting.value = true
  errorMessage.value = ''

  try {
    if (isRegister.value) {
      await authStore.register(email.value, password.value, confirmPassword.value, displayName.value)
      // Switch to login and prefill email
      isRegister.value = false
      password.value = ''
      confirmPassword.value = ''
      await alertStore.showAlert({
        message: langStore.t('register_success'),
        type: 'success'
      })
    } else {
      await authStore.login(email.value, password.value)
      navigateTo('/')
    }
  } catch (e: any) {
    errorMessage.value = e.data?.message || 'auth_failed'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center p-4 bg-gradient-to-tr from-[#FFF5F7] via-[#FFFDF9] to-[#EAF2FF] relative overflow-hidden">
    <!-- Premium background glowing gradients -->
    <div class="absolute w-96 h-96 -top-20 -left-20 bg-pink-400/10 rounded-full blur-[100px] pointer-events-none"></div>
    <div class="absolute w-96 h-96 -bottom-20 -right-20 bg-sky-400/10 rounded-full blur-[100px] pointer-events-none"></div>
 
    <div class="glass-card w-full max-w-md p-8 flex flex-col gap-6 relative z-10">
      
      <!-- Language Selector Button -->
      <div class="flex justify-end -mb-2">
        <button 
          type="button"
          @click="langStore.toggleLocale()" 
          class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl border border-pink-100/50 bg-pink-50 hover:bg-pink-100/50 transition-colors text-xs font-bold text-pink-500 shadow-sm"
        >
          <component :is="Globe" class="w-3.5 h-3.5 text-pink-400" />
          <span class="uppercase">{{ langStore.locale }}</span>
        </button>
      </div>
 
      <!-- Brand Logo / Identity -->
      <div class="flex flex-col items-center gap-2">
        <div class="w-14 h-14 bg-gradient-to-tr from-[#FF758F] to-[#4EA8DE] rounded-[1.3rem] flex items-center justify-center shadow-md shadow-pink-200/50">
          <span class="font-bold text-white text-2xl">NT</span>
        </div>
        <h1 class="text-2xl font-bold text-slate-800 mt-2 cartoon-font">
          {{ isRegister ? langStore.t('create_account') : langStore.t('welcome_back') }}
        </h1>
        <p class="text-xs text-slate-400 font-bold">
          {{ isRegister ? langStore.t('register_sub') : langStore.t('login_sub') }}
        </p>
      </div>
 
      <!-- Error Alert -->
      <div v-if="errorMessage" class="bg-rose-50 border border-rose-200 text-rose-600 p-3 rounded-xl text-sm">
        {{ langStore.t(errorMessage) }}
      </div>
 
      <!-- Credentials Form -->
      <form @submit.prevent="handleSubmit()" class="flex flex-col gap-4">
        
        <!-- Display Name (Register Only) -->
        <div v-if="isRegister" class="flex flex-col gap-1">
          <label class="text-xs text-slate-500 font-bold">{{ langStore.t('display_name') }}</label>
          <input 
            v-model="displayName" 
            type="text" 
            placeholder="e.g. John Doe"
            class="glass-input"
            required
          />
        </div>
 
        <!-- Email Field -->
        <div class="flex flex-col gap-1">
          <label class="text-xs text-slate-500 font-bold">{{ langStore.t('email_address') }}</label>
          <input 
            v-model="email" 
            type="email" 
            placeholder="name@example.com"
            class="glass-input"
            required
          />
        </div>
 
        <!-- Password Field -->
        <div class="flex flex-col gap-1">
          <label class="text-xs text-slate-500 font-bold">{{ langStore.t('password') }}</label>
          <input 
            v-model="password" 
            type="password" 
            placeholder="••••••••"
            class="glass-input"
            required
          />
        </div>
 
        <!-- Confirm Password Field (Register Only) -->
        <div v-if="isRegister" class="flex flex-col gap-1">
          <label class="text-xs text-slate-500 font-bold">{{ langStore.t('confirm_password') }}</label>
          <input 
            v-model="confirmPassword" 
            type="password" 
            placeholder="••••••••"
            class="glass-input"
            required
          />
        </div>
 
        <button 
          type="submit" 
          class="btn-primary w-full py-3.5 mt-2 flex items-center justify-center"
          :disabled="isSubmitting"
        >
          <span v-if="isSubmitting" class="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
          <span v-else>{{ isRegister ? langStore.t('sign_up') : langStore.t('log_in') }}</span>
        </button>
      </form>
 
      <!-- Toggle State Footer -->
      <div class="text-center text-xs text-slate-500 mt-2 font-medium">
        <span>{{ isRegister ? langStore.t('already_have_account') : langStore.t('dont_have_account') }}</span>
        <button 
          @click="isRegister = !isRegister; errorMessage = ''" 
          class="text-pink-500 hover:text-pink-600 font-extrabold ml-1.5 focus:outline-none transition-colors"
        >
          {{ isRegister ? langStore.t('login_here') : langStore.t('create_here') }}
        </button>
      </div>
 
    </div>
  </div>
</template>
