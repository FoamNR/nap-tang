// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  css: ['~/assets/css/main.css'],
  modules: [
    '@nuxtjs/tailwindcss',
    '@pinia/nuxt'
  ],
  runtimeConfig: {
    public: {
      authApiBase: 'http://localhost:8001/api/v1/auth',
      transactionApiBase: 'http://localhost:8002/api/v1/transactions',
      mediaApiBase: 'http://localhost:8003/api/v1/media'
    }
  }
})
