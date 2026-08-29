<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useTransactionStore } from '../stores/transactions'
import { useLangStore } from '../stores/lang'
import { useAlertStore } from '../stores/alert'
import { 
  Utensils, 
  Car, 
  ShoppingBag, 
  Receipt, 
  Tv, 
  Briefcase, 
  TrendingUp, 
  Coins, 
  LayoutDashboard, 
  Plus, 
  LogOut, 
  Upload, 
  X, 
  Calendar, 
  ChevronRight,
  User,
  Globe,
  Home,
  Droplet,
  Zap,
  MoreHorizontal
} from '@lucide/vue'

const authStore = useAuthStore()
const txStore = useTransactionStore()
const langStore = useLangStore()
const alertStore = useAlertStore()

const iconMap = {
  Utensils,
  Car,
  ShoppingBag,
  Receipt,
  Tv,
  Briefcase,
  TrendingUp,
  Coins,
  LayoutDashboard,
  Plus,
  LogOut,
  Upload,
  X,
  Calendar,
  ChevronRight,
  User,
  Globe,
  Home,
  Droplet,
  Zap,
  MoreHorizontal
}

// Redirect if not authenticated
onMounted(() => {
  if (!authStore.isAuthenticated) {
    navigateTo('/login')
  } else {
    txStore.fetchCategories()
  }
})

// Add Transaction Form State
const amount = ref<number | ''>('')
const type = ref<'expense' | 'income'>('expense')
const categoryId = ref('')
const description = ref('')
const transactionDate = ref(new Date().toISOString().substring(0, 10))
const slipUrl = ref<string | null>(null)
const isUploading = ref(false)
const errorMessage = ref('')

const filteredCategories = computed(() => {
  return txStore.categories.filter(c => c.type === type.value)
})

// Auto select first category when type changes
watch(type, () => {
  const cats = filteredCategories.value
  if (cats.length > 0) {
    categoryId.value = cats[0].id
  } else {
    categoryId.value = ''
  }
})

// File upload handler
async function handleFileUpload(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  // Validate size
  if (file.size > 5 * 1024 * 1024) {
    errorMessage.value = 'File exceeds the 5 MB limit.'
    return
  }

  // Validate extension
  const allowedExtensions = ['.jpg', '.jpeg', '.png', '.webp']
  const fileExt = file.name.substring(file.name.lastIndexOf('.')).toLowerCase()
  if (!allowedExtensions.includes(fileExt)) {
    errorMessage.value = 'Unsupported file type. Allowed formats: JPEG, PNG, WEBP.'
    return
  }

  isUploading.value = true
  errorMessage.value = ''

  try {
    const url = await txStore.uploadSlip(file)
    slipUrl.value = url
  } catch (e: any) {
    errorMessage.value = e.data?.message || 'Failed to upload receipt slip.'
  } finally {
    isUploading.value = false
  }
}

function removeSlip() {
  slipUrl.value = null
}

async function handleSave() {
  if (!amount.value || amount.value <= 0) {
    errorMessage.value = 'Please enter a valid amount.'
    return
  }
  if (!categoryId.value) {
    errorMessage.value = 'Please select a category.'
    return
  }

  try {
    errorMessage.value = ''
    await txStore.createTransaction({
      amount: Number(amount.value),
      type: type.value,
      categoryId: categoryId.value,
      description: description.value || null,
      transactionDate: new Date(transactionDate.value).toISOString(),
      slipUrl: slipUrl.value
    })

    // Reset Form
    amount.value = ''
    description.value = ''
    slipUrl.value = null
    txStore.isAddModalOpen = false
    
    // Refresh lists & analytics if we are on dashboard or analytics
    await txStore.fetchTransactions()
    
    // Refresh analytics summary for current month bounds dynamically
    const now = new Date()
    const start = new Date(now.getFullYear(), now.getMonth(), 1)
    const end = new Date(now.getFullYear(), now.getMonth() + 1, 0)
    const toISOStringLocalDate = (d: Date) => {
      const offset = d.getTimezoneOffset()
      const local = new Date(d.getTime() - (offset * 60 * 1000))
      return local.toISOString().substring(0, 10)
    }
    await txStore.fetchAnalytics(toISOStringLocalDate(start), toISOStringLocalDate(end))
  } catch (e: any) {
    errorMessage.value = e.data?.message || 'Failed to save transaction.'
  }
}

const showCustomCategoryInput = ref(false)
const newCategoryName = ref('')
const isCreatingCategory = ref(false)

async function handleCreateCustomCategory() {
  if (!newCategoryName.value.trim()) {
    await alertStore.showAlert({
      message: langStore.t('custom_cat_error'),
      type: 'warning'
    })
    return
  }
  isCreatingCategory.value = true
  try {
    const iconName = type.value === 'income' ? 'Coins' : 'ShoppingBag'
    const colorHex = type.value === 'income' ? '#10B981' : '#64748B'
    await txStore.createCategory(newCategoryName.value.trim(), type.value, iconName, colorHex)
    
    // Auto select the newly created category
    const newlyCreated = txStore.categories.find(
      c => c.name.toLowerCase() === newCategoryName.value.trim().toLowerCase() && c.type === type.value
    )
    if (newlyCreated) {
      categoryId.value = newlyCreated.id
    }
    
    newCategoryName.value = ''
    showCustomCategoryInput.value = false
  } catch (e) {
    console.error('Failed to create custom category', e)
    await alertStore.showAlert({
      message: langStore.t('save_failed'),
      type: 'error'
    })
  } finally {
    isCreatingCategory.value = false
  }
}

function closeAddModal() {
  txStore.isAddModalOpen = false
  errorMessage.value = ''
  amount.value = ''
  description.value = ''
  slipUrl.value = null
  showCustomCategoryInput.value = false
  newCategoryName.value = ''
}
</script>

<template>
  <div class="min-h-screen bg-gradient-to-tr from-[#FFF5F7] via-[#FFFDF9] to-[#EAF2FF] text-slate-800">
    <PopupAlert />
    <div v-if="authStore.isAuthenticated" class="min-h-screen flex flex-col md:flex-row pb-20 md:pb-0">
    
    <!-- Sidebar (Desktop Only) -->
    <aside class="hidden md:flex flex-col w-64 bg-white/70 backdrop-blur-md border-r-2 border-pink-100/60 p-6 justify-between shrink-0">
      <div class="flex flex-col gap-8">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 bg-gradient-to-tr from-pink-400 to-sky-400 rounded-2xl flex items-center justify-center shadow-md shadow-pink-200/50">
            <span class="font-bold text-white text-lg">NT</span>
          </div>
          <span class="text-xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-[#FF758F] to-[#4EA8DE] cartoon-font">{{ langStore.t('appName') }}</span>
        </div>
 
        <nav class="flex flex-col gap-2">
          <NuxtLink 
            to="/" 
            class="flex items-center gap-3 px-4 py-3 rounded-2xl transition-all duration-200"
            active-class="bg-pink-50 text-[#FF758F] font-bold border-2 border-pink-100/50 shadow-sm shadow-pink-100/10"
            inactive-class="text-slate-500 hover:text-slate-900 hover:bg-slate-100"
          >
            <component :is="iconMap.LayoutDashboard" class="w-5 h-5" />
            {{ langStore.t('dashboard') }}
          </NuxtLink>
          <NuxtLink 
            to="/analytics" 
            class="flex items-center gap-3 px-4 py-3 rounded-2xl transition-all duration-200"
            active-class="bg-pink-50 text-[#FF758F] font-bold border-2 border-pink-100/50 shadow-sm shadow-pink-100/10"
            inactive-class="text-slate-500 hover:text-slate-900 hover:bg-slate-100"
          >
            <component :is="iconMap.TrendingUp" class="w-5 h-5" />
            {{ langStore.t('analytics') }}
          </NuxtLink>
          <NuxtLink 
            to="/profile" 
            class="flex items-center gap-3 px-4 py-3 rounded-2xl transition-all duration-200"
            active-class="bg-pink-50 text-[#FF758F] font-bold border-2 border-pink-100/50 shadow-sm shadow-pink-100/10"
            inactive-class="text-slate-500 hover:text-slate-900 hover:bg-slate-100"
          >
            <component :is="iconMap.User" class="w-5 h-5" />
            {{ langStore.t('edit_account') }}
          </NuxtLink>
        </nav>
      </div>
 
      <div class="flex flex-col gap-4">
        <!-- Language Switcher -->
        <button 
          @click="langStore.toggleLocale()" 
          class="flex items-center justify-between px-4 py-2.5 rounded-xl border border-slate-200/80 bg-slate-50/50 hover:bg-slate-100 transition-colors text-xs font-semibold text-slate-700 w-full"
        >
          <span class="flex items-center gap-2">
            <component :is="iconMap.Globe" class="w-4 h-4 text-slate-500" />
            Language / ภาษา
          </span>
          <span class="px-2.5 py-0.5 rounded-lg bg-pink-100 text-pink-600 font-extrabold uppercase text-[10px]">
            {{ langStore.locale }}
          </span>
        </button>
 
        <!-- User Avatar & Profile Card -->
        <div class="flex items-center gap-3 p-3 rounded-xl bg-slate-50 border border-slate-200/60">
          <div class="w-9 h-9 rounded-xl bg-pink-50 border border-pink-100 text-[#FF758F] flex items-center justify-center font-extrabold shadow-sm">
            {{ authStore.user?.displayName.substring(0, 1).toUpperCase() || 'U' }}
          </div>
          <div class="flex flex-col min-w-0">
            <span class="font-semibold truncate text-sm text-slate-800 text-[13px]">{{ authStore.user?.displayName }}</span>
            <span class="text-[11px] text-slate-500 truncate">{{ authStore.user?.email }}</span>
          </div>
        </div>
 
        <button 
          @click="authStore.logout()" 
          class="flex items-center gap-3 px-4 py-3 rounded-xl text-rose-600 hover:text-rose-700 hover:bg-rose-50 transition-all duration-200 w-full font-medium"
        >
          <component :is="iconMap.LogOut" class="w-5 h-5" />
          {{ langStore.t('logout') }}
        </button>
      </div>
    </aside>
 
    <!-- Main Content Area -->
    <main class="flex-1 flex flex-col min-w-0 max-w-5xl mx-auto w-full p-4 md:p-8">
      <slot />
    </main>
 
    <!-- Bottom Navigation Bar (Mobile Only) -->
    <nav class="md:hidden fixed bottom-0 left-0 right-0 h-16 bg-white/95 backdrop-blur-md border-t border-slate-200/80 flex items-center justify-around z-40 px-4">
      <NuxtLink 
        to="/" 
        class="flex flex-col items-center gap-1 text-slate-400"
        active-class="text-[#FF758F] font-extrabold scale-105"
      >
        <component :is="iconMap.LayoutDashboard" class="w-6 h-6" />
        <span class="text-[10px]">{{ langStore.t('dashboard') }}</span>
      </NuxtLink>
 
      <button 
        @click="langStore.toggleLocale()" 
        class="flex flex-col items-center gap-1 text-slate-400"
      >
        <component :is="iconMap.Globe" class="w-6 h-6" />
        <span class="text-[10px] uppercase font-bold text-[#FF758F] bg-pink-50 border border-pink-100 rounded-md px-1.5 py-0.5">{{ langStore.locale }}</span>
      </button>
 
      <!-- Center Floating Button -->
      <button 
        @click="txStore.isAddModalOpen = true" 
        class="w-14 h-14 -mt-6 bg-gradient-to-tr from-[#FF758F] to-[#4EA8DE] rounded-full flex items-center justify-center text-white shadow-lg shadow-pink-300/40 active:scale-95 transition-all duration-200 border-4 border-[#FFFDF9] animate-in zoom-in-95 duration-200 hover:rotate-90"
      >
        <component :is="iconMap.Plus" class="w-7 h-7" />
      </button>
 
      <NuxtLink 
        to="/analytics" 
        class="flex flex-col items-center gap-1 text-slate-400"
        active-class="text-[#FF758F] font-extrabold scale-105"
      >
        <component :is="iconMap.TrendingUp" class="w-6 h-6" />
        <span class="text-[10px]">{{ langStore.t('analytics') }}</span>
      </NuxtLink>
 
      <NuxtLink 
        to="/profile" 
        class="flex flex-col items-center gap-1 text-slate-400"
        active-class="text-[#FF758F] font-extrabold scale-105"
      >
        <component :is="iconMap.User" class="w-6 h-6" />
        <span class="text-[10px]">{{ langStore.t('edit_account') }}</span>
      </NuxtLink>
    </nav>

    <!-- Add Transaction Modal (Overlay Form) -->
    <Teleport to="body">
      <div 
        v-if="txStore.isAddModalOpen" 
        class="fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-50 flex items-end md:items-center justify-center p-0 md:p-4"
        @click.self="closeAddModal()"
      >
        <div class="bg-white border border-slate-200/60 shadow-xl shadow-slate-200/30 w-full md:max-w-lg p-6 flex flex-col gap-5 rounded-t-3xl md:rounded-2xl max-h-[90vh] overflow-y-auto animate-in slide-in-from-bottom md:zoom-in-95 duration-300">
          
          <!-- Modal Header -->
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-bold text-slate-800">{{ langStore.t('add_transaction') }}</h2>
            <button @click="closeAddModal()" class="w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center hover:bg-slate-200 text-slate-500 hover:text-slate-800 transition-colors">
              <component :is="iconMap.X" class="w-5 h-5" />
            </button>
          </div>

          <!-- Error Alert -->
          <div v-if="errorMessage" class="bg-rose-50 border border-rose-200 text-rose-600 p-3 rounded-xl text-sm">
            {{ langStore.t(errorMessage) }}
          </div>

          <!-- Type Switcher -->
          <div class="grid grid-cols-2 p-1 bg-slate-100 rounded-xl border border-slate-200">
            <button 
              type="button"
              @click="type = 'expense'"
              class="py-2.5 rounded-lg text-sm font-semibold transition-all duration-200"
              :class="type === 'expense' ? 'bg-rose-100 text-rose-700 border border-rose-200/50 shadow-sm' : 'text-slate-500 hover:text-slate-800'"
            >
              {{ langStore.t('expense') }}
            </button>
            <button 
              type="button"
              @click="type = 'income'"
              class="py-2.5 rounded-lg text-sm font-semibold transition-all duration-200"
              :class="type === 'income' ? 'bg-emerald-100 text-emerald-700 border border-emerald-200/50 shadow-sm' : 'text-slate-500 hover:text-slate-800'"
            >
              {{ langStore.t('income') }}
            </button>
          </div>

          <!-- Amount Input (Large format) -->
          <div class="flex flex-col gap-2">
            <label class="text-xs text-slate-400 uppercase tracking-wider">{{ langStore.t('amount') }}</label>
            <div class="relative flex items-center">
              <span class="absolute left-4 text-3xl font-bold text-slate-500">฿</span>
              <input 
                v-model="amount" 
                type="number" 
                placeholder="0.00"
                class="w-full bg-slate-100/50 border border-slate-200/80 rounded-xl pl-10 pr-4 py-4 text-3xl font-extrabold focus:outline-none focus:border-blue-500 focus:bg-white text-slate-800 placeholder-slate-400"
                autofocus
              />
            </div>
          </div>

          <!-- Category Selection -->
          <div class="flex flex-col gap-2">
            <label class="text-xs text-slate-400 uppercase tracking-wider">{{ langStore.t('category') }}</label>
            <div class="grid grid-cols-3 sm:grid-cols-4 gap-2">
              <button 
                v-for="cat in filteredCategories" 
                :key="cat.id"
                type="button"
                @click="categoryId = cat.id"
                class="flex flex-col items-center gap-1.5 p-2.5 rounded-xl border transition-all duration-200"
                :class="categoryId === cat.id ? 'bg-blue-50 border-blue-500 text-blue-600 shadow-sm shadow-blue-500/5' : 'bg-slate-100/50 border-slate-200/80 hover:border-slate-300 text-slate-700'"
              >
                <div 
                  class="w-10 h-10 rounded-lg flex items-center justify-center text-white"
                  :style="{ backgroundColor: cat.colorHex + '30', color: cat.colorHex }"
                >
                  <component :is="iconMap[cat.iconName] || iconMap.Coins" class="w-5 h-5" />
                </div>
                <span class="text-[10px] text-slate-600 font-medium truncate w-full text-center">{{ langStore.translateCategory(cat.name) }}</span>
              </button>

              <!-- Custom Category Plus Button -->
              <button 
                type="button"
                @click="showCustomCategoryInput = !showCustomCategoryInput"
                class="flex flex-col items-center gap-1.5 p-2.5 rounded-xl border border-dashed border-slate-300 bg-slate-50/50 hover:bg-slate-100/50 hover:border-slate-400 text-slate-500 transition-all duration-200"
              >
                <div class="w-10 h-10 rounded-lg flex items-center justify-center bg-slate-200 text-slate-600">
                  <component :is="iconMap.Plus" class="w-5 h-5" />
                </div>
                <span class="text-[10px] font-medium truncate w-full text-center">+ {{ langStore.t('add_btn') }}...</span>
              </button>
            </div>
          </div>

          <!-- Inline New Category Input Form -->
          <div v-if="showCustomCategoryInput" class="flex flex-col gap-2 p-3.5 bg-blue-50/40 border border-blue-100 rounded-2xl mt-1 animate-in slide-in-from-top-2 duration-200">
            <label class="text-[11px] font-bold text-blue-600 uppercase tracking-wider">{{ langStore.t('create_custom_cat') }}</label>
            <div class="flex gap-2">
              <input 
                v-model="newCategoryName"
                type="text"
                :placeholder="langStore.t('placeholder_custom_cat')"
                class="flex-1 bg-white border border-slate-200 rounded-xl px-3 py-2 text-xs focus:outline-none focus:border-blue-500 text-slate-800"
                @keyup.enter="handleCreateCustomCategory()"
              />
              <button 
                type="button"
                @click="handleCreateCustomCategory()"
                class="bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold rounded-xl px-4 py-2 flex items-center justify-center transition-colors shrink-0"
                :disabled="isCreatingCategory"
              >
                <span v-if="isCreatingCategory" class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
                <span v-else>{{ langStore.t('add_btn') }}</span>
              </button>
              <button 
                type="button"
                @click="showCustomCategoryInput = false; newCategoryName = ''"
                class="bg-slate-200 hover:bg-slate-300 text-slate-700 text-xs font-bold rounded-xl px-3 py-2 flex items-center justify-center transition-colors shrink-0"
              >
                <component :is="iconMap.X" class="w-4 h-4" />
              </button>
            </div>
          </div>

          <!-- Date Picker -->
          <div class="flex flex-col gap-2">
            <label class="text-xs text-slate-400 uppercase tracking-wider">{{ langStore.t('date') }}</label>
            <input 
              v-model="transactionDate" 
              type="date"
              class="glass-input"
            />
          </div>

          <!-- Description -->
          <div class="flex flex-col gap-2">
            <label class="text-xs text-slate-400 uppercase tracking-wider">{{ langStore.t('notes') }}</label>
            <input 
              v-model="description" 
              type="text" 
              :placeholder="langStore.t('placeholder_notes')"
              class="glass-input"
            />
          </div>

          <!-- Slip Attachment component -->
          <div class="flex flex-col gap-2">
            <label class="text-xs text-slate-400 uppercase tracking-wider">{{ langStore.t('receipt_slip') }}</label>
            
            <div v-if="!slipUrl" class="relative group">
              <input 
                type="file" 
                accept="image/jpeg,image/png,image/webp"
                @change="handleFileUpload"
                class="absolute inset-0 w-full h-full opacity-0 cursor-pointer z-10"
                :disabled="isUploading"
              />
              <div class="border-2 border-dashed border-slate-200 group-hover:border-slate-300 rounded-xl p-4 flex flex-col items-center justify-center gap-2 bg-slate-100/50 transition-colors">
                <component v-if="!isUploading" :is="iconMap.Upload" class="w-6 h-6 text-slate-400" />
                <div v-else class="w-6 h-6 border-2 border-blue-600 border-t-transparent rounded-full animate-spin"></div>
                <span class="text-sm text-slate-500">{{ isUploading ? langStore.t('uploading_slip') : langStore.t('attach_receipt') }}</span>
                <span class="text-[10px] text-slate-400">{{ langStore.t('max_size_slip') }}</span>
              </div>
            </div>

            <!-- Uploaded Image Preview -->
            <div v-else class="flex items-center gap-3 p-3 rounded-xl bg-slate-100/50 border border-slate-200">
              <img :src="slipUrl" class="w-14 h-14 object-cover rounded-lg border border-slate-200" />
              <div class="flex-1 min-w-0">
                <span class="text-xs text-slate-500 block truncate">receipt_slip.jpg</span>
                <span class="text-[10px] text-emerald-600 font-semibold flex items-center gap-1">{{ langStore.t('upload_success') }}</span>
              </div>
              <button 
                type="button" 
                @click="removeSlip()"
                class="w-7 h-7 rounded-lg bg-rose-50 hover:bg-rose-100 text-rose-600 flex items-center justify-center transition-colors"
              >
                <component :is="iconMap.X" class="w-4 h-4" />
              </button>
            </div>
          </div>

          <!-- Action Buttons -->
          <button 
            type="button"
            @click="handleSave()"
            class="btn-primary w-full py-3.5 mt-2 flex items-center justify-center gap-2"
          >
            {{ langStore.t('save_transaction') }}
          </button>

        </div>
      </div>
    </Teleport>

    </div>
    <div v-else class="min-h-screen">
      <slot />
    </div>
  </div>
</template>
