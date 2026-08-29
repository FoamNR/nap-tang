import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface User {
  id: string
  email: string
  displayName: string
}

export const useAuthStore = defineStore('auth', () => {
  const accessToken = useCookie<string | null>('accessToken', { default: () => null })
  const user = useCookie<User | null>('user', { default: () => null })
  const config = useRuntimeConfig()
  const authUrl = config.public.authApiBase

  const isAuthenticated = computed(() => !!accessToken.value)

  async function register(email: string, password: string, confirmPassword: string, displayName: string) {
    await $fetch(`${authUrl}/register`, {
      method: 'POST',
      body: { email, password, confirmPassword, displayName }
    })
  }

  async function login(email: string, password: string) {
    const data = await $fetch<{ accessToken: string; user: User }>(`${authUrl}/login`, {
      method: 'POST',
      body: { email, password }
    })
    accessToken.value = data.accessToken
    user.value = data.user
  }

  async function logout() {
    try {
      await $fetch(`${authUrl}/logout`, { method: 'POST' })
    } catch (e) {
      console.error('Logout request failed', e)
    } finally {
      accessToken.value = null
      user.value = null
      navigateTo('/login')
    }
  }

  async function refreshToken() {
    try {
      const data = await $fetch<{ accessToken: string }>(`${authUrl}/refresh`, {
        method: 'POST'
      })
      accessToken.value = data.accessToken
      return data.accessToken
    } catch (e) {
      accessToken.value = null
      user.value = null
      navigateTo('/login')
      throw e
    }
  }

  async function updateProfile(displayName: string, password?: string, confirmPassword?: string) {
    const data = await $fetch<{ accessToken: string; user: User }>(`${authUrl}/profile`, {
      method: 'PUT',
      headers: {
        Authorization: accessToken.value ? `Bearer ${accessToken.value}` : ''
      },
      body: { 
        displayName, 
        newPassword: password || null,
        confirmPassword: confirmPassword || null
      }
    })
    accessToken.value = data.accessToken
    user.value = data.user
  }

  return {
    accessToken,
    user,
    isAuthenticated,
    register,
    login,
    logout,
    refreshToken,
    updateProfile
  }
})
