<template>
  <div class="container-fluid p-4">
    <!-- Header with Back Button -->
    <div class="mb-4">
      <div class="d-flex align-items-center mb-3">
        <NuxtLink
          to="/assets"
          class="btn btn-outline-light me-3 d-flex align-items-center"
        >
          <i class="fas fa-arrow-left me-2"></i>
          Back to Assets
        </NuxtLink>
      </div>
      <div class="d-flex justify-content-between align-items-center">
        <div>
          <h1 class="h2 fw-bold text-light mb-1">Asset Details</h1>
          <p class="text-light-emphasis mb-0">View and manage asset information</p>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="card glass-card">
      <div class="card-body text-center py-5">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <p class="mt-3 text-light-emphasis">Loading asset details...</p>
      </div>
    </div>

    <!-- Asset Not Found -->
    <div v-else-if="!asset" class="card glass-card">
      <div class="card-body text-center py-5">
        <i class="fas fa-exclamation-triangle text-warning display-4 mb-3"></i>
        <h3 class="h4 text-light mb-2">Asset Not Found</h3>
        <p class="text-light-emphasis mb-4">The requested asset could not be found.</p>
        <NuxtLink to="/assets" class="btn btn-primary">
          <i class="fas fa-arrow-left me-2"></i>
          Back to Assets
        </NuxtLink>
      </div>
    </div>

    <!-- Asset Details -->
    <div v-else>
      <!-- Asset Header Card -->
      <div class="card mb-4 glass-card">
        <div class="card-body">
          <div class="row align-items-center">
            <div class="col-md-8">
              <div class="d-flex align-items-center">
                <div class="asset-icon-large me-3">
                  <i :class="getAssetIcon(asset.category)" class="text-primary"></i>
                </div>
                <div>
                  <h2 class="h3 fw-bold text-light mb-1">{{ asset.name }}</h2>
                  <div class="text-light-emphasis mb-2">
                    <i class="fas fa-tag me-1"></i>
                    {{ asset.assetTag }}
                  </div>
                  <div v-if="asset.brand || asset.model" class="text-light-emphasis">
                    <i class="fas fa-info-circle me-1"></i>
                    {{ asset.brand }} {{ asset.model }}
                  </div>
                </div>
              </div>
            </div>
            <div class="col-md-4 text-md-end">
              <div class="mb-3">
                <span
                  :class="getStatusClass(asset.status)"
                  class="badge position-relative fs-6 px-3 py-2"
                >
                  {{ asset.status }}
                  <span v-if="asset.status === 'Available'" class="pulse-dot"></span>
                </span>
              </div>
              <button
                v-if="user?.role === 'Admin'"
                @click="editAsset"
                class="btn btn-warning"
              >
                <i class="fas fa-edit me-2"></i>
                Edit Asset
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Asset Information Cards -->
      <div class="row g-4">
        <!-- Basic Information -->
        <div class="col-lg-4">
          <div class="card glass-card h-100">
            <div class="card-header border-bottom border-secondary bg-transparent">
              <h5 class="card-title text-light mb-0 d-flex align-items-center">
                <i class="fas fa-info-circle me-2 text-primary"></i>
                Basic Information
              </h5>
            </div>
            <div class="card-body">
              <div class="row g-3">
                <div class="col-12">
                  <div class="info-item">
                    <dt class="text-light-emphasis small fw-medium">Category</dt>
                    <dd class="text-light mt-1 mb-0">
                      <span class="badge bg-secondary">{{ asset.category }}</span>
                    </dd>
                  </div>
                </div>
                <div class="col-12">
                  <div class="info-item">
                    <dt class="text-light-emphasis small fw-medium">Brand</dt>
                    <dd class="text-light mt-1 mb-0">{{ asset.brand || '-' }}</dd>
                  </div>
                </div>
                <div class="col-12">
                  <div class="info-item">
                    <dt class="text-light-emphasis small fw-medium">Model</dt>
                    <dd class="text-light mt-1 mb-0">{{ asset.model || '-' }}</dd>
                  </div>
                </div>
                <div class="col-12">
                  <div class="info-item">
                    <dt class="text-light-emphasis small fw-medium">Serial Number</dt>
                    <dd class="text-light mt-1 mb-0">
                      <code class="bg-dark text-light px-2 py-1 rounded">{{ asset.serialNumber || '-' }}</code>
                    </dd>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Assignment Information -->
        <div class="col-lg-4">
          <div class="card glass-card h-100">
            <div class="card-header border-bottom border-secondary bg-transparent">
              <h5 class="card-title text-light mb-0 d-flex align-items-center">
                <i class="fas fa-user-cog me-2 text-info"></i>
                Assignment & Location
              </h5>
            </div>
            <div class="card-body">
              <div class="row g-3">
                <div class="col-12">
                  <div class="info-item">
                    <dt class="text-light-emphasis small fw-medium">Status</dt>
                    <dd class="mt-1 mb-0">
                      <span
                        :class="getStatusClass(asset.status)"
                        class="badge position-relative"
                      >
                        {{ asset.status }}
                        <span v-if="asset.status === 'Available'" class="pulse-dot"></span>
                      </span>
                    </dd>
                  </div>
                </div>
                <div class="col-12">
                  <div class="info-item">
                    <dt class="text-light-emphasis small fw-medium">Assigned To</dt>
                    <dd class="text-light mt-1 mb-0">
                      <div v-if="asset.assignedTo" class="d-flex align-items-center">
                        <i class="fas fa-user me-2 text-primary"></i>
                        <span>{{ asset.assignedTo }}</span>
                      </div>
                      <span v-else class="text-light-emphasis">Unassigned</span>
                    </dd>
                  </div>
                </div>
                <div class="col-12">
                  <div class="info-item">
                    <dt class="text-light-emphasis small fw-medium">Location</dt>
                    <dd class="text-light mt-1 mb-0">
                      <div v-if="asset.location" class="d-flex align-items-center">
                        <i class="fas fa-map-marker-alt me-2 text-success"></i>
                        <span>{{ asset.location }}</span>
                      </div>
                      <span v-else class="text-light-emphasis">Not specified</span>
                    </dd>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Financial Information -->
        <div class="col-lg-4">
          <div class="card glass-card h-100">
            <div class="card-header border-bottom border-secondary bg-transparent">
              <h5 class="card-title text-light mb-0 d-flex align-items-center">
                <i class="fas fa-dollar-sign me-2 text-success"></i>
                Financial Information
              </h5>
            </div>
            <div class="card-body">
              <div class="row g-3">
                <div class="col-12">
                  <div class="info-item">
                    <dt class="text-light-emphasis small fw-medium">Purchase Price</dt>
                    <dd class="text-light mt-1 mb-0">
                      <span v-if="asset.purchasePrice" class="fw-medium">
                        RM {{ asset.purchasePrice.toFixed(2) }}
                      </span>
                      <span v-else class="text-light-emphasis">-</span>
                    </dd>
                  </div>
                </div>
                <div class="col-12">
                  <div class="info-item">
                    <dt class="text-light-emphasis small fw-medium">Purchase Date</dt>
                    <dd class="text-light mt-1 mb-0">
                      {{ asset.purchaseDate ? formatDate(asset.purchaseDate) : '-' }}
                    </dd>
                  </div>
                </div>
                <div class="col-12">
                  <div class="info-item">
                    <dt class="text-light-emphasis small fw-medium">Warranty Expiry</dt>
                    <dd class="mt-1 mb-0">
                      <span 
                        v-if="asset.warrantyExpiry"
                        :class="getWarrantyStatusColor(asset.warrantyExpiry)"
                        class="fw-medium"
                      >
                        {{ formatDate(asset.warrantyExpiry) }}
                        <i 
                          v-if="isWarrantyExpired(asset.warrantyExpiry)"
                          class="fas fa-exclamation-triangle text-danger ms-1"
                        ></i>
                      </span>
                      <span v-else class="text-light-emphasis">-</span>
                    </dd>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Notes Section -->
      <div v-if="asset.notes" class="row g-4 mt-2">
        <div class="col-12">
          <div class="card glass-card">
            <div class="card-header border-bottom border-secondary bg-transparent">
              <h5 class="card-title text-light mb-0 d-flex align-items-center">
                <i class="fas fa-sticky-note me-2 text-warning"></i>
                Notes
              </h5>
            </div>
            <div class="card-body">
              <div class="notes-content p-3 rounded" style="background: rgba(255,255,255,0.05);">
                <p class="text-light mb-0" style="white-space: pre-wrap;">{{ asset.notes }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- System Information -->
      <div class="row g-4 mt-2">
        <div class="col-12">
          <div class="card glass-card">
            <div class="card-header border-bottom border-secondary bg-transparent">
              <h5 class="card-title text-light mb-0 d-flex align-items-center">
                <i class="fas fa-cogs me-2 text-info"></i>
                System Information
              </h5>
            </div>
            <div class="card-body">
              <div class="row g-4">
                <div class="col-md-3">
                  <div class="info-item">
                    <dt class="text-light-emphasis small fw-medium">Created By</dt>
                    <dd class="text-light mt-1 mb-0">
                      <i class="fas fa-user me-1"></i>
                      {{ asset.createdBy }}
                    </dd>
                  </div>
                </div>
                <div class="col-md-3">
                  <div class="info-item">
                    <dt class="text-light-emphasis small fw-medium">Created At</dt>
                    <dd class="text-light mt-1 mb-0">
                      <i class="fas fa-calendar me-1"></i>
                      {{ formatDateTime(asset.createdAt) }}
                    </dd>
                  </div>
                </div>
                <div v-if="asset.updatedBy" class="col-md-3">
                  <div class="info-item">
                    <dt class="text-light-emphasis small fw-medium">Last Updated By</dt>
                    <dd class="text-light mt-1 mb-0">
                      <i class="fas fa-user-edit me-1"></i>
                      {{ asset.updatedBy }}
                    </dd>
                  </div>
                </div>
                <div v-if="asset.updatedAt" class="col-md-3">
                  <div class="info-item">
                    <dt class="text-light-emphasis small fw-medium">Last Updated At</dt>
                    <dd class="text-light mt-1 mb-0">
                      <i class="fas fa-clock me-1"></i>
                      {{ formatDateTime(asset.updatedAt) }}
                    </dd>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Edit Asset Modal -->
    <div
      v-if="showEditModal"
      class="modal fade show d-block"
      tabindex="-1"
      style="background-color: rgba(0,0,0,0.7);"
      @click.self="closeModal"
    >
      <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
        <div class="modal-content glass-modal">
          <div class="modal-header border-bottom border-secondary sticky-top">
            <h5 class="modal-title text-light d-flex align-items-center">
              <i class="fas fa-edit me-2"></i>
              Edit Asset
            </h5>
            <button type="button" class="btn-close btn-close-white" @click="closeModal"></button>
          </div>

          <form @submit.prevent="updateAsset">
            <div class="modal-body scrollable-modal-body">
              <div class="container-fluid p-0">
                <div class="row g-3">
                  <!-- Basic Information -->
                  <div class="col-12">
                    <h6 class="text-light fw-bold mb-3">
                      <i class="fas fa-info-circle me-2"></i>
                      Basic Information
                    </h6>
                  </div>
                  
                  <div class="col-md-6">
                    <label class="form-label text-light">Name *</label>
                    <input
                      v-model="assetForm.name"
                      type="text"
                      required
                      class="form-control custom-input"
                      placeholder="Enter asset name"
                    />
                  </div>
                  
                  <div class="col-md-6">
                    <label class="form-label text-light">Asset Tag *</label>
                    <input
                      v-model="assetForm.assetTag"
                      type="text"
                      required
                      class="form-control custom-input"
                      placeholder="Enter unique asset tag"
                    />
                  </div>
                  
                  <div class="col-md-6">
                    <label class="form-label text-light">Category *</label>
                    <select
                      v-model="assetForm.category"
                      required
                      class="form-select custom-input"
                    >
                      <option value="">Select Category</option>
                      <option value="Laptop">Laptop</option>
                      <option value="Desktop">Desktop</option>
                      <option value="Monitor">Monitor</option>
                      <option value="Printer">Printer</option>
                      <option value="Server">Server</option>
                      <option value="Network Equipment">Network Equipment</option>
                      <option value="Mobile Device">Mobile Device</option>
                      <option value="Tablet">Tablet</option>
                      <option value="Accessories">Accessories</option>
                      <option value="Software">Software</option>
                      <option value="Other">Other</option>
                    </select>
                  </div>
                  
                  <div class="col-md-6">
                    <label class="form-label text-light">Status</label>
                    <select
                      v-model="assetForm.status"
                      class="form-select custom-input"
                    >
                      <option value="Available">Available</option>
                      <option value="Assigned">Assigned</option>
                      <option value="Maintenance">Maintenance</option>
                      <option value="Retired">Retired</option>
                    </select>
                  </div>

                  <!-- Device Details -->
                  <div class="col-12 mt-4">
                    <h6 class="text-light fw-bold mb-3">
                      <i class="fas fa-cog me-2"></i>
                      Device Details
                    </h6>
                  </div>
                  
                  <div class="col-md-4">
                    <label class="form-label text-light">Brand</label>
                    <input
                      v-model="assetForm.brand"
                      type="text"
                      class="form-control custom-input"
                      placeholder="e.g., Dell, HP, Apple"
                    />
                  </div>
                  
                  <div class="col-md-4">
                    <label class="form-label text-light">Model</label>
                    <input
                      v-model="assetForm.model"
                      type="text"
                      class="form-control custom-input"
                      placeholder="e.g., XPS 15, MacBook Pro"
                    />
                  </div>
                  
                  <div class="col-md-4">
                    <label class="form-label text-light">Serial Number</label>
                    <input
                      v-model="assetForm.serialNumber"
                      type="text"
                      class="form-control custom-input"
                      placeholder="Device serial number"
                    />
                  </div>

                  <!-- Assignment & Location -->
                  <div class="col-12 mt-4">
                    <h6 class="text-light fw-bold mb-3">
                      <i class="fas fa-map-marker-alt me-2"></i>
                      Assignment & Location
                    </h6>
                  </div>
                  
                  <div class="col-md-6">
                    <label class="form-label text-light">Assigned To</label>
                    <select
                      v-model="assetForm.assignedTo"
                      class="form-select custom-input"
                      :disabled="assetForm.status === 'Retired'"
                    >
                      <option value="">Unassigned</option>
                      <!-- You'll need to load users list -->
                    </select>
                    <small class="text-light-emphasis">Select user to assign this asset</small>
                  </div>
                  
                  <div class="col-md-6">
                    <label class="form-label text-light">Location</label>
                    <input
                      v-model="assetForm.location"
                      type="text"
                      class="form-control custom-input"
                      placeholder="e.g., Office Floor 3, Room 301"
                    />
                  </div>

                  <!-- Financial Information -->
                  <div class="col-12 mt-4">
                    <h6 class="text-light fw-bold mb-3">
                      <i class="fas fa-dollar-sign me-2"></i>
                      Financial Information
                    </h6>
                  </div>
                  
                  <div class="col-md-4">
                    <label class="form-label text-light">Purchase Price</label>
                    <div class="input-group">
                      <span class="input-group-text custom-input-addon">$</span>
                      <input
                        v-model="assetForm.purchasePrice"
                        type="number"
                        step="0.01"
                        class="form-control custom-input"
                        placeholder="0.00"
                      />
                    </div>
                  </div>
                  
                  <div class="col-md-4">
                    <label class="form-label text-light">Purchase Date</label>
                    <input
                      v-model="assetForm.purchaseDate"
                      type="date"
                      class="form-control custom-input"
                    />
                  </div>
                  
                  <div class="col-md-4">
                    <label class="form-label text-light">Warranty Expiry</label>
                    <input
                      v-model="assetForm.warrantyExpiry"
                      type="date"
                      class="form-control custom-input"
                    />
                  </div>

                  <!-- Additional Information -->
                  <div class="col-12 mt-4">
                    <h6 class="text-light fw-bold mb-3">
                      <i class="fas fa-sticky-note me-2"></i>
                      Additional Information
                    </h6>
                  </div>
                  
                  <div class="col-12">
                    <label class="form-label text-light">Notes</label>
                    <textarea
                      v-model="assetForm.notes"
                      rows="4"
                      class="form-control custom-input"
                      placeholder="Additional notes, specifications, or comments..."
                    ></textarea>
                  </div>
                </div>

                <div v-if="formError" class="alert alert-danger mt-3">
                  <i class="fas fa-exclamation-triangle me-2"></i>
                  {{ formError }}
                </div>
              </div>
            </div>

            <div class="modal-footer border-top border-secondary sticky-bottom bg-modal-footer">
              <button
                type="button"
                @click="closeModal"
                class="btn btn-secondary"
              >
                <i class="fas fa-times me-2"></i>
                Cancel
              </button>
              <button
                type="submit"
                :disabled="saving"
                class="btn btn-primary"
              >
                <span v-if="saving" class="spinner-border spinner-border-sm me-2"></span>
                <i v-else class="fas fa-save me-2"></i>
                {{ saving ? 'Updating...' : 'Update Asset' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
definePageMeta({
  middleware: 'auth'
})

const config = useRuntimeConfig()
const route = useRoute()
const router = useRouter()
const user = useCookie('user')
const token = useCookie('token')

const asset = ref(null)
const loading = ref(false)
const saving = ref(false)
const showEditModal = ref(false)
const formError = ref('')
const assetForm = ref({})

const fetchAsset = async () => {
  loading.value = true
  try {
    const response = await $fetch(`${config.public.apiBase}/assets/${route.params.id}`, {
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })
    asset.value = response
  } catch (error) {
    console.error('Error fetching asset:', error)
    if (error.status === 404) {
      asset.value = null
    }
  } finally {
    loading.value = false
  }
}

const editAsset = () => {
  assetForm.value = {
    id: asset.value.id,
    name: asset.value.name,
    assetTag: asset.value.assetTag,
    category: asset.value.category,
    brand: asset.value.brand || '',
    model: asset.value.model || '',
    serialNumber: asset.value.serialNumber || '',
    status: asset.value.status,
    assignedTo: asset.value.assignedTo || '',
    location: asset.value.location || '',
    purchasePrice: asset.value.purchasePrice,
    purchaseDate: asset.value.purchaseDate ? asset.value.purchaseDate.split('T')[0] : '',
    warrantyExpiry: asset.value.warrantyExpiry ? asset.value.warrantyExpiry.split('T')[0] : '',
    notes: asset.value.notes || ''
  }
  showEditModal.value = true
}

const updateAsset = async () => {
  saving.value = true
  formError.value = ''

  try {
    await $fetch(`${config.public.apiBase}/assets/${assetForm.value.id}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token.value}`,
        'Content-Type': 'application/json'
      },
      body: assetForm.value
    })

    closeModal()
    await fetchAsset()
  } catch (error) {
    console.error('Error updating asset:', error)
    formError.value = error.data?.message || 'Error updating asset. Please try again.'
  } finally {
    saving.value = false
  }
}

const closeModal = () => {
  showEditModal.value = false
  formError.value = ''
  assetForm.value = {}
}

const getStatusClass = (status) => {
  const classes = {
    'Available': 'bg-success',
    'Assigned': 'bg-primary',
    'Maintenance': 'bg-warning text-dark',
    'Retired': 'bg-danger'
  }
  return classes[status] || 'bg-secondary'
}

const getAssetIcon = (category) => {
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
  return icons[category] || 'fas fa-cube'
}

const getWarrantyStatusColor = (warrantyDate) => {
  if (!warrantyDate) return 'text-light'
  
  const today = new Date()
  const warranty = new Date(warrantyDate)
  const daysUntilExpiry = Math.floor((warranty - today) / (1000 * 60 * 60 * 24))
  
  if (daysUntilExpiry < 0) {
    return 'text-danger' // Expired
  } else if (daysUntilExpiry < 90) {
    return 'text-warning' // Expiring soon
  }
  return 'text-success' // Valid
}

const isWarrantyExpired = (warrantyDate) => {
  if (!warrantyDate) return false
  const today = new Date()
  const warranty = new Date(warrantyDate)
  return warranty < today
}

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

const formatDateTime = (dateString) => {
  return new Date(dateString).toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

onMounted(() => {
  fetchAsset()
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

.glass-modal {
  background: rgba(15, 23, 42, 0.95);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 15px;
}

.asset-icon-large {
  width: 60px;
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  font-size: 1.5rem;
}

.info-item {
  padding: 0.5rem 0;
}

.custom-input {
  background: rgba(255, 255, 255, 0.1) !important;
  border: 1px solid rgba(255, 255, 255, 0.3) !important;
  color: #fff !important;
  border-radius: 8px !important;
  transition: all 0.3s ease;
}

.custom-input:focus {
  background: rgba(255, 255, 255, 0.15) !important;
  border-color: #06b6d4 !important;
  box-shadow: 0 0 0 0.2rem rgba(6, 182, 212, 0.25) !important;
  color: #fff !important;
}

.custom-input::placeholder {
  color: rgba(255, 255, 255, 0.6) !important;
}

.custom-input option {
  background: #1e293b !important;
  color: #fff !important;
}

.custom-input-addon {
  background: rgba(255, 255, 255, 0.1) !important;
  border: 1px solid rgba(255, 255, 255, 0.3) !important;
  border-right: none !important;
  color: rgba(255, 255, 255, 0.7) !important;
}

.btn-primary {
  background: linear-gradient(135deg, #06b6d4, #3b82f6) !important;
  border: none !important;
  border-radius: 8px !important;
  font-weight: 500;
  transition: all 0.3s ease;
}

.btn-primary:hover {
  background: linear-gradient(135deg, #0891b2, #2563eb) !important;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(6, 182, 212, 0.4);
}

.btn-warning {
  background: linear-gradient(135deg, #f59e0b, #d97706) !important;
  border: none !important;
  border-radius: 8px !important;
  font-weight: 500;
  transition: all 0.3s ease;
  color: white !important;
}

.btn-warning:hover {
  background: linear-gradient(135deg, #d97706, #b45309) !important;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(245, 158, 11, 0.4);
  color: white !important;
}

.btn-outline-light {
  border-color: rgba(255, 255, 255, 0.5) !important;
  color: rgba(255, 255, 255, 0.8) !important;
  border-radius: 8px !important;
  font-weight: 500;
}

.btn-outline-light:hover {
  background-color: rgba(255, 255, 255, 0.1) !important;
  border-color: rgba(255, 255, 255, 0.7) !important;
  color: #fff !important;
}

.btn-secondary {
  background: rgba(108, 117, 125, 0.3) !important;
  border-color: rgba(108, 117, 125, 0.5) !important;
  color: #fff !important;
  border-radius: 8px !important;
  font-weight: 500;
}

.btn-secondary:hover {
  background: rgba(108, 117, 125, 0.5) !important;
  border-color: rgba(108, 117, 125, 0.7) !important;
  color: #fff !important;
}

.text-light-emphasis {
  color: rgba(255, 255, 255, 0.7) !important;
}

.card-header {
  background: rgba(255, 255, 255, 0.05) !important;
}

.modal-header,
.modal-footer {
  background: transparent !important;
}

.bg-modal-footer {
  background: rgba(255, 255, 255, 0.05) !important;
}

.pulse-dot {
  position: absolute;
  top: -2px;
  right: -2px;
  width: 8px;
  height: 8px;
  background-color: #10b981;
  border-radius: 50%;
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0% {
    transform: scale(0.95);
    box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.7);
  }
  70% {
    transform: scale(1);
    box-shadow: 0 0 0 10px rgba(16, 185, 129, 0);
  }
  100% {
    transform: scale(0.95);
    box-shadow: 0 0 0 0 rgba(16, 185, 129, 0);
  }
}

/* Responsive improvements */
@media (max-width: 768px) {
  .asset-icon-large {
    width: 50px;
    height: 50px;
    font-size: 1.2rem;
  }
  
  .h2 {
    font-size: 1.5rem;
  }
  
  .h3 {
    font-size: 1.3rem;
  }
}

@media (max-width: 576px) {
  .asset-icon-large {
    width: 40px;
    height: 40px;
    font-size: 1rem;
  }
  
  .card-body {
    padding: 1rem;
  }
  
  .row.g-4 {
    --bs-gutter-x: 1rem;
    --bs-gutter-y: 1rem;
  }
}
</style>