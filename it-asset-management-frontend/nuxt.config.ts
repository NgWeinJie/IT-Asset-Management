export default defineNuxtConfig({
  devtools: { enabled: true },
  modules: ['@nuxt/ui'],
  css: [
    "bootstrap/dist/css/bootstrap.min.css",
    "@/assets/css/custom.css",
    '@fortawesome/fontawesome-free/css/all.css'
  ],
  app: {
    head: {
      script: [
        {
          src: 'https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js',
          defer: true
        }
      ]
    }
  },
  runtimeConfig: {
    public: {
      apiBase: 'http://localhost:5000/api'
    }
  },
  ssr: false
})