<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useTransactionStore } from '../stores/transactions'
import { useAuthStore } from '../stores/auth'
import { useLangStore } from '../stores/lang'
import { 
  Utensils, 
  Car, 
  ShoppingBag, 
  Receipt, 
  Tv, 
  Briefcase, 
  TrendingUp, 
  Coins,
  ChevronRight,
  TrendingDown,
  Home,
  Droplet,
  Zap,
  MoreHorizontal
} from '@lucide/vue'

const txStore = useTransactionStore()
const authStore = useAuthStore()
const langStore = useLangStore()

const iconMap = {
  Utensils,
  Car,
  ShoppingBag,
  Receipt,
  Tv,
  Briefcase,
  TrendingUp,
  Coins,
  ChevronRight,
  TrendingDown,
  Home,
  Droplet,
  Zap,
  MoreHorizontal
}

const interval = ref<'daily' | 'weekly' | 'monthly'>('daily')
const breakdownType = ref<'expense' | 'income'>('expense')

// Compute standard ranges
const getRange = (mode: 'daily' | 'weekly' | 'monthly') => {
  const now = new Date()
  let start: Date

  if (mode === 'daily') {
    // Last 30 days
    start = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000)
  } else if (mode === 'weekly') {
    // Last 12 weeks
    start = new Date(now.getTime() - 12 * 7 * 24 * 60 * 60 * 1000)
  } else {
    // Last 12 months
    start = new Date(now.getFullYear() - 1, now.getMonth() + 1, 1)
  }

  const toISOStringLocalDate = (d: Date) => {
    const offset = d.getTimezoneOffset()
    const local = new Date(d.getTime() - (offset * 60 * 1000))
    return local.toISOString().substring(0, 10)
  }

  return {
    start: toISOStringLocalDate(start),
    end: toISOStringLocalDate(now)
  }
}

async function loadAnalytics() {
  const range = getRange(interval.value)
  await txStore.fetchAnalytics(range.start, range.end, interval.value)
}

onMounted(() => {
  if (authStore.isAuthenticated) {
    loadAnalytics()
  }
})

watch(interval, () => {
  loadAnalytics()
})

// SVG chart dimensions
const chartWidth = 500
const chartHeight = 220
const padding = 20

const trendPoints = computed(() => txStore.trend?.dataPoints || [])

const maxVal = computed(() => {
  const points = trendPoints.value
  if (points.length === 0) return 1000
  const max = Math.max(...points.map(p => Math.max(p.income, p.expense)))
  return max > 0 ? max : 1000
})

const getX = (index: number) => {
  const points = trendPoints.value
  if (points.length <= 1) return padding
  return padding + (index * (chartWidth - padding * 2)) / (points.length - 1)
}

const getY = (value: number) => {
  const max = maxVal.value
  return chartHeight - padding - (value / max) * (chartHeight - padding * 2)
}

const incomePath = computed(() => {
  const points = trendPoints.value
  if (points.length === 0) return ''
  return points.map((p, i) => `${i === 0 ? 'M' : 'L'} ${getX(i)} ${getY(p.income)}`).join(' ')
})

const expensePath = computed(() => {
  const points = trendPoints.value
  if (points.length === 0) return ''
  return points.map((p, i) => `${i === 0 ? 'M' : 'L'} ${getX(i)} ${getY(p.expense)}`).join(' ')
})

// Filter breakdown list dynamically by type selected (Income vs Expense)
const filteredBreakdown = computed(() => {
  // If transaction service API returns breakdown for all types, we can filter them locally.
  // Wait, our API GetCategoryBreakdown takes a 'type' query parameter!
  // So let's check: did we query breakdown with type or did we get all categories?
  // Let's filter locally for convenience, or reload.
  // In `transactions.ts`, `fetchAnalytics` loads `analytics/category-breakdown` without type.
  // So it returns all categories (both income and expense)!
  // We can easily filter them in-memory here by category type:
  return txStore.breakdown.filter(item => {
    // Match matching category type
    const matchingCat = txStore.categories.find(c => c.id === item.categoryId)
    return matchingCat ? matchingCat.type === breakdownType.value : true
  })
})

const breakdownTotal = computed(() => {
  return filteredBreakdown.value.reduce((acc, curr) => acc + curr.totalAmount, 0)
})
</script>

<template>
  <div class="flex flex-col gap-6 w-full max-w-4xl mx-auto">
    
    <!-- Title & Navigation -->
    <div class="flex items-center justify-between">
      <div class="flex flex-col">
        <span class="text-[11px] text-pink-500 font-bold tracking-wider uppercase">{{ langStore.t('reports') }}</span>
        <h1 class="text-2xl font-bold text-slate-800 mt-0.5 cartoon-font">{{ langStore.t('financial_analytics') }}</h1>
      </div>
    </div>
 
    <!-- Interval Segmented Picker -->
    <div class="grid grid-cols-3 p-1 bg-slate-100 rounded-2xl border-2 border-pink-100/50 w-full max-w-md">
      <button 
        type="button"
        @click="interval = 'daily'"
        class="py-2 rounded-xl text-xs font-bold transition-all duration-200"
        :class="interval === 'daily' ? 'bg-[#FF758F] text-white shadow-md shadow-pink-300/30' : 'text-slate-500 hover:text-slate-800'"
      >
        {{ langStore.t('daily') }}
      </button>
      <button 
        type="button"
        @click="interval = 'weekly'"
        class="py-2 rounded-xl text-xs font-bold transition-all duration-200"
        :class="interval === 'weekly' ? 'bg-[#FF758F] text-white shadow-md shadow-pink-300/30' : 'text-slate-500 hover:text-slate-800'"
      >
        {{ langStore.t('weekly') }}
      </button>
      <button 
        type="button"
        @click="interval = 'monthly'"
        class="py-2 rounded-xl text-xs font-bold transition-all duration-200"
        :class="interval === 'monthly' ? 'bg-[#FF758F] text-white shadow-md shadow-pink-300/30' : 'text-slate-500 hover:text-slate-800'"
      >
        {{ langStore.t('monthly') }}
      </button>
    </div>
 
    <!-- Trend line Chart (Frosted Card) -->
    <div class="glass-card p-6 flex flex-col gap-4">
      <div class="flex items-center justify-between">
        <h3 class="font-bold text-slate-800 text-sm cartoon-font">{{ langStore.t('trend_title') }}</h3>
        <div class="flex items-center gap-3 text-[10px] uppercase font-bold tracking-wider">
          <span class="flex items-center gap-1.5 text-sky-500">
            <span class="w-2.5 h-2.5 bg-sky-400 rounded-full"></span>
            {{ langStore.t('income') }}
          </span>
          <span class="flex items-center gap-1.5 text-pink-500">
            <span class="w-2.5 h-2.5 bg-pink-400 rounded-full"></span>
            {{ langStore.t('expense') }}
          </span>
        </div>
      </div>
 
      <!-- Custom responsive SVG Line chart -->
      <div class="w-full h-56 relative bg-slate-50/50 rounded-2xl overflow-hidden mt-2 border-2 border-pink-50/60 shadow-inner">
        <svg 
          v-if="trendPoints.length > 0"
          :viewBox="`0 0 ${chartWidth} ${chartHeight}`" 
          class="w-full h-full"
          preserveAspectRatio="none"
        >
          <!-- Grid Lines -->
          <line :x1="padding" :y1="getY(maxVal * 0.75)" :x2="chartWidth - padding" :y2="getY(maxVal * 0.75)" stroke="rgba(255,133,161,0.06)" stroke-width="1.5" />
          <line :x1="padding" :y1="getY(maxVal * 0.5)" :x2="chartWidth - padding" :y2="getY(maxVal * 0.5)" stroke="rgba(255,133,161,0.06)" stroke-width="1.5" />
          <line :x1="padding" :y1="getY(maxVal * 0.25)" :x2="chartWidth - padding" :y2="getY(maxVal * 0.25)" stroke="rgba(255,133,161,0.06)" stroke-width="1.5" />
 
          <!-- Paths -->
          <path 
            :d="incomePath" 
            fill="none" 
            stroke="#4EA8DE" 
            stroke-width="3.5" 
            stroke-linecap="round"
            stroke-linejoin="round"
          />
          <path 
            :d="expensePath" 
            fill="none" 
            stroke="#FF758F" 
            stroke-width="3.5" 
            stroke-linecap="round"
            stroke-linejoin="round"
          />
 
          <!-- Dots for data points -->
          <g v-for="(pt, idx) in trendPoints" :key="idx">
            <circle 
              v-if="pt.income > 0"
              :cx="getX(idx)" 
              :cy="getY(pt.income)" 
              r="4.5" 
              fill="#4EA8DE" 
              stroke="#FFFFFF"
              stroke-width="1.5"
            />
            <circle 
              v-if="pt.expense > 0"
              :cx="getX(idx)" 
              :cy="getY(pt.expense)" 
              r="4.5" 
              fill="#FF758F" 
              stroke="#FFFFFF"
              stroke-width="1.5"
            />
          </g>
        </svg>
 
        <!-- Loading / Empty chart state -->
        <div v-else class="absolute inset-0 flex items-center justify-center text-slate-400 text-xs font-semibold">
          {{ langStore.t('no_trend') }}
        </div>
      </div>
      
      <!-- Timeline labels -->
      <div v-if="trendPoints.length > 0" class="flex justify-between px-2 text-[10px] text-slate-400 font-bold uppercase tracking-wider">
        <span>{{ trendPoints[0].label }}</span>
        <span>{{ trendPoints[Math.floor(trendPoints.length / 2)].label }}</span>
        <span>{{ trendPoints[trendPoints.length - 1].label }}</span>
      </div>
    </div>
 
    <!-- Category Distribution Breakdown -->
    <div class="glass-card p-6 flex flex-col gap-5">
      <div class="flex items-center justify-between">
        <h3 class="font-bold text-slate-800 text-sm cartoon-font">{{ langStore.t('distribution_title') }}</h3>
        
        <!-- Toggle category type (income vs expense) -->
        <div class="flex p-0.5 bg-slate-100 rounded-xl border border-slate-200">
          <button 
            type="button"
            @click="breakdownType = 'expense'"
            class="py-1 px-3.5 rounded-lg text-[10px] uppercase tracking-wider font-bold transition-all duration-200"
            :class="breakdownType === 'expense' ? 'bg-pink-100 text-pink-600 border border-pink-200/50 shadow-sm' : 'text-slate-500 hover:text-slate-800'"
          >
            {{ langStore.t('expenses') }}
          </button>
          <button 
            type="button"
            @click="breakdownType = 'income'"
            class="py-1 px-3.5 rounded-lg text-[10px] uppercase tracking-wider font-bold transition-all duration-200"
            :class="breakdownType === 'income' ? 'bg-sky-100 text-sky-600 border border-sky-200/50 shadow-sm' : 'text-slate-500 hover:text-slate-800'"
          >
            {{ langStore.t('income') }}
          </button>
        </div>
      </div>
 
      <!-- Empty state inside distribution card -->
      <div v-if="filteredBreakdown.length === 0" class="text-center py-6 text-slate-400 text-xs font-semibold">
        {{ langStore.t('no_distribution', { type: langStore.t(breakdownType) }) }}
      </div>

      <!-- Category Progress Indicators -->
      <div v-else class="flex flex-col gap-4">
        <div 
          v-for="item in filteredBreakdown" 
          :key="item.categoryId"
          class="flex flex-col gap-2"
        >
          <!-- Meta headers -->
          <div class="flex items-center justify-between text-xs font-semibold">
            <div class="flex items-center gap-2">
              <div 
                class="w-7 h-7 rounded-lg flex items-center justify-center text-white"
                :style="{ backgroundColor: item.colorHex + '20', color: item.colorHex }"
              >
                <component :is="iconMap[item.iconName] || iconMap.Coins" class="w-4 h-4" />
              </div>
              <span class="text-slate-800">{{ langStore.translateCategory(item.categoryName) }}</span>
              <span class="text-slate-500 text-[10px] font-semibold ml-1">
                {{ Math.round((item.totalAmount / breakdownTotal) * 100) }}%
              </span>
            </div>
            <span class="text-slate-700">
              ฿{{ item.totalAmount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}
            </span>
          </div>

          <!-- Progress Bar wrapper -->
          <div class="w-full h-2.5 bg-slate-100 rounded-full overflow-hidden">
            <div 
              class="h-full rounded-full transition-all duration-500 ease-out"
              :style="{ 
                width: `${(item.totalAmount / breakdownTotal) * 100}%`, 
                backgroundColor: item.colorHex 
              }"
            ></div>
          </div>
        </div>
      </div>

    </div>

  </div>
</template>
