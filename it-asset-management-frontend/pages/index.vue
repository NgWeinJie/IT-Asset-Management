<!-- pages/index.vue -->
<template>
  <div class="home-container">
    <!-- Hero Section -->
    <section class="hero d-flex align-items-center justify-content-center text-center">
      <div class="hero-content fade-in">
        <div class="hero-icon mb-4">
          <i class="fas fa-server display-1 text-gradient"></i>
        </div>
        <h1 class="display-3 fw-bold text-gradient mb-4">IT Asset Management System</h1>
        <p class="lead text-light mb-4">
          Welcome back, <span class="text-primary fw-semibold">{{ user?.firstName || user?.username }}</span>! 
          Track, manage, and optimize your organization's IT assets with ease.
        </p>
        <div class="hero-buttons">
          <NuxtLink to="/assets" class="btn btn-primary btn-lg me-3">
            <i class="fas fa-laptop me-2"></i>
            View Assets
          </NuxtLink>
        </div>
      </div>
    </section>

    <!-- Quick Stats -->
    <section class="container py-5">
      <div class="row g-4 mb-5">
        <div class="col-md-3 col-sm-6">
          <div class="glass-card text-center h-100 stats-card">
            <div class="card-body">
              <div class="stat-icon bg-primary mb-3">
                <i class="fas fa-laptop"></i>
              </div>
              <h3 class="text-light fw-bold mb-1">{{ stats.totalAssets || '0' }}</h3>
              <p class="text-light-emphasis mb-0">Total Assets</p>
            </div>
          </div>
        </div>
        
        <div class="col-md-3 col-sm-6">
          <div class="glass-card text-center h-100 stats-card">
            <div class="card-body">
              <div class="stat-icon bg-success mb-3">
                <i class="fas fa-check-circle"></i>
              </div>
              <h3 class="text-light fw-bold mb-1">{{ stats.availableAssets || '0' }}</h3>
              <p class="text-light-emphasis mb-0">Available</p>
            </div>
          </div>
        </div>
        
        <div class="col-md-3 col-sm-6">
          <div class="glass-card text-center h-100 stats-card">
            <div class="card-body">
              <div class="stat-icon bg-warning mb-3">
                <i class="fas fa-tools"></i>
              </div>
              <h3 class="text-light fw-bold mb-1">{{ stats.inMaintenance || '0' }}</h3>
              <p class="text-light-emphasis mb-0">In Maintenance</p>
            </div>
          </div>
        </div>
        
        <div class="col-md-3 col-sm-6">
          <div class="glass-card text-center h-100 stats-card">
            <div class="card-body">
              <div class="stat-icon bg-info mb-3">
                <i class="fas fa-user-check"></i>
              </div>
              <h3 class="text-light fw-bold mb-1">{{ stats.assignedAssets || '0' }}</h3>
              <p class="text-light-emphasis mb-0">Assigned</p>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Features Section -->
    <section class="container py-5">
      <div class="text-center mb-5">
        <h2 class="text-light fw-bold mb-3">System Features</h2>
        <p class="text-light-emphasis">Comprehensive tools for managing your IT infrastructure</p>
      </div>
      
      <div class="row g-4 justify-content-center">
        <!-- Asset Tracking - Always visible -->
        <div class="col-md-4">
          <div class="glass-card h-100 feature-card">
            <div class="card-body text-center p-4">
              <div class="feature-icon bg-gradient-primary mb-4">
                <i class="fas fa-laptop display-4"></i>
              </div>
              <h5 class="text-light fw-bold mb-3">Asset Tracking</h5>
              <p class="text-light-emphasis mb-4">
                Monitor all hardware and software assets across your organization with real-time status updates.
              </p>
              <NuxtLink to="/assets" class="btn btn-outline-primary">
                <i class="fas fa-arrow-right me-1"></i>
                View Assets
              </NuxtLink>
            </div>
          </div>
        </div>

        <!-- User Management - Only for Admins -->
        <div v-if="isAdmin" class="col-md-4">
          <div class="glass-card h-100 feature-card">
            <div class="card-body text-center p-4">
              <div class="feature-icon bg-gradient-success mb-4">
                <i class="fas fa-users display-4"></i>
              </div>
              <h5 class="text-light fw-bold mb-3">User Management</h5>
              <p class="text-light-emphasis mb-4">
                Assign, manage, and track users linked with IT assets for better accountability.
              </p>
              <NuxtLink to="/users" class="btn btn-outline-success">
                <i class="fas fa-arrow-right me-1"></i>
                Manage Users
              </NuxtLink>
            </div>
          </div>
        </div>

        <!-- Reports & Analytics - Only for Admins -->
        <div v-if="isAdmin" class="col-md-4">
          <div class="glass-card h-100 feature-card">
            <div class="card-body text-center p-4">
              <div class="feature-icon bg-gradient-warning mb-4">
                <i class="fas fa-chart-bar display-4"></i>
              </div>
              <h5 class="text-light fw-bold mb-3">Reports & Analytics</h5>
              <p class="text-light-emphasis mb-4">
                Generate comprehensive reports for asset usage, lifecycle, and compliance tracking.
              </p>
              <NuxtLink to="/reports" class="btn btn-outline-warning">
                <i class="fas fa-arrow-right me-1"></i>
                View Reports
              </NuxtLink>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Recent Activity Section -->
    <section class="container py-5">
      <div class="glass-card">
        <div class="card-body">
          <div class="d-flex justify-content-between align-items-center mb-4">
            <h3 class="text-light fw-bold mb-0">Recent Activity</h3>
            <NuxtLink to="/activity" class="btn btn-sm btn-outline-primary">
              View All Activity
            </NuxtLink>
          </div>
          
          <div v-if="recentActivity.length === 0" class="text-center py-4">
            <i class="fas fa-inbox text-light-emphasis display-4 mb-3"></i>
            <p class="text-light-emphasis mb-0">No recent activity found</p>
          </div>
          
          <div v-else class="activity-list">
            <div 
              v-for="activity in recentActivity" 
              :key="activity.id"
              class="activity-item d-flex align-items-center py-3"
            >
              <div class="activity-icon me-3">
                <i :class="getActivityIcon(activity.type)"></i>
              </div>
              <div class="flex-grow-1">
                <p class="text-light mb-1">{{ activity.description }}</p>
                <small class="text-light-emphasis">{{ formatDate(activity.timestamp) }}</small>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
definePageMeta({
  middleware: 'auth'
})

const user = useCookie('user')
const token = useCookie('token')
const config = useRuntimeConfig()

// Reactive data
const stats = ref({
  totalAssets: 0,
  availableAssets: 0,
  inMaintenance: 0,
  assignedAssets: 0
})

const recentActivity = ref([])

// Computed properties
const isAdmin = computed(() => {
  // Check if user role is admin
  // Adjust this based on your user object structure
  return user.value?.role === 'admin' || user.value?.role === 'Admin'
})

// Methods
const fetchStats = async () => {
  try {
    const response = await $fetch(`${config.public.apiBase}/dashboard/stats`, {
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })
    stats.value = response
  } catch (error) {
    console.error('Error fetching stats:', error)
  }
}

const fetchRecentActivity = async () => {
  try {
    const response = await $fetch(`${config.public.apiBase}/dashboard/recent-activity`, {
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })
    recentActivity.value = response.slice(0, 5) // Show only 5 recent activities
  } catch (error) {
    console.error('Error fetching recent activity:', error)
  }
}

const getActivityIcon = (type) => {
  const icons = {
    'asset_created': 'fas fa-plus-circle text-success',
    'asset_updated': 'fas fa-edit text-warning',
    'asset_deleted': 'fas fa-trash text-danger',
    'asset_assigned': 'fas fa-user-plus text-info',
    'asset_maintenance': 'fas fa-wrench text-warning'
  }
  return icons[type] || 'fas fa-info-circle text-primary'
}

const formatDate = (date) => {
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// Lifecycle
onMounted(() => {
  fetchStats()
  fetchRecentActivity()
})
</script>

<style scoped>
/* Home container with matching background */
.home-container {
  background: linear-gradient(135deg, #0f172a, #3b0764, #0f172a);
  background-attachment: fixed;
  min-height: 100vh;
}

/* Hero section */
.hero {
  min-height: 70vh;
  position: relative;
  overflow: hidden;
}

.hero-content {
  position: relative;
  z-index: 2;
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
}

.hero-icon {
  animation: float 3s ease-in-out infinite;
}

/* Text gradient */
.text-gradient {
  background: linear-gradient(135deg, #fff, #06b6d4, #3b82f6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.text-light-emphasis {
  color: rgba(255, 255, 255, 0.7) !important;
}

/* Glass morphism cards */
.glass-card {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(15px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 15px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  transition: all 0.3s ease;
}

.glass-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.4);
}

/* Stats cards */
.stats-card .stat-icon {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto;
  font-size: 1.5rem;
  color: white;
}

/* Feature cards */
.feature-card {
  transition: all 0.3s ease;
}

.feature-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.5);
}

.feature-icon {
  width: 80px;
  height: 80px;
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto;
  color: white;
}

.bg-gradient-primary {
  background: linear-gradient(135deg, #06b6d4, #3b82f6);
}

.bg-gradient-success {
  background: linear-gradient(135deg, #10b981, #059669);
}

.bg-gradient-warning {
  background: linear-gradient(135deg, #f59e0b, #d97706);
}

/* Buttons */
.btn-primary {
  background: linear-gradient(135deg, #06b6d4, #3b82f6);
  border: none;
  border-radius: 10px;
  font-weight: 600;
  transition: all 0.3s ease;
}

.btn-primary:hover {
  background: linear-gradient(135deg, #0891b2, #2563eb);
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(6, 182, 212, 0.4);
}

.btn-outline-light {
  border-color: rgba(255, 255, 255, 0.5);
  color: rgba(255, 255, 255, 0.9);
  border-radius: 10px;
  font-weight: 600;
}

.btn-outline-light:hover {
  background-color: rgba(255, 255, 255, 0.1);
  border-color: rgba(255, 255, 255, 0.8);
  color: #fff;
  transform: translateY(-2px);
}

.btn-outline-primary {
  border-color: #06b6d4;
  color: #06b6d4;
  border-radius: 8px;
}

.btn-outline-primary:hover {
  background-color: #06b6d4;
  border-color: #06b6d4;
  color: white;
}

.btn-outline-success {
  border-color: #10b981;
  color: #10b981;
  border-radius: 8px;
}

.btn-outline-success:hover {
  background-color: #10b981;
  border-color: #10b981;
  color: white;
}

.btn-outline-warning {
  border-color: #f59e0b;
  color: #f59e0b;
  border-radius: 8px;
}

.btn-outline-warning:hover {
  background-color: #f59e0b;
  border-color: #f59e0b;
  color: white;
}

/* Activity section */
.activity-item {
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.activity-item:last-child {
  border-bottom: none;
}

.activity-icon {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.1);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
}

/* Animations */
.fade-in {
  animation: fadeInUp 0.8s ease-out;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes float {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-10px);
  }
}

/* Responsive design */
@media (max-width: 768px) {
  .hero-content {
    padding: 1rem;
  }
  
  .display-3 {
    font-size: 2.5rem;
  }
  
  .hero-buttons .btn {
    display: block;
    width: 100%;
    margin-bottom: 1rem;
  }
  
  .feature-icon,
  .stat-icon {
    width: 60px;
    height: 60px;
  }
}

/* Center single feature card for users */
@media (min-width: 768px) {
  .row.justify-content-center .col-md-4:only-child {
    max-width: 400px;
  }
}
</style>