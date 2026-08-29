import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { useAuthStore } from './auth'

export interface Category {
  id: string
  userId: string | null
  name: string
  type: string
  iconName: string
  colorHex: string
}

export interface Transaction {
  id: string
  userId: string
  amount: number
  type: string
  description: string | null
  transactionDate: string
  slipUrl: string | null
  category: Category
}

export interface Summary {
  totalIncome: number
  totalExpense: number
  netBalance: number
  startDate: string
  endDate: string
}

export interface CategoryBreakdown {
  categoryId: string
  categoryName: string
  iconName: string
  colorHex: string
  totalAmount: number
  percentage: number
}

export interface TrendDataPoint {
  label: string
  income: number
  expense: number
}

export interface Trend {
  interval: string
  dataPoints: TrendDataPoint[]
}

export const useTransactionStore = defineStore('transactions', () => {
  const authStore = useAuthStore()

  const categories = ref<Category[]>([])
  const transactions = ref<Transaction[]>([])
  const totalCount = ref(0)
  const page = ref(1)
  const pageSize = ref(20)

  const summary = ref<Summary | null>(null)
  const breakdown = ref<CategoryBreakdown[]>([])
  const trend = ref<Trend | null>(null)

  const isAddModalOpen = ref(false)

  const config = useRuntimeConfig()
  const txUrl = config.public.transactionApiBase
  const mediaUrl = config.public.mediaApiBase

  const headers = computed(() => ({
    Authorization: authStore.accessToken ? `Bearer ${authStore.accessToken}` : ''
  }))

  async function apiFetch<T>(url: string, options: any = {}) {
    try {
      return await $fetch<T>(url, {
        ...options,
        headers: {
          ...headers.value,
          ...options.headers
        }
      })
    } catch (e: any) {
      if (e.status === 401) {
        authStore.logout()
      }
      throw e
    }
  }

  async function fetchCategories() {
    const data = await apiFetch<Category[]>(`${txUrl}/categories`)
    categories.value = data
  }

  async function createCategory(name: string, type: string, iconName: string, colorHex: string) {
    const data = await apiFetch<Category>(`${txUrl}/categories`, {
      method: 'POST',
      body: { name, type, iconName, colorHex }
    })
    categories.value.push(data)
  }

  async function fetchTransactions(filters: {
    startDate?: string
    endDate?: string
    type?: string
    categoryId?: string
    page?: number
    pageSize?: number
  } = {}) {
    const query: Record<string, any> = {
      page: filters.page || page.value,
      pageSize: filters.pageSize || pageSize.value,
      ...filters
    }
    const data = await apiFetch<{ items: Transaction[]; totalCount: number }>(txUrl, {
      query
    })
    transactions.value = data.items
    totalCount.value = data.totalCount
  }

  async function createTransaction(txData: {
    amount: number
    type: string
    categoryId: string
    description?: string | null
    transactionDate: string
    slipUrl?: string | null
  }) {
    const data = await apiFetch<Transaction>(txUrl, {
      method: 'POST',
      body: txData
    })
    transactions.value.unshift(data)
    totalCount.value++
  }

  async function updateTransaction(id: string, txData: {
    amount: number
    type: string
    categoryId: string
    description?: string | null
    transactionDate: string
    slipUrl?: string | null
  }) {
    const data = await apiFetch<Transaction>(`${txUrl}/${id}`, {
      method: 'PUT',
      body: txData
    })
    const idx = transactions.value.findIndex(t => t.id === id)
    if (idx !== -1) {
      transactions.value[idx] = data
    }
  }

  async function deleteTransaction(id: string) {
    await apiFetch(`${txUrl}/${id}`, {
      method: 'DELETE'
    })
    transactions.value = transactions.value.filter(t => t.id !== id)
    totalCount.value--
  }

  async function fetchAnalytics(startDate: string, endDate: string, trendInterval = 'daily') {
    const [summaryData, breakdownData, trendData] = await Promise.all([
      apiFetch<Summary>(`${txUrl}/analytics/summary`, {
        query: { startDate, endDate }
      }),
      apiFetch<CategoryBreakdown[]>(`${txUrl}/analytics/category-breakdown`, {
        query: { startDate, endDate }
      }),
      apiFetch<Trend>(`${txUrl}/analytics/trend`, {
        query: { startDate, endDate, interval: trendInterval }
      })
    ])

    summary.value = summaryData
    breakdown.value = breakdownData
    trend.value = trendData
  }

  async function uploadSlip(file: File) {
    const formData = new FormData()
    formData.append('file', file)

    const data = await apiFetch<{ url: string }>(`${mediaUrl}/upload-slip`, {
      method: 'POST',
      body: formData
    })
    return data.url
  }

  return {
    categories,
    transactions,
    totalCount,
    page,
    pageSize,
    summary,
    breakdown,
    trend,
    isAddModalOpen,
    fetchCategories,
    createCategory,
    fetchTransactions,
    createTransaction,
    updateTransaction,
    deleteTransaction,
    fetchAnalytics,
    uploadSlip
  }
})
