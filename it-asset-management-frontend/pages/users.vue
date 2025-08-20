<template>
  <div class="container-fluid p-4">
    <!-- Header -->
    <div class="mb-4">
      <div class="d-flex justify-content-between align-items-center">
        <div>
          <h1 class="h2 fw-bold text-light mb-1">User Management</h1>
          <p class="text-light-emphasis mb-0">Manage users and their asset assignments</p>
        </div>
        <div class="d-flex gap-2">
          <button
            @click="exportUsers"
            class="btn btn-outline-success"
            :disabled="loading"
          >
            <i class="fas fa-file-export me-2"></i>
            Export
          </button>
          <button
            @click="showCreateModal = true"
            class="btn btn-primary"
          >
            <i class="fas fa-plus me-2"></i>
            Register New User
          </button>
        </div>
      </div>
    </div>

    <!-- Search and Filters -->
    <div class="card mb-4 glass-card">
      <div class="card-body">
        <div class="row g-3">
          <div class="col-md-4">
            <div class="input-group">
              <span class="input-group-text custom-input-addon">
                <i class="fas fa-search text-light-emphasis"></i>
              </span>
              <input
                v-model="searchParams.searchTerm"
                @input="debounceSearch"
                type="text"
                placeholder="Search users..."
                class="form-control custom-input"
              />
            </div>
          </div>
          <div class="col-md-3">
          <select v-model="searchParams.role" @change="searchUsers" class="form-select custom-input">
            <option value="">All Roles</option>
            <option value="admin">Admin</option>
            <option value="user">User</option>
            <option value="manager">Manager</option>
          </select>
          </div>
          <div class="col-md-3">
            <select
              v-model="searchParams.department"
              @change="searchUsers"
              class="form-select custom-input"
            >
              <option value="">All Departments</option>
              <option v-for="dept in departments" :key="dept" :value="dept">
                {{ dept }}
              </option>
            </select>
          </div>
          <div class="col-md-2">
            <button
              @click="resetSearch"
              class="btn btn-outline-light w-100"
              title="Reset Filters"
            >
              <i class="fas fa-undo me-2"></i>
              Reset
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Bulk Actions -->
    <div v-if="selectedUsers.length > 0" class="card mb-4 glass-card">
      <div class="card-body">
        <div class="d-flex justify-content-between align-items-center">
          <span class="text-light">
            <i class="fas fa-check-square me-2"></i>
            {{ selectedUsers.length }} user(s) selected
          </span>
          <div class="d-flex gap-2">
            <button
              @click="bulkUpdateRole"
              class="btn btn-sm btn-outline-warning"
            >
              <i class="fas fa-user-tag me-1"></i>
              Update Role
            </button>
            <button
              @click="bulkDelete"
              class="btn btn-sm btn-outline-danger"
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

    <!-- Users Table -->
    <div class="card glass-card">
      <div v-if="loading" class="card-body text-center py-5">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <p class="mt-3 text-light-emphasis">Loading users...</p>
      </div>

      <div v-else-if="users.length === 0" class="card-body text-center py-5">
        <i class="fas fa-users text-light-emphasis display-4 mb-3"></i>
        <p class="text-light-emphasis mb-0">No users found</p>
        <button 
          @click="showCreateModal = true"
          class="btn btn-primary mt-3"
        >
          <i class="fas fa-plus me-2"></i>
          Register First User
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
                <th scope="col" class="sortable" @click="sortBy('firstName')">
                  User
                  <i :class="getSortIcon('firstName')" class="ms-1"></i>
                </th>
                <th scope="col" class="sortable" @click="sortBy('email')">
                  Email
                  <i :class="getSortIcon('email')" class="ms-1"></i>
                </th>
                <th scope="col" class="sortable" @click="sortBy('role')">
                  Role
                  <i :class="getSortIcon('role')" class="ms-1"></i>
                </th>
                <th scope="col">Department</th>
                <th scope="col">Assets</th>
                <th scope="col" class="sortable" @click="sortBy('createdAt')">
                  Created
                  <i :class="getSortIcon('createdAt')" class="ms-1"></i>
                </th>
                <th scope="col" class="text-end">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="user in filteredUsers" :key="user.id" 
                  :class="{ 'table-active': selectedUsers.includes(user.id) }">
                <td>
                  <div class="form-check">
                    <input
                      v-model="selectedUsers"
                      :value="user.id"
                      class="form-check-input"
                      type="checkbox"
                      :id="`user-${user.id}`"
                    >
                    <label class="form-check-label" :for="`user-${user.id}`"></label>
                  </div>
                </td>
                <td>
                  <div class="d-flex align-items-center">
                    <div class="avatar me-3">
                      {{ user.firstName?.[0] }}{{ user.lastName?.[0] }}
                    </div>
                    <div>
                      <div class="fw-medium text-light">{{ user.firstName }} {{ user.lastName }}</div>
                      <small class="text-light-emphasis">{{ user.username }}</small>
                    </div>
                  </div>
                </td>
                <td class="text-light">{{ user.email }}</td>
                <td>
                  <span :class="getRoleBadgeClass(user.role)" class="badge">
                    {{ user.role }}
                  </span>
                </td>
                <td class="text-light">
                  <div v-if="user.department" class="d-flex align-items-center">
                    <i class="fas fa-building me-2 text-success"></i>
                    <span>{{ user.department }}</span>
                  </div>
                  <span v-else class="text-light-emphasis">-</span>
                </td>
                <td class="text-light">
                  <div class="d-flex align-items-center">
                    <i class="fas fa-laptop me-2 text-primary"></i>
                    <span>{{ user.assignedAssets || 0 }}</span>
                  </div>
                </td>
                <td class="text-light">
                  {{ formatDate(user.createdAt) }}
                </td>
                <td class="text-end">
                  <div class="btn-group btn-group-sm" role="group">
                    <button
                      @click="editUser(user)"
                      class="btn btn-outline-warning"
                      title="Edit User"
                    >
                      <i class="fas fa-pencil-alt"></i>
                    </button>
                    <button
                      @click="confirmDelete(user)"
                      class="btn btn-outline-danger"
                      title="Delete User"
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

    <!-- Create/Edit User Modal -->
    <div
      v-if="showCreateModal || showEditModal"
      class="modal fade show d-block"
      tabindex="-1"
      style="background-color: rgba(0,0,0,0.7);"
      @click.self="closeModals"
    >
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content glass-modal">
          <div class="modal-header border-bottom border-secondary">
            <h5 class="modal-title text-light d-flex align-items-center">
              <i :class="showCreateModal ? 'fas fa-user-plus' : 'fas fa-user-edit'" class="me-2"></i>
              {{ showCreateModal ? 'Register New User' : 'Edit User' }}
            </h5>
            <button type="button" class="btn-close btn-close-white" @click="closeModals"></button>
          </div>

          <form @submit.prevent="saveUser">
            <div class="modal-body">
              <div class="container-fluid p-0">
                <div class="row g-3">
                  <!-- Personal Information -->
                  <div class="col-12">
                    <h6 class="text-light fw-bold mb-3">
                      <i class="fas fa-user me-2"></i>
                      Personal Information
                    </h6>
                  </div>
                  
                  <div class="col-md-6">
                    <label class="form-label text-light">First Name *</label>
                    <input
                      v-model="userForm.firstName"
                      type="text"
                      required
                      class="form-control custom-input"
                      placeholder="Enter first name"
                      :disabled="saving"
                    />
                  </div>
                  
                  <div class="col-md-6">
                    <label class="form-label text-light">Last Name *</label>
                    <input
                      v-model="userForm.lastName"
                      type="text"
                      required
                      class="form-control custom-input"
                      placeholder="Enter last name"
                      :disabled="saving"
                    />
                  </div>
                  
                  <div class="col-md-6">
                    <label class="form-label text-light">Username *</label>
                    <input
                      v-model="userForm.username"
                      type="text"
                      required
                      class="form-control custom-input"
                      placeholder="Enter username"
                      :disabled="saving"
                    />
                  </div>
                  
                  <div class="col-md-6">
                    <label class="form-label text-light">Email *</label>
                    <input
                      v-model="userForm.email"
                      type="email"
                      required
                      class="form-control custom-input"
                      placeholder="Enter email address"
                      :disabled="saving"
                    />
                  </div>

                  <!-- Account Details -->
                  <div class="col-12 mt-4">
                    <h6 class="text-light fw-bold mb-3">
                      <i class="fas fa-key me-2"></i>
                      Account Details
                    </h6>
                  </div>
                  
                  <div class="col-md-6" v-if="showCreateModal">
                    <label class="form-label text-light">Password *</label>
                    <input
                      v-model="userForm.password"
                      type="password"
                      :required="showCreateModal"
                      minlength="6"
                      class="form-control custom-input"
                      placeholder="Enter password"
                      :disabled="saving"
                    />
                    <small class="text-light-emphasis">Minimum 6 characters</small>
                  </div>
                  
                  <div class="col-md-6" v-if="showCreateModal">
                    <label class="form-label text-light">Confirm Password *</label>
                    <input
                      v-model="userForm.confirmPassword"
                      type="password"
                      :required="showCreateModal"
                      minlength="6"
                      class="form-control custom-input"
                      placeholder="Confirm password"
                      :disabled="saving"
                    />
                  </div>
                  
                  <div class="col-md-6">
                    <label class="form-label text-light">Role *</label>
                    <select
                      v-model="userForm.role"
                      required
                      class="form-select custom-input"
                      :disabled="saving"
                    >
                      <option value="">Select Role</option>
                      <option value="admin">Admin</option>
                      <option value="manager">Manager</option>
                      <option value="user">User</option>
                    </select>
                  </div>
                  
                  <div class="col-md-6">
                    <label class="form-label text-light">Department</label>
                    <input
                      v-model="userForm.department"
                      type="text"
                      class="form-control custom-input"
                      placeholder="e.g., IT, HR, Finance"
                      :disabled="saving"
                    />
                  </div>

                  <!-- Additional Information -->
                  <div class="col-12 mt-4">
                    <h6 class="text-light fw-bold mb-3">
                      <i class="fas fa-info-circle me-2"></i>
                      Additional Information
                    </h6>
                  </div>
                  
                  <div class="col-12">
                    <label class="form-label text-light">Notes</label>
                    <textarea
                      v-model="userForm.notes"
                      rows="3"
                      class="form-control custom-input"
                      placeholder="Additional notes about the user..."
                      :disabled="saving"
                    ></textarea>
                  </div>
                </div>

                <div v-if="formError" class="alert alert-danger mt-3">
                  <i class="fas fa-exclamation-triangle me-2"></i>
                  {{ formError }}
                </div>
              </div>
            </div>

            <div class="modal-footer border-top border-secondary">
              <button
                type="button"
                @click="closeModals"
                class="btn btn-secondary"
                :disabled="saving"
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
                <i v-else :class="showCreateModal ? 'fas fa-user-plus' : 'fas fa-save'" class="me-2"></i>
                {{ saving ? 'Saving...' : (showCreateModal ? 'Register User' : 'Update User') }}
              </button>
            </div>
          </form>
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
const token = useCookie('token')

// Reactive data
const users = ref([])
const departments = ref([])
const loading = ref(false)
const saving = ref(false)
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showConfirmModal = ref(false)
const showToast = ref(false)
const formError = ref('')
const selectedUsers = ref([])
const selectAll = ref(false)
const confirmMessage = ref('')
const confirmAction = ref(null)
const successMessage = ref('')

const searchParams = ref({
  searchTerm: '',
  role: '',
  department: '',
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

const userForm = ref({
  id: null,
  firstName: '',
  lastName: '',
  username: '',
  email: '',
  password: '',
  confirmPassword: '',
  role: '',
  department: '',
  notes: ''
})

// Debounce search
let searchTimeout = null
const debounceSearch = () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    searchUsers()
  }, 300)
}

// Computed
const filteredUsers = computed(() => {
  if (!Array.isArray(users.value)) return []
  return users.value.filter(user => {
    const matchesSearch = !searchParams.value.searchTerm || 
      `${user.firstName} ${user.lastName} ${user.username} ${user.email}`.toLowerCase()
        .includes(searchParams.value.searchTerm.toLowerCase())
    
    const matchesRole = !searchParams.value.role || user.role === searchParams.value.role
    const matchesDept = !searchParams.value.department || user.department === searchParams.value.department
    
    return matchesSearch && matchesRole && matchesDept
  })
})

// Methods
const fetchUsers = async () => {
  loading.value = true
  try {
    const query = new URLSearchParams()
    Object.keys(searchParams.value).forEach(key => {
      if (searchParams.value[key]) {
        query.append(key, searchParams.value[key].toString())
      }
    })

    const response = await $fetch(`${config.public.apiBase}/users?${query}`, {
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })

    users.value = response.users || response
    pagination.value = {
      totalCount: response.totalCount || users.value.length,
      pageNumber: response.pageNumber || 1,
      pageSize: response.pageSize || 10,
      totalPages: response.totalPages || Math.ceil((response.totalCount || users.value.length) / 10)
    }
    
    // Clear selection when users change
    selectedUsers.value = []
    selectAll.value = false
  } catch (error) {
    console.error('Error fetching users:', error)
  } finally {
    loading.value = false
  }
}

const fetchDepartments = async () => {
  try {
    const response = await $fetch(`${config.public.apiBase}/users/departments`, {
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })
    departments.value = response
  } catch (error) {
    console.error('Error fetching departments:', error)
  }
}

const searchUsers = () => {
  searchParams.value.pageNumber = 1
  fetchUsers()
}

const resetSearch = () => {
  searchParams.value = {
    searchTerm: '',
    role: '',
    department: '',
    pageNumber: 1,
    pageSize: 10,
    sortBy: '',
    sortOrder: 'asc'
  }
  fetchUsers()
}

const sortBy = (field) => {
  if (searchParams.value.sortBy === field) {
    searchParams.value.sortOrder = searchParams.value.sortOrder === 'asc' ? 'desc' : 'asc'
  } else {
    searchParams.value.sortBy = field
    searchParams.value.sortOrder = 'asc'
  }
  searchUsers()
}

const getSortIcon = (field) => {
  if (searchParams.value.sortBy !== field) return 'fas fa-sort text-light-emphasis'
  return searchParams.value.sortOrder === 'asc' ? 'fas fa-sort-up text-primary' : 'fas fa-sort-down text-primary'
}

const changePage = (page) => {
  if (page >= 1 && page <= pagination.value.totalPages) {
    searchParams.value.pageNumber = page
    fetchUsers()
  }
}

const toggleSelectAll = () => {
  if (selectAll.value) {
    selectedUsers.value = filteredUsers.value.map(user => user.id)
  } else {
    selectedUsers.value = []
  }
}

const clearSelection = () => {
  selectedUsers.value = []
  selectAll.value = false
}

const editUser = (user) => {
  userForm.value = {
    id: user.id,
    firstName: user.firstName,
    lastName: user.lastName,
    username: user.username,
    email: user.email,
    password: '',
    confirmPassword: '',
    role: user.role || '',
    department: user.department || '',
    notes: user.notes || ''
  }
  showEditModal.value = true
}

const confirmDelete = (user) => {
  confirmMessage.value = `Are you sure you want to delete "${user.firstName} ${user.lastName}"? This action cannot be undone.`
  confirmAction.value = () => deleteUser(user)
  showConfirmModal.value = true
}

const deleteUser = async (user) => {
  try {
    await $fetch(`${config.public.apiBase}/users/${user.id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })
    await fetchUsers()
    showConfirmModal.value = false
    showSuccessToast('User deleted successfully!')
  } catch (error) {
    console.error('Error deleting user:', error)
    formError.value = 'Error deleting user. Please try again.'
  }
}

const bulkDelete = () => {
  confirmMessage.value = `Are you sure you want to delete ${selectedUsers.value.length} selected user(s)? This action cannot be undone.`
  confirmAction.value = async () => {
    try {
      await $fetch(`${config.public.apiBase}/users/bulk-delete`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token.value}`,
          'Content-Type': 'application/json'
        },
        body: { userIds: selectedUsers.value }
      })
      await fetchUsers()
      showConfirmModal.value = false
      showSuccessToast('Users deleted successfully!')
    } catch (error) {
      console.error('Error bulk deleting users:', error)
      formError.value = 'Error deleting users. Please try again.'
    }
  }
  showConfirmModal.value = true
}

const bulkUpdateRole = () => {
  // Implementation for bulk role update
  console.log('Bulk role update functionality to be implemented')
}

const saveUser = async () => {
  saving.value = true
  formError.value = ''

  // Validation for create modal
  if (showCreateModal.value) {
    if (userForm.value.password !== userForm.value.confirmPassword) {
      formError.value = 'Passwords do not match!'
      saving.value = false
      return
    }
    if (userForm.value.password.length < 6) {
      formError.value = 'Password must be at least 6 characters long!'
      saving.value = false
      return
    }
  }

  try {
    const method = showCreateModal.value ? 'POST' : 'PUT'
    const url = showCreateModal.value 
      ? `${config.public.apiBase}/users`
      : `${config.public.apiBase}/users/${userForm.value.id}`

    const userData = { ...userForm.value }
    delete userData.confirmPassword

    await $fetch(url, {
      method,
      headers: {
        'Authorization': `Bearer ${token.value}`,
        'Content-Type': 'application/json'
      },
      body: userData
    })

    closeModals()
    await fetchUsers()
    showSuccessToast(showCreateModal.value ? 'User registered successfully!' : 'User updated successfully!')
  } catch (error) {
    console.error('Error saving user:', error)
    formError.value = error.data?.message || 'Error saving user. Please try again.'
  } finally {
    saving.value = false
  }
}

const exportUsers = async () => {
  try {
    const query = new URLSearchParams()
    Object.keys(searchParams.value).forEach(key => {
      if (searchParams.value[key] && key !== 'pageNumber' && key !== 'pageSize') {
        query.append(key, searchParams.value[key].toString())
      }
    })
    
    const response = await $fetch(`${config.public.apiBase}/users/export?${query}`, {
      headers: {
        'Authorization': `Bearer ${token.value}`
      }
    })
    
    // Create and download CSV file
    const blob = new Blob([response], { type: 'text/csv' })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `users_export_${new Date().toISOString().split('T')[0]}.csv`
    link.click()
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('Error exporting users:', error)
    formError.value = 'Error exporting users. Please try again.'
  }
}

const closeModals = () => {
  showCreateModal.value = false
  showEditModal.value = false
  formError.value = ''
  userForm.value = {
    id: null,
    firstName: '',
    lastName: '',
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    role: '',
    department: '',
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

const getRoleBadgeClass = (role) => {
  const classes = {
    'admin': 'bg-danger',
    'manager': 'bg-warning text-dark',
    'user': 'bg-info'
  }
  return classes[role] || 'bg-secondary'
}

const formatDate = (dateString) => {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

// Watch for selected users changes
watch(selectedUsers, (newVal) => {
  selectAll.value = newVal.length === filteredUsers.value.length && filteredUsers.value.length > 0
})

// Lifecycle
onMounted(() => {
  fetchUsers()
  fetchDepartments()
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

.avatar {
  width: 40px;
  height: 40px;
  background: linear-gradient(135deg, #06b6d4, #3b82f6);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  color: white;
  font-size: 0.875rem;
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

.alert-danger {
  background-color: rgba(220, 53, 69, 0.1) !important;
  border-color: rgba(220, 53, 69, 0.3) !important;
  color: #f8d7da !important;
}

.toast {
  background: rgba(15, 23, 42, 0.95) !important;
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.toast-header.bg-success {
  background: linear-gradient(135deg, #10b981, #059669) !important;
}

.toast-body {
  color: #fff !important;
}

/* Action buttons styling */
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

/* Responsive improvements */
@media (max-width: 768px) {
  .btn-group .btn {
    min-width: 30px !important;
    height: 30px !important;
    padding: 0.3rem !important;
  }
  
  .btn-group .btn i {
    font-size: 0.8rem;
  }
  
  .table-responsive {
    font-size: 0.875rem;
  }
  
  .avatar {
    width: 35px;
    height: 35px;
    font-size: 0.8rem;
  }
}

@media (max-width: 576px) {
  .btn-group .btn {
    min-width: 28px !important;
    height: 28px !important;
    padding: 0.25rem !important;
    margin-right: 4px;
  }
  
  .btn-group .btn i {
    font-size: 0.75rem;
  }
  
  .avatar {
    width: 30px;
    height: 30px;
    font-size: 0.75rem;
  }
}

/* Container background matching assets page */
.container-fluid {
  background: linear-gradient(135deg, #0f172a, #1e293b, #0f172a);
  min-height: 100vh;
}
</style>