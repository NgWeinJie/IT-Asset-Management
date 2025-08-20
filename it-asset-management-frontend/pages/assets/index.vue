<template>
  <div class="container-fluid p-4">
    <!-- Header -->
    <div class="mb-4">
      <div class="d-flex justify-content-between align-items-center">
        <div>
          <h1 class="h2 fw-bold text-light mb-1">Asset Management</h1>
          <p class="text-light-emphasis mb-0">Manage and track IT assets</p>
        </div>
        <div class="d-flex gap-2">
          <button
            v-if="user?.role === 'admin'"
            @click="exportAssets"
            class="btn btn-outline-success"
            :disabled="loading"
          >
            <i class="fas fa-file-export me-2"></i>
            Export
          </button>
          <button
            v-if="user?.role === 'admin'"
            @click="showCreateModal = true"
            class="btn btn-primary"
          >
            <i class="fas fa-plus me-2"></i>
            Add Asset
          </button>
        </div>
      </div>
    </div>

    <!-- Search and Filters -->
    <div class="card mb-4 glass-card">
      <div class="card-body">
        <div class="row g-3">
          <div class="col-md-3">
            <div class="input-group">
              <span class="input-group-text custom-input-addon">
                <i class="fas fa-search text-light-emphasis"></i>
              </span>
              <input
                v-model="searchParams.searchTerm"
                @input="debounceSearch"
                type="text"
                placeholder="Search assets..."
                class="form-control custom-input"
              />
            </div>
          </div>
          <div class="col-md-2">
            <select
              v-model="searchParams.category"
              @change="searchAssets"
              class="form-select custom-input"
            >
              <option value="">All Categories</option>
              <option v-for="category in categories" :key="category" :value="category">
                {{ category }}
              </option>
            </select>
          </div>
          <div class="col-md-2">
            <select
              v-model="searchParams.status"
              @change="searchAssets"
              class="form-select custom-input"
            >
              <option value="">All Status</option>
              <option v-for="status in statuses" :key="status" :value="status">
                {{ status }}
              </option>
            </select>
          </div>
          <div class="col-md-2">
            <select
              v-model="searchParams.assignedTo"
              @change="searchAssets"
              class="form-select custom-input"
            >
              <option value="">All Assignments</option>
              <option value="unassigned">Unassigned</option>
              <option v-for="user in users" :key="user.id" :value="user.username">
                {{ user.firstName }} {{ user.lastName }}
              </option>
            </select>
          </div>
          <div class="col-md-2">
            <select
              v-model="searchParams.pageSize"
              @change="searchAssets"
              class="form-select custom-input"
            >
              <option value="10">10 per page</option>
              <option value="25">25 per page</option>
              <option value="50">50 per page</option>
              <option value="100">100 per page</option>
            </select>
          </div>
          <div class="col-md-1">
            <button
              @click="resetSearch"
              class="btn btn-outline-light w-100"
              title="Reset Filters"
            >
              <i class="fas fa-undo"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Bulk Actions -->
    <div v-if="selectedAssets.length > 0" class="card mb-4 glass-card">
      <div class="card-body">
        <div class="d-flex justify-content-between align-items-center">
          <span class="text-light">
            <i class="fas fa-check-square me-2"></i>
            {{ selectedAssets.length }} asset(s) selected
          </span>
          <div class="d-flex gap-2">
            <button
              @click="bulkAssign"
              class="btn btn-sm btn-outline-primary"
              v-if="user?.role === 'Admin'"
            >
              <i class="fas fa-user-plus me-1"></i>
              Bulk Assign
            </button>
            <button
              @click="bulkUpdateStatus"
              class="btn btn-sm btn-outline-warning"
              v-if="user?.role === 'admin'"
            >
              <i class="fas fa-edit me-1"></i>
              Update Status
            </button>
            <button
              @click="bulkDelete"
              class="btn btn-sm btn-outline-danger"
              v-if="user?.role === 'admin'"
            >
              <i class="fas fa-trash me-1"></i>
              Delete Selected
            </button>
            <button
              @click="clearSelection"
              class="btn btn-sm btn-outline-light"
            >
              <i class="fas fa-times me-1"></i>
              Clear
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Assets Table -->
    <div class="card glass-card">
      <div v-if="loading" class="card-body text-center py-5">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <p class="mt-3 text-light-emphasis">Loading assets...</p>
      </div>

      <div v-else-if="assets.length === 0" class="card-body text-center py-5">
        <i class="fas fa-inbox text-light-emphasis display-4 mb-3"></i>
        <p class="text-light-emphasis mb-0">No assets found</p>
        <button 
          v-if="user?.role === 'admin'"
          @click="showCreateModal = true"
          class="btn btn-primary mt-3"
        >
          <i class="fas fa-plus me-2"></i>
          Add First Asset
        </button>
      </div>

      <div v-else>
        <div class="table-responsive">
          <table class="table table-dark table-hover mb-0">
            <thead>
              <tr>
                <th scope="col" width="50">
                  <div class="form-check">
                    <input
                      v-model="selectAll"
                      @change="toggleSelectAll"
                      class="form-check-input"
                      type="checkbox"
                      id="selectAll"
                    >
                    <label class="form-check-label" for="selectAll"></label>
                  </div>
                </th>
                <th scope="col" class="sortable" @click="sortBy('name')">
                  Asset
                  <i :class="getSortIcon('name')" class="ms-1"></i>
                </th>
                <th scope="col" class="sortable" @click="sortBy('category')">
                  Category
                  <i :class="getSortIcon('category')" class="ms-1"></i>
                </th>
                <th scope="col" class="sortable" @click="sortBy('status')">
                  Status
                  <i :class="getSortIcon('status')" class="ms-1"></i>
                </th>
                <th scope="col" class="sortable" @click="sortBy('assignedTo')">
                  Assigned To
                  <i :class="getSortIcon('assignedTo')" class="ms-1"></i>
                </th>
                <th scope="col">Location</th>
                <th scope="col">Purchase Date</th>
                <th scope="col" class="text-end">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="asset in assets" :key="asset.id" 
                  :class="{ 'table-active': selectedAssets.includes(asset.id) }">
                <td>
                  <div class="form-check">
                    <input
                      v-model="selectedAssets"
                      :value="asset.id"
                      class="form-check-input"
                      type="checkbox"
                      :id="`asset-${asset.id}`"
                    >
                    <label class="form-check-label" :for="`asset-${asset.id}`"></label>
                  </div>
                </td>
                <td>
                  <div class="d-flex align-items-center">
                    <div class="asset-icon me-2">
                      <i :class="getAssetIcon(asset.category)" class="text-primary"></i>
                    </div>
                    <div>
                      <div class="fw-medium text-light">{{ asset.name }}</div>
                      <small class="text-light-emphasis">{{ asset.assetTag }}</small>
                      <div v-if="asset.brand || asset.model" class="text-light-emphasis small">
                        {{ asset.brand }} {{ asset.model }}
                      </div>
                    </div>
                  </div>
                </td>
                <td class="text-light">
                  <span class="badge bg-secondary">{{ asset.category }}</span>
                </td>
                <td>
                  <span
                    :class="getStatusClass(asset.status)"
                    class="badge position-relative"
                  >
                    {{ asset.status }}
                    <span v-if="asset.status === 'Available'" class="pulse-dot"></span>
                  </span>
                </td>
                <td class="text-light">
                  <div v-if="asset.assignedTo" class="d-flex align-items-center">
                    <i class="fas fa-user me-2 text-primary"></i>
                    <span>{{ getAssignedUserName(asset.assignedTo) }}</span>
                  </div>
                  <span v-else class="text-light-emphasis">-</span>
                </td>
                <td class="text-light">
                  <div v-if="asset.location" class="d-flex align-items-center">
                    <i class="fas fa-map-marker-alt me-2 text-success"></i>
                    <span>{{ asset.location }}</span>
                  </div>
                  <span v-else class="text-light-emphasis">-</span>
                </td>
                <td class="text-light">
                  {{ asset.purchaseDate ? formatDate(asset.purchaseDate) : '-' }}
                </td>
                <td class="text-end">
                <div class="btn-group btn-group-sm" role="group">
                  <NuxtLink
                    :to="`/assets/${asset.id}`"
                    class="btn btn-outline-primary"
                    title="View Details"
                    data-bs-toggle="tooltip"
                    data-bs-placement="top"
                  >
                    <i class="fas fa-eye"></i>
                  </NuxtLink>
                  <button
                    v-if="user?.role === 'admin'"
                    @click="quickAssign(asset)"
                    class="btn btn-outline-info"
                    title="Quick Assign"
                    data-bs-toggle="tooltip"
                    data-bs-placement="top"
                    :disabled="asset.status === 'Retired'"
                  >
                    <i class="fas fa-user-plus"></i>
                  </button>
                  <button
                    v-if="user?.role === 'admin'"
                    @click="editAsset(asset)"
                    class="btn btn-outline-warning"
                    title="Edit Asset"
                    data-bs-toggle="tooltip"
                    data-bs-placement="top"
                  >
                    <i class="fas fa-pencil-alt"></i>
                  </button>
                  <button
                    v-if="user?.role === 'admin'"
                    @click="confirmDelete(asset)"
                    class="btn btn-outline-danger"
                    title="Delete Asset"
                    data-bs-toggle="tooltip"
                    data-bs-placement="top"
                  >
                    <i class="fas fa-trash"></i>
                  </button>
                </div>
              </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div class="card-footer d-flex justify-content-between align-items-center">
          <small class="text-light-emphasis">
            Showing {{ ((pagination.pageNumber - 1) * pagination.pageSize) + 1 }} to 
            {{ Math.min(pagination.pageNumber * pagination.pageSize, pagination.totalCount) }} of 
            {{ pagination.totalCount }} results
          </small>
          <nav>
            <ul class="pagination pagination-sm mb-0">
              <li class="page-item" :class="{ disabled: pagination.pageNumber <= 1 }">
                <button
                  @click="changePage(1)"
                  class="page-link"
                  :disabled="pagination.pageNumber <= 1"
                >
                  <i class="fas fa-angle-double-left"></i>
                </button>
              </li>
              <li class="page-item" :class="{ disabled: pagination.pageNumber <= 1 }">
                <button
                  @click="changePage(pagination.pageNumber - 1)"
                  class="page-link"
                  :disabled="pagination.pageNumber <= 1"
                >
                  <i class="fas fa-angle-left"></i>
                </button>
              </li>
              <li class="page-item active">
                <span class="page-link">
                  {{ pagination.pageNumber }} / {{ pagination.totalPages }}
                </span>
              </li>
              <li class="page-item" :class="{ disabled: pagination.pageNumber >= pagination.totalPages }">
                <button
                  @click="changePage(pagination.pageNumber + 1)"
                  class="page-link"
                  :disabled="pagination.pageNumber >= pagination.totalPages"
                >
                  <i class="fas fa-angle-right"></i>
                </button>
              </li>
              <li class="page-item" :class="{ disabled: pagination.pageNumber >= pagination.totalPages }">
                <button
                  @click="changePage(pagination.totalPages)"
                  class="page-link"
                  :disabled="pagination.pageNumber >= pagination.totalPages"
                >
                  <i class="fas fa-angle-double-right"></i>
                </button>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </div>

    <!-- Create/Edit Asset Modal -->
    <div
      v-if="showCreateModal || showEditModal"
      class="modal fade show d-block"
      tabindex="-1"
      style="background-color: rgba(0,0,0,0.7);"
      @click.self="closeModals"
    >
      <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
        <div class="modal-content glass-modal">
          <div class="modal-header border-bottom border-secondary sticky-top">
            <h5 class="modal-title text-light d-flex align-items-center">
              <i :class="showCreateModal ? 'fas fa-plus' : 'fas fa-edit'" class="me-2"></i>
              {{ showCreateModal ? 'Create New Asset' : 'Edit Asset' }}
            </h5>
            <button type="button" class="btn-close btn-close-white" @click="closeModals"></button>
          </div>

          <form @submit.prevent="saveAsset">
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
                      <option v-for="status in statuses" :key="status" :value="status">
                        {{ status }}
                      </option>
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
                      <option v-for="user in users" :key="user.id" :value="user.username">
                        {{ user.firstName }} {{ user.lastName }} ({{ user.username }})
                      </option>
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
                @click="closeModals"
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
                <i v-else :class="showCreateModal ? 'fas fa-plus' : 'fas fa-save'" class="me-2"></i>
                {{ saving ? 'Saving...' : (showCreateModal ? 'Create Asset' : 'Update Asset') }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Quick Assign Modal -->
    <div
      v-if="showAssignModal"
      class="modal fade show d-block"
      tabindex="-1"
      style="background-color: rgba(0,0,0,0.7);"
      @click.self="closeAssignModal"
    >
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content glass-modal">
          <div class="modal-header border-bottom border-secondary">
            <h5 class="modal-title text-light">
              <i class="fas fa-user-plus me-2"></i>
              Quick Assign Asset
            </h5>
            <button type="button" class="btn-close btn-close-white" @click="closeAssignModal"></button>
          </div>
          
          <div class="modal-body">
            <div class="mb-3">
              <h6 class="text-light">Asset:</h6>
              <div class="p-3 rounded" style="background: rgba(255,255,255,0.1);">
                <strong class="text-light">{{ selectedAssetForAssign?.name }}</strong>
                <br>
                <small class="text-light-emphasis">{{ selectedAssetForAssign?.assetTag }}</small>
              </div>
            </div>
            
            <div class="mb-3">
              <label class="form-label text-light">Assign To:</label>
              <select
                v-model="assignForm.assignedTo"
                class="form-select custom-input"
                required
              >
                <option value="">Select User</option>
                <option v-for="user in users" :key="user.id" :value="user.username">
                  {{ user.firstName }} {{ user.lastName }} ({{ user.username }})
                </option>
              </select>
            </div>
            
            <div class="mb-3">
              <label class="form-label text-light">Assignment Notes:</label>
              <textarea
                v-model="assignForm.notes"
                rows="3"
                class="form-control custom-input"
                placeholder="Optional assignment notes..."
              ></textarea>
            </div>
          </div>
          
          <div class="modal-footer border-top border-secondary">
            <button
              type="button"
              @click="closeAssignModal"
              class="btn btn-secondary"
            >
              Cancel
            </button>
            <button
              @click="processAssignment"
              :disabled="!assignForm.assignedTo || assigning"
              class="btn btn-primary"
            >
              <span v-if="assigning" class="spinner-border spinner-border-sm me-2"></span>
              <i v-else class="fas fa-check me-2"></i>
              {{ assigning ? 'Assigning...' : 'Assign Asset' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Confirmation Modal -->
    <div
      v-if="showConfirmModal"
      class="modal fade show d-block"
      tabindex="-1"
      style="background-color: rgba(0,0,0,0.7);"
    >
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content glass-modal">
          <div class="modal-header border-bottom border-secondary">
            <h5 class="modal-title text-light">
              <i class="fas fa-exclamation-triangle text-warning me-2"></i>
              Confirm Action
            </h5>
          </div>
          
          <div class="modal-body">
            <p class="text-light">{{ confirmMessage }}</p>
          </div>
          
          <div class="modal-footer border-top border-secondary">
            <button
              type="button"
              @click="showConfirmModal = false"
              class="btn btn-secondary"
            >
              Cancel
            </button>
            <button
              @click="executeConfirmAction"
              class="btn btn-danger"
            >
              <i class="fas fa-check me-2"></i>
              Confirm
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Success Toast -->
    <div
      v-if="showToast"
      class="position-fixed bottom-0 end-0 p-3"
      style="z-index: 1100;"
    >
      <div class="toast show" role="alert">
        <div class="toast-header bg-success text-white">
          <i class="fas fa-check-circle me-2"></i>
          <strong class="me-auto">Success</strong>
          <button type="button" class="btn-close btn-close-white" @click="showToast = false"></button>
        </div>
        <div class="toast-body bg-dark text-light">
          {{ successMessage }}
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
const user = useCookie('user')
const token = useCookie('token')

// Reactive data
const assets = ref([])
const users = ref([])
const categories = ref([])
const statuses = ref(['Available', 'Assigned', 'Maintenance', 'Retired'])
const loading = ref(false)
const saving = ref(false)
const assigning = ref(false)
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showAssignModal = ref(false)
const showConfirmModal = ref(false)
const showToast = ref(false)
const formError = ref('')
const selectedAssets = ref([])
const selectAll = ref(false)
const selectedAssetForAssign = ref(null)
const confirmMessage = ref('')
const confirmAction = ref(null)
const successMessage = ref('')

const searchParams = ref({
  searchTerm: '',
  category: '',
  status: '',
  assignedTo: '',
  pageNumber: 1,
  pageSize: 10,
  sortBy: '',
  sortOrder: 'asc'
})

const pagination = ref({
  totalCount: 0,
  pageNumber: 1,
  pageSize: 10,
  totalPages: 0
})

const assetForm = ref({
  id: null,
  name: '',
  assetTag: '',
  category: '',
  brand: '',
  model: '',
  serialNumber: '',
  status: 'Available',
  assignedTo: '',
  location: '',
  purchasePrice: null,
  purchaseDate: '',
  warrantyExpiry: '',
  notes: ''
})

const assignForm = ref({
  assignedTo: '',
  notes: ''
})

// Debounce search
let searchTimeout = null
const debounceSearch = () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    searchAssets()
  }, 300)
}

// Methods
const fetchAssets = async () => {
  loading.value = true
  try {
    const query = new URLSearchParams()
    Object.keys(searchParams.value).forEach(key => {
      if (searchParams.value[key]) {
        query.append(key, searchParams.value[key].toString())
      }
    })

    const response = await $fetch(`${config.public.apiBase}/assets?${query}`, {
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })

    assets.value = response.assets
    pagination.value = {
      totalCount: response.totalCount,
      pageNumber: response.pageNumber,
      pageSize: response.pageSize,
      totalPages: response.totalPages
    }
    
    // Clear selection when assets change
    selectedAssets.value = []
    selectAll.value = false
  } catch (error) {
    console.error('Error fetching assets:', error)
  } finally {
    loading.value = false
  }
}

const fetchUsers = async () => {
  try {
    const response = await $fetch(`${config.public.apiBase}/users`, {
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })
    users.value = response.users || response
  } catch (error) {
    console.error('Error fetching users:', error)
  }
}

const fetchCategories = async () => {
  try {
    const response = await $fetch(`${config.public.apiBase}/assets/categories`, {
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })
    categories.value = response
  } catch (error) {
    console.error('Error fetching categories:', error)
  }
}

const searchAssets = () => {
  searchParams.value.pageNumber = 1
  fetchAssets()
}

const resetSearch = () => {
  searchParams.value = {
    searchTerm: '',
    category: '',
    status: '',
    assignedTo: '',
    pageNumber: 1,
    pageSize: 10,
    sortBy: '',
    sortOrder: 'asc'
  }
  fetchAssets()
}

const sortBy = (field) => {
  if (searchParams.value.sortBy === field) {
    searchParams.value.sortOrder = searchParams.value.sortOrder === 'asc' ? 'desc' : 'asc'
  } else {
    searchParams.value.sortBy = field
    searchParams.value.sortOrder = 'asc'
  }
  searchAssets()
}

const getSortIcon = (field) => {
  if (searchParams.value.sortBy !== field) return 'fas fa-sort text-light-emphasis'
  return searchParams.value.sortOrder === 'asc' ? 'fas fa-sort-up text-primary' : 'fas fa-sort-down text-primary'
}

const changePage = (page) => {
  if (page >= 1 && page <= pagination.value.totalPages) {
    searchParams.value.pageNumber = page
    fetchAssets()
  }
}

const toggleSelectAll = () => {
  if (selectAll.value) {
    selectedAssets.value = assets.value.map(asset => asset.id)
  } else {
    selectedAssets.value = []
  }
}

const clearSelection = () => {
  selectedAssets.value = []
  selectAll.value = false
}

const editAsset = (asset) => {
  assetForm.value = {
    id: asset.id,
    name: asset.name,
    assetTag: asset.assetTag,
    category: asset.category,
    brand: asset.brand || '',
    model: asset.model || '',
    serialNumber: asset.serialNumber || '',
    status: asset.status,
    assignedTo: asset.assignedTo || '',
    location: asset.location || '',
    purchasePrice: asset.purchasePrice,
    purchaseDate: asset.purchaseDate ? asset.purchaseDate.split('T')[0] : '',
    warrantyExpiry: asset.warrantyExpiry ? asset.warrantyExpiry.split('T')[0] : '',
    notes: asset.notes || ''
  }
  showEditModal.value = true
}

const confirmDelete = (asset) => {
  confirmMessage.value = `Are you sure you want to delete "${asset.name}"? This action cannot be undone.`
  confirmAction.value = () => deleteAsset(asset)
  showConfirmModal.value = true
}

const deleteAsset = async (asset) => {
  try {
    await $fetch(`${config.public.apiBase}/assets/${asset.id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })
    await fetchAssets()
    showConfirmModal.value = false
    showSuccessToast('Asset deleted successfully!')
  } catch (error) {
    console.error('Error deleting asset:', error)
    formError.value = 'Error deleting asset. Please try again.'
  }
}

const bulkDelete = () => {
  confirmMessage.value = `Are you sure you want to delete ${selectedAssets.value.length} selected asset(s)? This action cannot be undone.`
  confirmAction.value = async () => {
    try {
      await $fetch(`${config.public.apiBase}/assets/bulk-delete`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token.value}`,
          'Content-Type': 'application/json'
        },
        body: { assetIds: selectedAssets.value }
      })
      await fetchAssets()
      showConfirmModal.value = false
      showSuccessToast('Assets deleted successfully!')
    } catch (error) {
      console.error('Error bulk deleting assets:', error)
      formError.value = 'Error deleting assets. Please try again.'
    }
  }
  showConfirmModal.value = true
}

const saveAsset = async () => {
  saving.value = true
  formError.value = ''

  try {
    const method = showCreateModal.value ? 'POST' : 'PUT'
    const url = showCreateModal.value 
      ? `${config.public.apiBase}/assets`
      : `${config.public.apiBase}/assets/${assetForm.value.id}`

    await $fetch(url, {
      method,
      headers: {
        'Authorization': `Bearer ${token.value}`,
        'Content-Type': 'application/json'
      },
      body: assetForm.value
    })

    closeModals()
    await fetchAssets()
    showSuccessToast(showCreateModal.value ? 'Asset created successfully!' : 'Asset updated successfully!')
  } catch (error) {
    console.error('Error saving asset:', error)
    formError.value = error.data?.message || 'Error saving asset. Please try again.'
  } finally {
    saving.value = false
  }
}

const quickAssign = (asset) => {
  selectedAssetForAssign.value = asset
  assignForm.value = {
    assignedTo: asset.assignedTo || '',
    notes: ''
  }
  showAssignModal.value = true
}

const processAssignment = async () => {
  assigning.value = true
  try {
    await $fetch(`${config.public.apiBase}/assets/${selectedAssetForAssign.value.id}/assign`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token.value}`,
        'Content-Type': 'application/json'
      },
      body: assignForm.value
    })
    
    closeAssignModal()
    await fetchAssets()
    showSuccessToast('Asset assigned successfully!')
  } catch (error) {
    console.error('Error assigning asset:', error)
    formError.value = 'Error assigning asset. Please try again.'
  } finally {
    assigning.value = false
  }
}

const bulkAssign = () => {
  // Implementation for bulk assign modal
  console.log('Bulk assign functionality to be implemented')
}

const bulkUpdateStatus = () => {
  // Implementation for bulk status update modal
  console.log('Bulk status update functionality to be implemented')
}

const exportAssets = async () => {
  try {
    const query = new URLSearchParams()
    Object.keys(searchParams.value).forEach(key => {
      if (searchParams.value[key] && key !== 'pageNumber' && key !== 'pageSize') {
        query.append(key, searchParams.value[key].toString())
      }
    })
    
    const response = await $fetch(`${config.public.apiBase}/assets/export?${query}`, {
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })
    
    // Create and download CSV file
    const blob = new Blob([response], { type: 'text/csv' })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `assets_export_${new Date().toISOString().split('T')[0]}.csv`
    link.click()
    window.URL.revokeObjectURL(url)
    showSuccessToast('Assets exported successfully!')
  } catch (error) {
    console.error('Error exporting assets:', error)
    formError.value = 'Error exporting assets. Please try again.'
  }
}

const closeModals = () => {
  showCreateModal.value = false
  showEditModal.value = false
  formError.value = ''
  assetForm.value = {
    id: null,
    name: '',
    assetTag: '',
    category: '',
    brand: '',
    model: '',
    serialNumber: '',
    status: 'Available',
    assignedTo: '',
    location: '',
    purchasePrice: null,
    purchaseDate: '',
    warrantyExpiry: '',
    notes: ''
  }
}

const closeAssignModal = () => {
  showAssignModal.value = false
  selectedAssetForAssign.value = null
  assignForm.value = {
    assignedTo: '',
    notes: ''
  }
}

const executeConfirmAction = () => {
  if (confirmAction.value) {
    confirmAction.value()
  }
}

const showSuccessToast = (message) => {
  successMessage.value = message
  showToast.value = true
  setTimeout(() => {
    showToast.value = false
  }, 3000)
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

const getAssignedUserName = (username) => {
  const user = users.value.find(u => u.username === username)
  return user ? `${user.firstName} ${user.lastName}` : username
}

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

// Watch for selected assets changes
watch(selectedAssets, (newVal) => {
  selectAll.value = newVal.length === assets.value.length && assets.value.length > 0
})

// Lifecycle
onMounted(() => {
  fetchAssets()
  fetchCategories()
  fetchUsers()
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

.table-dark {
  --bs-table-bg: rgba(255, 255, 255, 0.05);
  border: none;
}

.table-dark > :not(caption) > * > * {
  border-bottom-color: rgba(255, 255, 255, 0.1);
}

.table-hover > tbody > tr:hover > * {
  --bs-table-accent-bg: rgba(255, 255, 255, 0.075);
}

.table-active > * {
  background-color: rgba(6, 182, 212, 0.1) !important;
}

.sortable {
  cursor: pointer;
  user-select: none;
}

.sortable:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

.asset-icon {
  width: 30px;
  height: 30px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  font-size: 0.9rem;
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

.btn-outline-success {
  border-color: #10b981 !important;
  color: #10b981 !important;
}

.btn-outline-success:hover {
  background-color: #10b981 !important;
  border-color: #10b981 !important;
  color: white !important;
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

.btn-outline-info {
  border-color: #06b6d4 !important;
  color: #06b6d4 !important;
}

.btn-outline-info:hover {
  background-color: #06b6d4 !important;
  border-color: #06b6d4 !important;
  color: white !important;
}

.btn-outline-warning {
  border-color: #f59e0b !important;
  color: #f59e0b !important;
}

.btn-outline-warning:hover {
  background-color: #f59e0b !important;
  border-color: #f59e0b !important;
  color: white !important;
}

.btn-outline-danger {
  border-color: #ef4444 !important;
  color: #ef4444 !important;
}

.btn-outline-danger:hover {
  background-color: #ef4444 !important;
  border-color: #ef4444 !important;
  color: white !important;
}

.btn-outline-light {
  border-color: rgba(255, 255, 255, 0.5) !important;
  color: rgba(255, 255, 255, 0.8) !important;
}

.btn-outline-light:hover {
  background-color: rgba(255, 255, 255, 0.1) !important;
  border-color: rgba(255, 255, 255, 0.7) !important;
  color: #fff !important;
}

.text-light-emphasis {
  color: rgba(255, 255, 255, 0.7) !important;
}

.card-footer {
  background: rgba(255, 255, 255, 0.05) !important;
  border-top: 1px solid rgba(255, 255, 255, 0.1) !important;
}

.modal-header,
.modal-footer {
  background: transparent !important;
}

.pagination .page-link {
  background: rgba(255, 255, 255, 0.1) !important;
  border-color: rgba(255, 255, 255, 0.2) !important;
  color: #fff !important;
}

.pagination .page-item.active .page-link {
  background: linear-gradient(135deg, #06b6d4, #3b82f6) !important;
  border-color: #06b6d4 !important;
}

.pagination .page-link:hover {
  background: rgba(255, 255, 255, 0.2) !important;
  border-color: rgba(255, 255, 255, 0.3) !important;
  color: #fff !important;
}

.form-check-input:checked {
  background-color: #06b6d4;
  border-color: #06b6d4;
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

/* Improved action buttons styling */
.btn-group-vertical .btn {
  min-width: 100px;
  text-align: left;
  border-radius: 6px !important;
}

.btn-group-vertical .btn:not(:last-child) {
  margin-bottom: 4px;
}

/* NuxtLink button styling fix */
.btn-outline-primary.btn-sm {
  color: #06b6d4 !important;
  text-decoration: none;
}

.btn-outline-primary.btn-sm:hover {
  color: white !important;
  text-decoration: none;
}

/* Responsive improvements */
@media (max-width: 768px) {
  .btn-group-vertical {
    width: 100%;
  }
  
  .btn-group-vertical .btn {
    width: 100%;
    min-width: auto;
  }
  
  .table-responsive {
    font-size: 0.875rem;
  }
  
  .asset-icon {
    width: 24px;
    height: 24px;
    font-size: 0.8rem;
  }
}

@media (max-width: 576px) {
  .btn-group-vertical .btn {
    font-size: 0.8rem;
    padding: 0.25rem 0.5rem;
  }
  
  .btn-group-vertical .btn i {
    margin-right: 0.25rem;
  }
}

/* Icon-only button group styling */
.btn-group .btn {
  min-width: 34px !important;
  height: 34px !important;
  padding: 0.4rem !important;
  border-radius: 50% !important;
  margin-right: 6px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.btn-group .btn:last-child {
  margin-right: 0;
}

.btn-group .btn i {
  font-size: 0.875rem;
}

.btn-group .btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
}

/* Tooltip styling - if using Bootstrap tooltips */
.tooltip {
  font-size: 0.75rem;
}

.tooltip-inner {
  background-color: rgba(0, 0, 0, 0.9) !important;
  color: white !important;
  padding: 4px 8px;
  border-radius: 4px;
}

/* Custom tooltip implementation if not using Bootstrap tooltips */
.btn[title]:hover::after {
  content: attr(title);
  position: absolute;
  bottom: 100%;
  left: 50%;
  transform: translateX(-50%);
  background-color: rgba(0, 0, 0, 0.9);
  color: white;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 0.75rem;
  white-space: nowrap;
  z-index: 1000;
  margin-bottom: 4px;
  pointer-events: none;
}

.btn[title]:hover::before {
  content: '';
  position: absolute;
  bottom: 100%;
  left: 50%;
  transform: translateX(-50%);
  width: 0;
  height: 0;
  border-left: 4px solid transparent;
  border-right: 4px solid transparent;
  border-top: 4px solid rgba(0, 0, 0, 0.9);
  z-index: 1000;
  pointer-events: none;
}

/* Responsive adjustments for small screens */
@media (max-width: 768px) {
  .btn-group .btn {
    min-width: 30px !important;
    height: 30px !important;
    padding: 0.3rem !important;
  }
  
  .btn-group .btn i {
    font-size: 0.8rem;
  }
}

@media (max-width: 576px) {
  .btn-group .btn {
    min-width: 28px !important;
    height: 28px !important;
    padding: 0.25rem !important;
    margin-right: 1px;
  }
  
  .btn-group .btn i {
    font-size: 0.75rem;
  }
}
</style>