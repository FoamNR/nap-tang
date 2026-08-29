<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useTransactionStore, type Transaction } from '../stores/transactions'
import { useAuthStore } from '../stores/auth'
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
  Plus, 
  Calendar,
  X,
  Trash2,
  Tag,
  FileText,
  DollarSign,
  Home,
  Droplet,
  Zap,
  MoreHorizontal
} from '@lucide/vue'

const txStore = useTransactionStore()
const authStore = useAuthStore()
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
  Plus,
  Calendar,
  X,
  Trash2,
  Tag,
  FileText,
  DollarSign,
  Home,
  Droplet,
  Zap,
  MoreHorizontal
}

// Current month bounds
const getMonthBounds = () => {
  const now = new Date()
  const start = new Date(now.getFullYear(), now.getMonth(), 1)
  const end = new Date(now.getFullYear(), now.getMonth() + 1, 0)
  
  const toISOStringLocalDate = (d: Date) => {
    const offset = d.getTimezoneOffset()
    const local = new Date(d.getTime() - (offset * 60 * 1000))
    return local.toISOString().substring(0, 10)
  }
  
  return {
    start: toISOStringLocalDate(start),
    end: toISOStringLocalDate(end)
  }
}

const bounds = getMonthBounds()

const filterStartDate = ref(bounds.start)
const filterEndDate = ref(bounds.end)

async function applyFilters() {
  await Promise.all([
    txStore.fetchTransactions({
      startDate: filterStartDate.value ? new Date(filterStartDate.value).toISOString() : undefined,
      endDate: filterEndDate.value ? new Date(filterEndDate.value).toISOString() : undefined,
    }),
    txStore.fetchAnalytics(filterStartDate.value, filterEndDate.value)
  ])
}

async function resetFilters() {
  filterStartDate.value = bounds.start
  filterEndDate.value = bounds.end
  await applyFilters()
}

const activeRangeLabel = computed(() => {
  if (filterStartDate.value === bounds.start && filterEndDate.value === bounds.end) {
    return langStore.t('this_month')
  }
  
  const formatDate = (dateStr: string) => {
    if (!dateStr) return '?'
    const d = new Date(dateStr)
    return d.toLocaleDateString(langStore.locale === 'th' ? 'th-TH' : 'en-US', {
      day: 'numeric',
      month: 'short',
      year: '2-digit'
    })
  }
  return `${formatDate(filterStartDate.value)} - ${formatDate(filterEndDate.value)}`
})

onMounted(async () => {
  if (authStore.isAuthenticated) {
    await applyFilters()
  }
})

// Grouped transactions computation
const groupedTransactions = computed(() => {
  const groups: Record<string, Transaction[]> = {}
  txStore.transactions.forEach(tx => {
    const d = new Date(tx.transactionDate)
    const dateStr = d.toLocaleDateString('en-US', {
      day: 'numeric',
      month: 'short',
      year: 'numeric'
    })
    if (!groups[dateStr]) {
      groups[dateStr] = []
    }
    groups[dateStr].push(tx)
  })
  return groups
})

// Transaction Detail Modal State
const selectedTx = ref<Transaction | null>(null)
const isDetailOpen = ref(false)

function showTxDetail(tx: Transaction) {
  selectedTx.value = tx
  isDetailOpen.value = true
}

async function handleDelete(id: string) {
  const isConfirmed = await alertStore.showAlert({
    message: langStore.t('delete_confirm'),
    type: 'confirm'
  })
  
  if (isConfirmed) {
    try {
      await txStore.deleteTransaction(id)
      isDetailOpen.value = false
      // Refresh transactions and analytics
      await applyFilters()
    } catch (e) {
      await alertStore.showAlert({
        message: langStore.t('delete_failed'),
        type: 'error'
      })
    }
  }
}
</script>

<template>
  <div class="flex flex-col gap-6 w-full max-w-4xl mx-auto">
    
    <!-- User welcome header -->
    <div class="flex items-center justify-between">
      <div class="flex flex-col">
        <span class="text-[11px] text-pink-500 font-bold tracking-wider uppercase">{{ langStore.t('overview') }}</span>
        <h1 class="text-2xl font-bold text-slate-800 mt-0.5 cartoon-font">{{ langStore.t('hello') }}, {{ authStore.user?.displayName || 'User' }}</h1>
      </div>
      <button 
        @click="txStore.isAddModalOpen = true"
        class="hidden md:flex items-center gap-2 btn-primary py-2 px-4 text-sm"
      >
        <component :is="iconMap.Plus" class="w-4 h-4" />
        {{ langStore.t('add_transaction') }}
      </button>
    </div>
 
    <!-- Balance Summary Card with blue-sky gradient styling -->
    <div class="relative bg-gradient-to-r from-[#4EA8DE] to-[#FF85A1] rounded-[2rem] p-6 md:p-8 overflow-hidden shadow-lg shadow-pink-200/40 border-2 border-pink-100/30">
      <div class="absolute w-52 h-52 -top-10 -right-10 bg-white/10 rounded-full blur-2xl"></div>
      
      <div class="flex flex-col gap-1 relative z-10">
        <span class="text-xs text-white/90 font-bold tracking-widest uppercase cartoon-font">{{ langStore.t('net_balance') }}</span>
        <h2 class="text-4xl md:text-5xl font-extrabold text-white cartoon-font">
          ฿{{ ((txStore.summary?.totalIncome || 0) - (txStore.summary?.totalExpense || 0)).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}
        </h2>
      </div>
 
      <div class="grid grid-cols-2 gap-4 mt-8 pt-6 border-t border-white/20 relative z-10">
        <!-- Monthly Income -->
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-white/20 text-white flex items-center justify-center">
            <component :is="iconMap.TrendingUp" class="w-5 h-5" />
          </div>
          <div class="flex flex-col">
            <span class="text-[10px] text-white/80 font-bold uppercase tracking-wider">{{ langStore.t('income') }}</span>
            <span class="text-lg font-bold text-white">
              +฿{{ (txStore.summary?.totalIncome || 0).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}
            </span>
          </div>
        </div>
 
        <!-- Monthly Expense -->
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-white/20 text-white flex items-center justify-center rotate-180">
            <component :is="iconMap.TrendingUp" class="w-5 h-5" />
          </div>
          <div class="flex flex-col">
            <span class="text-[10px] text-white/80 font-bold uppercase tracking-wider">{{ langStore.t('expense') }}</span>
            <span class="text-lg font-bold text-white">
              -฿{{ (txStore.summary?.totalExpense || 0).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}
            </span>
          </div>
        </div>
      </div>
    </div>
 
    <!-- Date Filter Card -->
    <div class="glass-card p-4 flex flex-col md:flex-row gap-4 items-end justify-between border border-pink-100/50 shadow-sm shadow-pink-100/5">
      <div class="flex flex-wrap items-center gap-3 w-full md:w-auto flex-1">
        <!-- Start Date -->
        <div class="flex flex-col gap-1.5 flex-1 min-w-[130px]">
          <label class="text-[10px] text-slate-400 font-bold uppercase tracking-wider">{{ langStore.t('start_date') }}</label>
          <input 
            v-model="filterStartDate" 
            type="date"
            class="w-full bg-slate-50 border border-slate-200/80 rounded-xl px-3 py-2 text-xs font-semibold text-slate-700 focus:outline-none focus:border-[#FF758F] focus:bg-white transition-all duration-200 shadow-sm"
          />
        </div>
        <!-- End Date -->
        <div class="flex flex-col gap-1.5 flex-1 min-w-[130px]">
          <label class="text-[10px] text-slate-400 font-bold uppercase tracking-wider">{{ langStore.t('end_date') }}</label>
          <input 
            v-model="filterEndDate" 
            type="date"
            class="w-full bg-slate-50 border border-slate-200/80 rounded-xl px-3 py-2 text-xs font-semibold text-slate-700 focus:outline-none focus:border-[#FF758F] focus:bg-white transition-all duration-200 shadow-sm"
          />
        </div>
      </div>
      
      <!-- Action Buttons -->
      <div class="flex gap-2 w-full md:w-auto justify-end">
        <button 
          @click="resetFilters()"
          class="flex items-center justify-center py-2.5 px-4 rounded-xl border-2 border-slate-200/80 bg-slate-100 hover:bg-slate-200/80 hover:border-slate-300 text-slate-600 text-xs font-extrabold transition-all duration-200 active:scale-95 shadow-sm shadow-slate-100 shrink-0"
        >
          {{ langStore.t('reset_btn') }}
        </button>
        <button 
          @click="applyFilters()"
          class="flex items-center justify-center py-2.5 px-5 rounded-xl text-white bg-gradient-to-tr from-[#FF758F] to-[#FF85A1] hover:from-[#ff5c7d] hover:to-[#ff6d8e] text-xs font-extrabold transition-all duration-200 active:scale-95 shadow-md shadow-pink-100 border-2 border-white shrink-0"
        >
          {{ langStore.t('filter_btn') }}
        </button>
      </div>
    </div>

    <!-- History Header -->
    <div class="flex items-center justify-between mt-2">
      <h3 class="text-lg font-bold text-slate-800 cartoon-font">{{ langStore.t('recent_transactions') }}</h3>
      <span class="text-xs text-pink-500 font-bold bg-pink-50 border border-pink-100 rounded-full px-3 py-1 shadow-sm">{{ activeRangeLabel }}</span>
    </div>
 
    <!-- Empty State -->
    <div 
      v-if="txStore.transactions.length === 0" 
      class="glass-card p-10 flex flex-col items-center justify-center text-center gap-3"
    >
      <div class="w-16 h-16 rounded-2xl bg-pink-50 border-2 border-pink-100 flex items-center justify-center text-pink-400">
        <component :is="iconMap.Calendar" class="w-8 h-8" />
      </div>
      <span class="font-bold text-slate-700 cartoon-font">{{ langStore.t('no_transactions') }}</span>
      <p class="text-xs text-slate-500 max-w-xs">{{ langStore.t('no_transactions_sub') }}</p>
    </div>
 
    <!-- Grouped Transactions Timeline -->
    <div v-else class="flex flex-col gap-6">
      <div v-for="(txList, dateStr) in groupedTransactions" :key="dateStr" class="flex flex-col gap-3">
        <!-- Date divider tag -->
        <span class="text-xs font-bold text-pink-500 bg-[#FFFDF9]/90 backdrop-blur-sm rounded-full px-4.5 py-1 border-2 border-pink-100/60 shadow-sm inline-block w-fit z-10 sticky top-2 cartoon-font">{{ dateStr }}</span>
        
        <!-- Transaction Cards -->
        <div class="flex flex-col gap-2">
          <div 
            v-for="tx in txList" 
            :key="tx.id"
            @click="showTxDetail(tx)"
            class="flex items-center justify-between p-4 rounded-2xl bg-white hover:bg-pink-50/20 border-2 border-pink-50 hover:border-pink-100 transition-all duration-200 cursor-pointer hover:scale-[1.01] shadow-sm shadow-pink-100/10"
          >
            <!-- Category Icon & Info -->
            <div class="flex items-center gap-4 min-w-0">
              <div 
                class="w-11 h-11 rounded-2xl flex items-center justify-center shrink-0 border border-slate-100 shadow-sm"
                :style="{ backgroundColor: tx.category.colorHex + '18', color: tx.category.colorHex }"
              >
                <component :is="iconMap[tx.category.iconName] || iconMap.Coins" class="w-5 h-5" />
              </div>
              <div class="flex flex-col min-w-0">
                <span class="font-bold text-sm text-slate-700 truncate">{{ tx.description || langStore.translateCategory(tx.category.name) }}</span>
                <span class="text-[10px] text-slate-400 font-bold truncate uppercase mt-0.5">{{ langStore.translateCategory(tx.category.name) }}</span>
              </div>
            </div>
 
            <!-- Amount & Attachment Indicator -->
            <div class="flex items-center gap-3 shrink-0">
              <span 
                v-if="tx.slipUrl"
                class="text-[9px] px-2 py-0.5 rounded-lg bg-sky-50 border border-sky-100 text-sky-500 font-bold"
              >
                {{ langStore.t('slip_badge') }}
              </span>
              <span 
                class="font-bold text-sm cartoon-font"
                :class="tx.type === 'income' ? 'text-emerald-500' : 'text-rose-500'"
              >
                {{ tx.type === 'income' ? '+' : '-' }}฿{{ tx.amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}
              </span>
            </div>
 
          </div>
        </div>
 
      </div>
    </div>
 
    <!-- Transaction Detail Drawer/Modal -->
    <Teleport to="body">
      <div 
        v-if="isDetailOpen && selectedTx" 
        class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm z-50 flex items-end md:items-center justify-center p-0 md:p-4"
        @click.self="isDetailOpen = false"
      >
        <div class="bg-white border-2 border-pink-100 shadow-xl shadow-pink-100/40 w-full md:max-w-md p-6 flex flex-col gap-6 rounded-t-[2rem] md:rounded-[2rem] max-h-[90vh] overflow-y-auto animate-in slide-in-from-bottom md:zoom-in-95 duration-200">
          
          <!-- Modal Header -->
          <div class="flex items-center justify-between">
            <h4 class="text-lg font-bold text-slate-800 cartoon-font">{{ langStore.t('transaction_details') }}</h4>
            <button @click="isDetailOpen = false" class="w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center hover:bg-slate-200 text-slate-500 hover:text-slate-800 transition-colors">
              <component :is="iconMap.X" class="w-5 h-5" />
            </button>
          </div>

          <!-- Large Amount display -->
          <div class="flex flex-col items-center gap-1 py-4 border-y border-slate-100">
            <span class="text-[10px] text-slate-500 uppercase tracking-widest font-semibold">{{ langStore.t('amount') }}</span>
            <h3 
              class="text-4xl font-extrabold"
              :class="selectedTx.type === 'income' ? 'text-emerald-600' : 'text-rose-600'"
            >
              {{ selectedTx.type === 'income' ? '+' : '-' }}฿{{ selectedTx.amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}
            </h3>
          </div>

          <!-- Metadata properties -->
          <div class="flex flex-col gap-3">
            <div class="flex justify-between items-center text-sm">
              <span class="text-slate-500 flex items-center gap-2">
                <component :is="iconMap.Tag" class="w-4 h-4" />
                {{ langStore.t('category') }}
              </span>
              <span class="font-semibold text-slate-800 flex items-center gap-1.5">
                <span 
                  class="w-3.5 h-3.5 rounded-full inline-block"
                  :style="{ backgroundColor: selectedTx.category.colorHex }"
                ></span>
                {{ langStore.translateCategory(selectedTx.category.name) }}
              </span>
            </div>

            <div class="flex justify-between items-center text-sm">
              <span class="text-slate-500 flex items-center gap-2">
                <component :is="iconMap.Calendar" class="w-4 h-4" />
                {{ langStore.t('date') }}
              </span>
              <span class="font-semibold text-slate-800">
                {{ new Date(selectedTx.transactionDate).toLocaleDateString('en-US', { day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit' }) }}
              </span>
            </div>

            <div class="flex justify-between items-center text-sm">
              <span class="text-slate-500 flex items-center gap-2">
                <component :is="iconMap.FileText" class="w-4 h-4" />
                {{ langStore.t('notes') }}
              </span>
              <span class="font-semibold text-slate-800 truncate max-w-[200px]">{{ selectedTx.description || '-' }}</span>
            </div>
          </div>

          <!-- Slip Receipt Attachment rendering -->
          <div v-if="selectedTx.slipUrl" class="flex flex-col gap-2">
            <span class="text-xs text-slate-500 font-semibold uppercase tracking-wider">{{ langStore.t('attached_receipt') }}</span>
            <div class="relative group rounded-xl overflow-hidden border border-slate-200 shadow-lg">
              <img :src="selectedTx.slipUrl" class="w-full h-48 object-cover group-hover:scale-105 transition-transform duration-300" />
              <!-- Direct open link -->
              <a 
                :href="selectedTx.slipUrl" 
                target="_blank" 
                class="absolute inset-0 bg-black/40 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-200 text-white font-semibold text-sm"
              >
                {{ langStore.t('view_full_size') }}
              </a>
            </div>
          </div>

          <!-- Actions -->
          <div class="grid grid-cols-1 gap-2 mt-2">
            <button 
              @click="handleDelete(selectedTx.id)" 
              class="btn-secondary py-3 text-rose-600 hover:text-white border-rose-200 hover:bg-rose-600 flex items-center justify-center gap-2"
            >
              <component :is="iconMap.Trash2" class="w-5 h-5" />
              {{ langStore.t('delete_transaction') }}
            </button>
          </div>

        </div>
      </div>
    </Teleport>

  </div>
</template>
