<template>
  <div class="bg-dark text-white min-vh-100 d-flex flex-column">
    <!-- Navigation -->
    <nav class="navbar navbar-expand-lg navbar-dark bg-transparent border-bottom border-secondary shadow-sm">
      <div class="container-fluid">
        <!-- Brand -->
        <NuxtLink to="/" class="navbar-brand d-flex align-items-center">
          <div class="brand-icon bg-gradient p-2 rounded me-2">
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="white" viewBox="0 0 24 24">
              <path stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"/>
            </svg>
          </div>
          <span class="fw-bold fs-4 text-gradient">AssetFlow</span>
        </NuxtLink>
        <!-- Toggle for mobile -->
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#mainNav">
          <span class="navbar-toggler-icon"></span>
        </button>
        <!-- Links -->
        <div class="collapse navbar-collapse" id="mainNav">
          <ul class="navbar-nav ms-auto mb-2 mb-lg-0">
            <!-- Dashboard - Available to all users -->
            <li class="nav-item">
              <NuxtLink to="/" class="nav-link" active-class="active">
                <i class="fas fa-home me-2"></i>
                Dashboard
              </NuxtLink>
            </li>
            <!-- Assets - Available to all users -->
            <li class="nav-item">
              <NuxtLink to="/assets" class="nav-link" active-class="active">
                <i class="fas fa-laptop me-2"></i>
                Assets
              </NuxtLink>
            </li>
            <!-- Reports - Only for admin and manager roles -->
            <li class="nav-item" v-if="canViewReports">
              <NuxtLink to="/reports" class="nav-link" active-class="active">
                <i class="fas fa-chart-bar me-2"></i>
                Reports
              </NuxtLink>
            </li>
            <!-- Users - Only for admin role -->
            <li class="nav-item" v-if="canManageUsers">
              <NuxtLink to="/users" class="nav-link" active-class="active">
                <i class="fas fa-users me-2"></i>
                Users
              </NuxtLink>
            </li>
          </ul>
          <!-- Profile -->
          <div class="dropdown ms-3">
            <button class="btn btn-outline-light rounded-circle profile-btn" data-bs-toggle="dropdown">
              {{ (user?.firstName?.[0] || 'J') + (user?.lastName?.[0] || 'D') }}
            </button>
            <ul class="dropdown-menu dropdown-menu-end bg-dark text-white shadow">
              <li>
                <div class="dropdown-header text-light">
                  <strong>{{ user?.firstName }} {{ user?.lastName }}</strong>
                  <br>
                  <small class="text-muted">{{ user?.email || user?.username }}</small>
                  <br>
                  <small class="text-info">{{ user?.role || 'User' }}</small>
                </div>
              </li>
              <li><hr class="dropdown-divider"></li>
              <li><button class="dropdown-item text-danger" @click="logout">
                <i class="fas fa-sign-out-alt me-2"></i>
                Sign out
              </button></li>
            </ul>
          </div>
        </div>
      </div>
    </nav>
    <!-- Main -->
    <main class="flex-grow-1 container-fluid py-4">
      <slot />
    </main>
    <!-- Footer -->
    <footer class="bg-transparent border-top border-secondary py-3 text-center small text-muted">
      <div class="container">
        <div class="row align-items-center">
          <div class="col-md-6 text-md-start">
            © 2025 AssetFlow Management - Modern IT Asset Management
          </div>
          <div class="col-md-6 text-md-end">
            <span class="me-3">
              <i class="fas fa-shield-alt me-1"></i>
              Secure
            </span>
            <span class="me-3">
              <i class="fas fa-clock me-1"></i>
              Real-time
            </span>
            <span>
              <i class="fas fa-users me-1"></i>
              Multi-user
            </span>
          </div>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup>
const user = useCookie('user')
const token = useCookie('token')
const router = useRouter()

// Define role-based permissions
const userRole = computed(() => {
  return user.value?.role?.toLowerCase() || 'user'
})

// Permission checks
const canViewReports = computed(() => {
  const allowedRoles = ['admin', 'manager', 'supervisor']
  return allowedRoles.includes(userRole.value)
})

const canManageUsers = computed(() => {
  const allowedRoles = ['admin', 'super_admin']
  return allowedRoles.includes(userRole.value)
})

// Alternative: More granular permission system
// const permissions = computed(() => {
//   const rolePermissions = {
//     'user': ['dashboard', 'assets'],
//     'employee': ['dashboard', 'assets'],
//     'supervisor': ['dashboard', 'assets', 'reports'],
//     'manager': ['dashboard', 'assets', 'reports'],
//     'admin': ['dashboard', 'assets', 'reports', 'users'],
//     'super_admin': ['dashboard', 'assets', 'reports', 'users', 'system_settings']
//   }
//   return rolePermissions[userRole.value] || ['dashboard']
// })

// const hasPermission = (permission) => {
//   return permissions.value.includes(permission)
// }

// Navigation guard - redirect if accessing unauthorized page
onMounted(() => {
  const currentPath = router.currentRoute.value.path
  
  if (currentPath === '/reports' && !canViewReports.value) {
    router.push('/')
    showUnauthorizedMessage()
  }
  
  if (currentPath === '/users' && !canManageUsers.value) {
    router.push('/')
    showUnauthorizedMessage()
  }
})

const showUnauthorizedMessage = () => {
  const toast = document.createElement('div')
  toast.className = 'toast-notification'
  toast.innerHTML = `
    <div class="alert alert-warning alert-dismissible fade show" role="alert">
      <i class="fas fa-exclamation-triangle me-2"></i>
      You don't have permission to access this page.
      <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </div>
  `
  document.body.appendChild(toast)
  
  setTimeout(() => {
    if (toast.parentNode) {
      toast.parentNode.removeChild(toast)
    }
  }, 4000)
}

const logout = async () => {
  // Clear cookies
  token.value = null
  user.value = null
  
  // Show success message
  const toast = document.createElement('div')
  toast.className = 'toast-notification'
  toast.innerHTML = `
    <div class="alert alert-success alert-dismissible fade show" role="alert">
      <i class="fas fa-check-circle me-2"></i>
      Logged out successfully!
      <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </div>
  `
  document.body.appendChild(toast)
  
  // Remove toast after delay
  setTimeout(() => {
    if (toast.parentNode) {
      toast.parentNode.removeChild(toast)
    }
  }, 3000)
  
  // Redirect to login
  await new Promise(r => setTimeout(r, 500))
  router.push('/login')
}
</script>

<style scoped>
/* Brand gradient */
.bg-gradient {
  background: linear-gradient(135deg, #06b6d4, #3b82f6);
}

/* Text gradient */
.text-gradient {
  background: linear-gradient(135deg, #fff, #06b6d4, #3b82f6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

/* Navigation styling */
.navbar {
  background: rgba(15, 23, 42, 0.8) !important;
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(255, 255, 255, 0.1) !important;
}

.nav-link {
  color: rgba(255, 255, 255, 0.8) !important;
  font-weight: 500;
  padding: 0.75rem 1rem !important;
  border-radius: 8px;
  transition: all 0.3s ease;
  margin: 0 0.25rem;
}

.nav-link:hover {
  color: #fff !important;
  background-color: rgba(255, 255, 255, 0.1);
  transform: translateY(-1px);
}

.nav-link.active {
  color: #06b6d4 !important;
  background: linear-gradient(135deg, rgba(6, 182, 212, 0.2), rgba(59, 130, 246, 0.2));
  border: 1px solid rgba(6, 182, 212, 0.3);
}

/* Profile button */
.profile-btn {
  width: 45px;
  height: 45px;
  border-radius: 50% !important;
  font-weight: bold;
  font-size: 1rem;
  transition: all 0.3s ease;
  background: linear-gradient(135deg, #06b6d4, #3b82f6);
  border: 2px solid rgba(255, 255, 255, 0.2) !important;
  color: white !important;
}

.profile-btn:hover {
  background: linear-gradient(135deg, #0891b2, #2563eb);
  transform: scale(1.05);
  box-shadow: 0 5px 15px rgba(6, 182, 212, 0.4);
}

/* Dropdown styling */
.dropdown-menu.bg-dark {
  background-color: rgba(30, 30, 30, 0.95) !important;
  backdrop-filter: blur(15px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5);
  min-width: 250px;
}

.dropdown-header {
  padding: 1rem 1rem 0.5rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  margin-bottom: 0.5rem;
}

.dropdown-item {
  color: rgba(255, 255, 255, 0.9) !important;
  padding: 0.75rem 1rem;
  border-radius: 8px;
  margin: 0.25rem 0.5rem;
  transition: all 0.2s ease;
}

.dropdown-item:hover {
  background-color: rgba(255, 255, 255, 0.1) !important;
  color: #fff !important;
  transform: translateX(3px);
}

.dropdown-item.text-danger:hover {
  background-color: rgba(239, 68, 68, 0.2) !important;
  color: #fca5a5 !important;
}

/* Footer styling */
footer {
  background: rgba(15, 23, 42, 0.6) !important;
  backdrop-filter: blur(5px);
}

/* Toast notification */
.toast-notification {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 9999;
  animation: slideInRight 0.3s ease-out;
}

@keyframes slideInRight {
  from {
    opacity: 0;
    transform: translateX(100%);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

/* Mobile responsiveness */
@media (max-width: 991px) {
  .navbar-nav {
    margin-top: 1rem;
    padding-top: 1rem;
    border-top: 1px solid rgba(255, 255, 255, 0.1);
  }
  
  .nav-link {
    margin: 0.25rem 0;
  }
  
  .profile-btn {
    margin-top: 1rem;
  }
}

/* Brand hover effect */
.navbar-brand {
  transition: all 0.3s ease;
}

.navbar-brand:hover {
  transform: scale(1.05);
}

.brand-icon {
  transition: all 0.3s ease;
}

.navbar-brand:hover .brand-icon {
  transform: rotate(5deg);
}
</style>