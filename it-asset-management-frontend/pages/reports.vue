<template>
  <div class="container-fluid p-4">
    <!-- Page Header -->
    <div class="mb-4">
      <div class="d-flex justify-content-between align-items-center">
        <div>
          <h1 class="h2 fw-bold text-light mb-1">
            <i class="fas fa-chart-bar me-2"></i>
            Reports & Analytics
          </h1>
          <p class="text-light-emphasis mb-0">Comprehensive insights into your IT assets</p>
        </div>
        <div class="d-flex gap-2">
          <select 
            v-model="selectedPeriod" 
            @change="fetchReportData"
            class="form-select form-select-sm bg-dark text-light border-secondary"
          >
            <option value="7">Last 7 Days</option>
            <option value="30">Last 30 Days</option>
            <option value="90">Last 90 Days</option>
            <option value="365">Last Year</option>
          </select>
          <button 
            class="btn btn-outline-primary btn-sm" 
            @click="exportReport"
            :disabled="loading"
          >
            <i class="fas fa-download me-1"></i>
            Export
          </button>
        </div>
      </div>
    </div>

    <!-- Summary Cards -->
    <div class="row g-4 mb-4">
      <div class="col-xl-3 col-lg-6">
        <div class="card glass-card">
          <div class="card-body">
            <div class="d-flex align-items-center">
              <div class="flex-shrink-0">
                <div class="stats-icon bg-primary bg-opacity-20 rounded-3">
                  <i class="fas fa-cubes text-primary"></i>
                </div>
              </div>
              <div class="ms-3">
                <div class="text-white-50 small">Total Assets</div>
                <div class="h4 text-white mb-0">{{ stats.totalAssets }}</div>
                <div class="small text-success" v-if="stats.assetsChange > 0">
                  <i class="fas fa-arrow-up me-1"></i>
                  {{ stats.assetsChange }}% from last period
                </div>
                <div class="small text-danger" v-else-if="stats.assetsChange < 0">
                  <i class="fas fa-arrow-down me-1"></i>
                  {{ Math.abs(stats.assetsChange) }}% from last period
                </div>
                <div class="small text-light-emphasis" v-else>
                  No change from last period
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-lg-6">
        <div class="card glass-card">
          <div class="card-body">
            <div class="d-flex align-items-center">
              <div class="flex-shrink-0">
                <div class="stats-icon bg-success bg-opacity-20 rounded-3">
                  <i class="fas fa-user-check text-success"></i>
                </div>
              </div>
              <div class="ms-3">
                <div class="text-white-50 small">Assigned Assets</div>
                <div class="h4 text-white mb-0">{{ stats.assignedAssets }}</div>
                <div class="small text-white-50">
                  {{ stats.totalAssets > 0 ? ((stats.assignedAssets / stats.totalAssets) * 100).toFixed(1) : 0 }}% of total
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-lg-6">
        <div class="card glass-card">
          <div class="card-body">
            <div class="d-flex align-items-center">
              <div class="flex-shrink-0">
                <div class="stats-icon bg-warning bg-opacity-20 rounded-3">
                  <i class="fas fa-wrench text-warning"></i>
                </div>
              </div>
              <div class="ms-3">
                <div class="text-white-50 small">Maintenance Due</div>
                <div class="h4 text-white mb-0">{{ stats.maintenanceDue }}</div>
                <div class="small text-white-50">
                  Need attention
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-lg-6">
        <div class="card glass-card">
          <div class="card-body">
            <div class="d-flex align-items-center">
              <div class="flex-shrink-0">
                <div class="stats-icon bg-info bg-opacity-20 rounded-3">
                  <i class="fas fa-dollar-sign text-info"></i>
                </div>
              </div>
              <div class="ms-3">
                <div class="text-white-50 small">Total Value</div>
                <div class="h4 text-white mb-0">RM {{ stats.totalValue.toLocaleString() }}</div>
                <div class="small text-white-50">
                  Asset portfolio
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Charts Row -->
    <div class="row g-4 mb-4">
      <!-- Asset Distribution Chart -->
      <div class="col-lg-6">
        <div class="card glass-card h-100">
          <div class="card-header bg-transparent border-secondary">
            <h5 class="card-title text-light mb-0">
              <i class="fas fa-chart-pie me-2"></i>
              Asset Distribution by Type
            </h5>
          </div>
          <div class="card-body">
            <div v-if="loading" class="d-flex justify-content-center align-items-center" style="height: 300px;">
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
            </div>
            <div v-else-if="assetDistribution.length === 0" class="d-flex justify-content-center align-items-center" style="height: 300px;">
              <div class="text-center text-light-emphasis">
                <i class="fas fa-chart-pie display-4 mb-3"></i>
                <p>No assets found or all assets have the same category</p>
                <small>Add assets with different categories to see distribution</small>
              </div>
            </div>
            <div v-else>
              <div class="chart-container" style="height: 300px;">
                <canvas ref="pieChart" width="400" height="300"></canvas>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Assignment Status Chart -->
      <div class="col-lg-6">
        <div class="card glass-card h-100">
          <div class="card-header bg-transparent border-secondary">
            <h5 class="card-title text-light mb-0">
              <i class="fas fa-chart-bar me-2"></i>
              Assigned vs Unassigned Assets
            </h5>
          </div>
          <div class="card-body">
            <div v-if="loading" class="d-flex justify-content-center align-items-center" style="height: 300px;">
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
            </div>
            <div v-else-if="stats.totalAssets === 0" class="d-flex justify-content-center align-items-center" style="height: 300px;">
              <div class="text-center text-light-emphasis">
                <i class="fas fa-chart-bar display-4 mb-3"></i>
                <p>No assets found</p>
                <small>Add assets to see assignment status</small>
              </div>
            </div>
            <div v-else>
              <div class="chart-container" style="height: 300px;">
                <canvas ref="barChart" width="400" height="300"></canvas>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Recent Activity Table -->
    <div class="card glass-card">
      <div class="card-header bg-transparent border-secondary d-flex justify-content-between align-items-center">
        <h5 class="card-title text-light mb-0">
          <i class="fas fa-history me-2"></i>
          Recent Asset Activities
        </h5>
        <small class="text-light-emphasis">Last {{ selectedPeriod }} days</small>
      </div>
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table table-dark table-hover mb-0">
            <thead>
              <tr>
                <th scope="col">Date</th>
                <th scope="col">Asset</th>
                <th scope="col">Activity</th>
                <th scope="col">User</th>
                <th scope="col">Status</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="5" class="text-center py-4">
                  <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                  </div>
                </td>
              </tr>
              <tr v-else-if="recentActivities.length === 0">
                <td colspan="5" class="text-center py-4 text-light-emphasis">
                  <i class="fas fa-inbox display-4 mb-3 d-block"></i>
                  No recent activities found for the selected period
                </td>
              </tr>
              <tr v-for="activity in recentActivities" :key="activity.id">
                <td>{{ formatDate(activity.date) }}</td>
                <td>
                  <div class="d-flex align-items-center">
                    <i :class="getAssetIcon(activity.assetType)" class="me-2 text-primary"></i>
                    <div>
                      <div>{{ activity.assetName }}</div>
                      <small class="text-light-emphasis" v-if="activity.assetTag">{{ activity.assetTag }}</small>
                    </div>
                  </div>
                </td>
                <td>{{ activity.activity }}</td>
                <td>{{ activity.user }}</td>
                <td>
                  <span :class="getStatusBadgeClass(activity.status)" class="badge">
                    {{ activity.status }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, nextTick, watch } from 'vue'

// Import Chart.js with proper registration
let Chart
if (process.client) {
  const ChartJS = await import('chart.js/auto')
  Chart = ChartJS.default
}

definePageMeta({
  middleware: 'auth'
})

const config = useRuntimeConfig()
const token = useCookie('token')
const loading = ref(false)

// Reactive data
const selectedPeriod = ref('30')
const stats = ref({
  totalAssets: 0,
  assignedAssets: 0,
  maintenanceDue: 0,
  totalValue: 0,
  assetsChange: 0
})

const recentActivities = ref([])
const assetDistribution = ref([])
const assetStatusData = ref([])
const pieChart = ref(null)
const barChart = ref(null)

let pieChartInstance = null
let barChartInstance = null

// Methods
const fetchAssetData = async () => {
  try {
    const response = await $fetch(`${config.public.apiBase}/assets?pageSize=1000`, {
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })
    
    const assets = response.assets || response || []
    console.log('Fetched assets:', assets.length, assets)
    return assets
    
  } catch (error) {
    console.error('Error fetching assets:', error)
    return []
  }
}

const fetchUsers = async () => {
  try {
    const response = await $fetch(`${config.public.apiBase}/users`, {
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })
    return response.users || response || []
  } catch (error) {
    console.error('Error fetching users:', error)
    return []
  }
}

const calculateAssetDistribution = (assets) => {
  console.log('Calculating distribution for assets:', assets)
  
  const distribution = {}
  assets.forEach(asset => {
    const category = asset.category || 'Unknown'
    distribution[category] = (distribution[category] || 0) + 1
  })
  
  console.log('Distribution calculated:', distribution)
  
  return Object.entries(distribution).map(([category, count]) => ({
    category,
    count
  }))
}

const calculateAssetStatusData = (assets) => {
  const statusCounts = {}
  assets.forEach(asset => {
    const status = asset.status || 'Unknown'
    statusCounts[status] = (statusCounts[status] || 0) + 1
  })
  
  return Object.entries(statusCounts).map(([status, count]) => ({
    status,
    count
  }))
}

const calculateStats = (assets, previousAssets = []) => {
  const totalAssets = assets.length
  const assignedAssets = assets.filter(asset => asset.status === 'Assigned').length
  
  const maintenanceDue = assets.filter(asset => {
    if (asset.status === 'Maintenance') return true
    if (asset.warrantyExpiry) {
      const warrantyDate = new Date(asset.warrantyExpiry)
      const thirtyDaysFromNow = new Date()
      thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30)
      return warrantyDate <= thirtyDaysFromNow && warrantyDate > new Date()
    }
    return false
  }).length
  
  const totalValue = assets.reduce((sum, asset) => {
    return sum + (parseFloat(asset.purchasePrice) || 0)
  }, 0)
  
  const previousTotal = previousAssets.length
  const assetsChange = previousTotal > 0 
    ? (((totalAssets - previousTotal) / previousTotal) * 100).toFixed(1)
    : 0
  
  return {
    totalAssets,
    assignedAssets,
    maintenanceDue,
    totalValue,
    assetsChange: parseFloat(assetsChange)
  }
}

const generateRecentActivities = async (assets) => {
  const activities = []
  const users = await fetchUsers()
  
  const getUserName = (username) => {
    if (!username) return 'System'
    const user = users.find(u => u.username === username)
    return user ? `${user.firstName} ${user.lastName}` : username
  }
  
  assets.forEach(asset => {
    if (asset.createdAt) {
      const createdDate = new Date(asset.createdAt)
      const daysAgo = Math.floor((new Date() - createdDate) / (1000 * 60 * 60 * 24))
      
      if (daysAgo <= parseInt(selectedPeriod.value)) {
        activities.push({
          id: `create-${asset.id}`,
          date: asset.createdAt,
          assetName: asset.name,
          assetTag: asset.assetTag,
          assetType: asset.category,
          activity: 'Asset Created',
          user: 'System',
          status: 'Completed'
        })
      }
    }
    
    if (asset.assignedTo && asset.assignedAt) {
      const assignedDate = new Date(asset.assignedAt)
      const daysAgo = Math.floor((new Date() - assignedDate) / (1000 * 60 * 60 * 24))
      
      if (daysAgo <= parseInt(selectedPeriod.value)) {
        activities.push({
          id: `assign-${asset.id}`,
          date: asset.assignedAt,
          assetName: asset.name,
          assetTag: asset.assetTag,
          assetType: asset.category,
          activity: 'Asset Assigned',
          user: getUserName(asset.assignedTo),
          status: 'Completed'
        })
      }
    }
    
    if (asset.status === 'Maintenance') {
      activities.push({
        id: `maintenance-${asset.id}`,
        date: asset.updatedAt || new Date().toISOString(),
        assetName: asset.name,
        assetTag: asset.assetTag,
        assetType: asset.category,
        activity: 'Maintenance Required',
        user: 'System',
        status: 'Pending'
      })
    }
    
    if (asset.warrantyExpiry) {
      const warrantyDate = new Date(asset.warrantyExpiry)
      const thirtyDaysFromNow = new Date()
      thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30)
      
      if (warrantyDate <= thirtyDaysFromNow && warrantyDate > new Date()) {
        activities.push({
          id: `warranty-${asset.id}`,
          date: new Date().toISOString(),
          assetName: asset.name,
          assetTag: asset.assetTag,
          assetType: asset.category,
          activity: 'Warranty Expiring Soon',
          user: 'System',
          status: 'Warning'
        })
      }
    }
  })
  
  return activities
    .sort((a, b) => new Date(b.date) - new Date(a.date))
    .slice(0, 20)
}

const fetchReportData = async () => {
  loading.value = true
  try {
    const assets = await fetchAssetData()
    const previousAssets = []
    
    stats.value = calculateStats(assets, previousAssets)
    assetDistribution.value = calculateAssetDistribution(assets)
    assetStatusData.value = calculateAssetStatusData(assets)
    recentActivities.value = await generateRecentActivities(assets)
    
    // Wait for DOM to update before creating charts
    await nextTick()
    setTimeout(() => {
      updateCharts()
    }, 100)
    
  } catch (error) {
    console.error('Error fetching report data:', error)
    stats.value = {
      totalAssets: 0,
      assignedAssets: 0,
      maintenanceDue: 0,
      totalValue: 0,
      assetsChange: 0
    }
    assetDistribution.value = []
    assetStatusData.value = []
    recentActivities.value = []
  } finally {
    loading.value = false
  }
}

const exportReport = async () => {
  try {
    loading.value = true
    
    const response = await $fetch(`${config.public.apiBase}/assets/export`, {
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })
    
    const blob = new Blob([response], { type: 'text/csv' })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `assets_report_${new Date().toISOString().split('T')[0]}.csv`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
    
  } catch (error) {
    console.error('Error exporting report:', error)
    
    try {
      const assets = await fetchAssetData()
      const headers = ['Asset Tag', 'Name', 'Category', 'Status', 'Assigned To', 'Purchase Price', 'Created Date', 'Location']
      const csvContent = [
        headers.join(','),
        ...assets.map(asset => [
          asset.assetTag || '',
          asset.name || '',
          asset.category || '',
          asset.status || '',
          asset.assignedTo || '',
          asset.purchasePrice || '',
          asset.createdAt ? new Date(asset.createdAt).toLocaleDateString() : '',
          asset.location || ''
        ].map(field => `"${field}"`).join(','))
      ].join('\n')
      
      const blob = new Blob([csvContent], { type: 'text/csv' })
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `assets_report_${new Date().toISOString().split('T')[0]}.csv`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)
      
    } catch (fallbackError) {
      console.error('Error generating fallback CSV:', fallbackError)
      alert('Error exporting report. Please try again.')
    }
  } finally {
    loading.value = false
  }
}

const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const getAssetIcon = (type) => {
  const icons = {
    'Laptop': 'fas fa-laptop',
    'Desktop': 'fas fa-desktop',
    'Monitor': 'fas fa-tv',
    'Printer': 'fas fa-print',
    'Server': 'fas fa-server',
    'Network Equipment': 'fas fa-network-wired',
    'Mobile Device': 'fas fa-mobile-alt',
    'Tablet': 'fas fa-tablet-alt',
    'Accessories': 'fas fa-plug',
    'Software': 'fas fa-code'
  }
  return icons[type] || 'fas fa-cube'
}

const getStatusBadgeClass = (status) => {
  const classes = {
    'Available': 'bg-success',
    'Assigned': 'bg-primary',
    'Maintenance': 'bg-warning text-dark',
    'Retired': 'bg-danger',
    'Completed': 'bg-success',
    'Pending': 'bg-warning text-dark',
    'Warning': 'bg-danger'
  }
  return classes[status] || 'bg-secondary'
}

const updateCharts = () => {
  if (!process.client || !Chart) return

  console.log('Updating charts...')
  console.log('Asset distribution:', assetDistribution.value)
  console.log('Stats:', stats.value)

  // Destroy existing charts
  if (pieChartInstance) {
    pieChartInstance.destroy()
    pieChartInstance = null
  }
  if (barChartInstance) {
    barChartInstance.destroy()
    barChartInstance = null
  }

  // Create pie chart
  if (pieChart.value && assetDistribution.value.length > 0) {
    console.log('Creating pie chart...')
    const ctx = pieChart.value.getContext('2d')
    const colors = [
      '#06b6d4', '#3b82f6', '#10b981', '#f59e0b', '#ef4444',
      '#8b5cf6', '#f97316', '#06d6a0', '#ffd23f', '#ff6b9d'
    ]
    
    try {
      pieChartInstance = new Chart(ctx, {
        type: 'pie',
        data: {
          labels: assetDistribution.value.map(item => item.category),
          datasets: [{
            data: assetDistribution.value.map(item => item.count),
            backgroundColor: colors.slice(0, assetDistribution.value.length),
            borderWidth: 2,
            borderColor: '#1e293b'
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              position: 'right',
              labels: {
                color: '#e2e8f0',
                usePointStyle: true,
                padding: 15,
                font: {
                  size: 12
                }
              }
            },
            tooltip: {
              backgroundColor: 'rgba(30, 41, 59, 0.9)',
              titleColor: '#e2e8f0',
              bodyColor: '#e2e8f0',
              borderColor: '#475569',
              borderWidth: 1,
              callbacks: {
                label: function(context) {
                  const total = context.dataset.data.reduce((a, b) => a + b, 0)
                  const percentage = ((context.parsed / total) * 100).toFixed(1)
                  return `${context.label}: ${context.parsed} (${percentage}%)`
                }
              }
            }
          }
        }
      })
      console.log('Pie chart created successfully')
    } catch (error) {
      console.error('Error creating pie chart:', error)
    }
  } else {
    console.log('Pie chart not created - canvas or data missing')
  }

  // Create bar chart
  if (barChart.value && stats.value.totalAssets > 0) {
    console.log('Creating bar chart...')
    const ctx = barChart.value.getContext('2d')
    const assigned = stats.value.assignedAssets
    const unassigned = stats.value.totalAssets - stats.value.assignedAssets
    
    try {
      barChartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: ['Assigned', 'Unassigned'],
          datasets: [{
            label: 'Assets',
            data: [assigned, unassigned],
            backgroundColor: ['#10b981', '#f59e0b'],
            borderWidth: 0,
            borderRadius: 8,
            borderSkipped: false
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: {
            y: {
              beginAtZero: true,
              ticks: {
                color: '#e2e8f0',
                stepSize: 1,
                font: {
                  size: 12
                }
              },
              grid: {
                color: 'rgba(255, 255, 255, 0.1)'
              }
            },
            x: {
              ticks: {
                color: '#e2e8f0',
                font: {
                  size: 12
                }
              },
              grid: {
                color: 'rgba(255, 255, 255, 0.1)'
              }
            }
          },
          plugins: {
            legend: {
              display: false
            },
            tooltip: {
              backgroundColor: 'rgba(30, 41, 59, 0.9)',
              titleColor: '#e2e8f0',
              bodyColor: '#e2e8f0',
              borderColor: '#475569',
              borderWidth: 1
            }
          }
        }
      })
      console.log('Bar chart created successfully')
    } catch (error) {
      console.error('Error creating bar chart:', error)
    }
  } else {
    console.log('Bar chart not created - canvas or data missing')
  }
}

// Watch for period changes
watch(selectedPeriod, () => {
  fetchReportData()
})

// Lifecycle
onMounted(() => {
  if (process.client) {
    fetchReportData()
  }
})
</script>

<style scoped>
.glass-card {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(15px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 15px;
  transition: all 0.3s ease;
}

.glass-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.4);
}

.stats-icon {
  width: 60px;
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 8px;
}

.table-dark {
  --bs-table-bg: transparent;
}

.table-hover tbody tr:hover {
  background-color: rgba(255, 255, 255, 0.05);
}

.chart-container {
  position: relative;
  width: 100%;
  height: 300px;
}

.chart-container canvas {
  width: 100% !important;
  height: 100% !important;
}

.card-header {
  background: rgba(255, 255, 255, 0.05) !important;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1) !important;
}

.text-light-emphasis {
  color: rgba(255, 255, 255, 0.7) !important;
}

.btn-outline-primary {
  border-color: #06b6d4 !important;
  color: #06b6d4 !important;
}

.btn-outline-primary:hover {
  background-color: #06b6d4 !important;
  border-color: #06b6d4 !important;
  color: white !important;
}
</style>