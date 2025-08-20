<!-- pages/login.vue -->
<template>
  <div class="login-container d-flex align-items-center justify-content-center vh-100">
    <div class="col-12 col-md-6 col-lg-4">
      <div class="glass-card shadow-lg border-0 rounded-4 fade-in">
        <div class="card-body p-5">
          <!-- Logo/Icon -->
          <div class="text-center mb-4">
            <div class="logo-circle mx-auto mb-3">
              <i class="fas fa-laptop text-light" style="font-size: 2rem;"></i>
            </div>
            <h2 class="text-gradient fw-bold mb-2">IT Asset Management</h2>
            <p class="text-light-emphasis mb-0">Sign in to your account</p>
          </div>

          <!-- Form -->
          <form @submit.prevent="login" class="mt-4">
            <div class="mb-3">
              <div class="input-group">
                <span class="input-group-text custom-input-addon">
                  <i class="fas fa-user text-light-emphasis"></i>
                </span>
                <input
                  v-model="loginForm.username"
                  type="text"
                  class="form-control custom-input"
                  placeholder="Username"
                  required
                />
              </div>
            </div>
            
            <div class="mb-4">
              <div class="input-group">
                <span class="input-group-text custom-input-addon">
                  <i class="fas fa-lock text-light-emphasis"></i>
                </span>
                <input
                  v-model="loginForm.password"
                  type="password"
                  class="form-control custom-input"
                  placeholder="Password"
                  required
                />
              </div>
            </div>

            <!-- Error Message -->
            <div v-if="errorMessage" class="alert alert-danger text-center py-2 mb-3 custom-alert">
              <i class="fas fa-exclamation-triangle me-2"></i>
              {{ errorMessage }}
            </div>

            <!-- Button -->
            <div class="d-grid mb-4">
              <button
                type="submit"
                class="btn btn-primary btn-lg custom-login-btn"
                :disabled="isLoading"
              >
                <span v-if="isLoading" class="spinner-border spinner-border-sm me-2"></span>
                <i v-else class="fas fa-sign-in-alt me-2"></i>
                {{ isLoading ? 'Signing in...' : 'Sign in' }}
              </button>
            </div>
          </form>

          <!-- Demo Accounts -->
          <div class="demo-section text-center">
            <div class="demo-header mb-3">
              <hr class="demo-divider">
              <span class="demo-text">Demo Accounts</span>
              <hr class="demo-divider">
            </div>
            
            <div class="demo-accounts">
              <div class="demo-account mb-2">
                <span class="badge bg-primary me-2">Admin</span>
                <code class="demo-credentials">admin / admin123</code>
              </div>
              <div class="demo-account">
                <span class="badge bg-success me-2">User</span>
                <code class="demo-credentials">user1 / user123</code>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Floating particles effect -->
      <div class="particles">
        <div class="particle"></div>
        <div class="particle"></div>
        <div class="particle"></div>
        <div class="particle"></div>
        <div class="particle"></div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

definePageMeta({
  layout: false
})

const config = useRuntimeConfig()
const router = useRouter()

const loginForm = ref({
  username: '',
  password: ''
})

const isLoading = ref(false)
const errorMessage = ref('')

const login = async () => {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const response = await $fetch(`${config.public.apiBase}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: loginForm.value
    })

    const token = useCookie('token', { default: () => null, maxAge: 60 * 60 * 24 })
    const user = useCookie('user', { default: () => null, maxAge: 60 * 60 * 24 })

    token.value = response.token
    user.value = {
      username: response.username,
      role: response.role,
      firstName: response.firstName,
      lastName: response.lastName
    }

    await router.push('/')
  } catch (error) {
    errorMessage.value = 'Invalid credentials. Please try again.'
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
/* Login container with matching background */
.login-container {
  background: linear-gradient(135deg, #0f172a, #3b0764, #0f172a);
  background-attachment: fixed;
  min-height: 100vh;
  position: relative;
  overflow: hidden;
}

/* Glass morphism card */
.glass-card {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  box-shadow: 0 25px 50px rgba(0, 0, 0, 0.5);
  position: relative;
  z-index: 10;
}

/* Logo circle */
.logo-circle {
  width: 80px;
  height: 80px;
  background: linear-gradient(135deg, #06b6d4, #3b82f6);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 10px 30px rgba(6, 182, 212, 0.3);
}

/* Text gradient */
.text-gradient {
  background: linear-gradient(to right, #fff, #06b6d4);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.text-light-emphasis {
  color: rgba(255, 255, 255, 0.7) !important;
}

/* Custom inputs */
.custom-input {
  background: rgba(255, 255, 255, 0.1) !important;
  border: 1px solid rgba(255, 255, 255, 0.3) !important;
  color: #fff !important;
  border-radius: 10px !important;
  padding: 12px 15px;
  font-size: 1rem;
  transition: all 0.3s ease;
}

.custom-input:focus {
  background: rgba(255, 255, 255, 0.15) !important;
  border-color: #06b6d4 !important;
  box-shadow: 0 0 0 0.2rem rgba(6, 182, 212, 0.25) !important;
  color: #fff !important;
  transform: translateY(-2px);
}

.custom-input::placeholder {
  color: rgba(255, 255, 255, 0.6) !important;
}

.custom-input-addon {
  background: rgba(255, 255, 255, 0.1) !important;
  border: 1px solid rgba(255, 255, 255, 0.3) !important;
  border-right: none !important;
  border-radius: 10px 0 0 10px !important;
}

.input-group .custom-input {
  border-left: none !important;
  border-radius: 0 10px 10px 0 !important;
}

/* Custom login button */
.custom-login-btn {
  background: linear-gradient(135deg, #06b6d4, #3b82f6) !important;
  border: none !important;
  border-radius: 10px !important;
  font-weight: 600;
  padding: 12px;
  font-size: 1.1rem;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.custom-login-btn:hover {
  background: linear-gradient(135deg, #0891b2, #2563eb) !important;
  transform: translateY(-2px);
  box-shadow: 0 10px 25px rgba(6, 182, 212, 0.4);
}

.custom-login-btn:active {
  transform: translateY(0);
}

.custom-login-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
  transform: none;
}

/* Custom alert */
.custom-alert {
  background: rgba(220, 53, 69, 0.2) !important;
  color: #f8d7da !important;
  border: 1px solid rgba(220, 53, 69, 0.3) !important;
  border-radius: 10px !important;
  backdrop-filter: blur(10px);
}

/* Demo section */
.demo-section {
  margin-top: 2rem;
  padding-top: 1rem;
}

.demo-header {
  display: flex;
  align-items: center;
  margin-bottom: 1rem;
}

.demo-divider {
  flex: 1;
  height: 1px;
  background: linear-gradient(to right, transparent, rgba(255, 255, 255, 0.3), transparent);
  border: none;
  margin: 0;
}

.demo-text {
  padding: 0 1rem;
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.9rem;
  font-weight: 500;
}

.demo-account {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.demo-credentials {
  background: rgba(255, 255, 255, 0.1);
  color: #06b6d4;
  padding: 0.3rem 0.6rem;
  border-radius: 5px;
  font-size: 0.85rem;
  border: 1px solid rgba(6, 182, 212, 0.3);
}

/* Fade in animation */
.fade-in {
  animation: fadeIn 0.6s ease-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Floating particles */
.particles {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  pointer-events: none;
  z-index: 1;
}

.particle {
  position: absolute;
  background: linear-gradient(45deg, #06b6d4, #3b82f6);
  border-radius: 50%;
  opacity: 0.1;
  animation: float 6s ease-in-out infinite;
}

.particle:nth-child(1) {
  width: 20px;
  height: 20px;
  top: 20%;
  left: 20%;
  animation-delay: 0s;
}

.particle:nth-child(2) {
  width: 15px;
  height: 15px;
  top: 60%;
  left: 80%;
  animation-delay: 2s;
}

.particle:nth-child(3) {
  width: 25px;
  height: 25px;
  top: 80%;
  left: 10%;
  animation-delay: 4s;
}

.particle:nth-child(4) {
  width: 12px;
  height: 12px;
  top: 10%;
  left: 70%;
  animation-delay: 1s;
}

.particle:nth-child(5) {
  width: 18px;
  height: 18px;
  top: 40%;
  left: 90%;
  animation-delay: 3s;
}

@keyframes float {
  0%, 100% {
    transform: translateY(0) rotate(0deg);
    opacity: 0.1;
  }
  50% {
    transform: translateY(-20px) rotate(180deg);
    opacity: 0.3;
  }
}

/* Responsive design */
@media (max-width: 576px) {
  .glass-card {
    margin: 1rem;
  }
  
  .card-body {
    padding: 2rem !important;
  }
  
  .logo-circle {
    width: 60px;
    height: 60px;
  }
  
  .logo-circle i {
    font-size: 1.5rem !important;
  }
}

/* Loading animation */
@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.spinner-border-sm {
  width: 1rem;
  height: 1rem;
}
</style>