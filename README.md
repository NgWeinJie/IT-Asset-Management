IT Asset Management System

Overview
A web-based IT Asset Management Tool designed to help IT teams track and manage company assets. The system provides role-based access control, asset management operations, dashboard analytics, and export capabilities.
Features

Core Features (Implemented)
•	Authentication System with Admin and Normal User roles
•	Asset Management with CRUD operations
•	Asset list page with search functionality
•	Asset details view with full specifications
•	Dashboard with charts showing asset metrics
•	Export asset list to Excel format

Additional Features
•	User Management (Admin only)
•	User list export to Excel

Advanced Features (nice-to-have)
•	IT Request Ticket System 

Tools and Technologies Used
Frontend Development:
•	Vue 3 (TypeScript)
•	Nuxt 3 
•	Tailwind CSS for styling
•	Chart.js for data visualization

Backend Development:
•	.NET Core 
•	Microsoft SQL Server

Development Tools:
•	Visual Studio Code / Visual Studio
•	SQL Server Management Studio
•	Git for version control

Setup Instructions
Prerequisites
•	Node.js (v18.0.0 or higher)
•	.NET 8 SDK
•	SQL Server 2019 or later
•	Git

Backend Setup
1.	Clone the repository
https://github.com/NgWeinJie/IT-Asset-Management.git

2.	Install dependencies
dotnet restore
3.	Configure database connection Update appsettings.json with your SQL Server connection string:
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ITAssetManagementDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}

4.	Run database migrations
dotnet ef database update

5.	Start the backend server
dotnet run
Backend will be available at https://localhost:5000

Frontend Setup
1.	Navigate to frontend directory
cd ../ it-asset-management-frontend

2.	Install dependencies
npm install

3.	Start the development server
npm run dev
Frontend will be available at http://localhost:3000

Completed Features List
Authentication
•	Login/logout functionality
•	Role-based access control (Admin/Normal User)

Asset Management
•	Create, read, update, delete assets
•	Asset list with pagination
•	Search and filter assets
•	Asset details view
•	Assign/unassign assets to users

Report
•	Total assets count
•	Assigned vs unassigned assets chart
•	Assets by status visualization
•	Asset distribution by category

Export Functionality
•	Export asset list to Excel
•	Export user list to Excel (Admin only)

User Management (Admin Only)
•	Create, edit, delete users
•	Assign roles to users
•	View user list

Recent Activity
•	Track all asset operations
•	User action history
•	Timestamp logging

API Documentation
Key endpoints:
•	GET /api/assets - Retrieve asset list
•	POST /api/assets - Create new asset
•	PUT /api/assets/{id} - Update asset
•	DELETE /api/assets/{id} - Delete asset
•	POST /api/assets/{id}/assign - Assign asset
•	GET /api/dashboard/metrics - Dashboard data
•	GET /api/assets/export - Export to Excel


# IT Asset Management System - Error Handling Strategy

## 1. Invalid User Input Handling

**Frontend Validation**
- Real-time validation with HTML5 + Vue.js reactive properties
- Immediate visual feedback with custom error styling
- Required field validation before API calls

**Backend Validation**
- ModelState validation with detailed error messages
- Business logic validation (unique asset tags, status transitions)
- Input sanitization (trim whitespace, null handling)

**Key Implementation**
```csharp
if (!ModelState.IsValid) {
    return BadRequest(new { message = "Validation failed", errors });
}

// Unique constraint check
if (await _context.Assets.AnyAsync(a => a.AssetTag == request.AssetTag)) {
    return BadRequest(new { message = "Asset tag already exists" });
}
```

## 2. Asset Update Error Handling

**Database Error Management**
- DbUpdateConcurrencyException handling for concurrent updates
- Automatic transaction rollback on failures
- Entity Framework parameterized queries prevent SQL injection

**Frontend Error Recovery**
- Optimistic UI updates with rollback capability
- Loading states with retry mechanisms
- Clear error messages with actionable guidance

**Key Implementation**
```csharp
try {
    await _context.SaveChangesAsync();
} catch (DbUpdateConcurrencyException) {
    if (!await AssetExists(id)) return NotFound();
    else throw;
} catch (Exception ex) {
    return StatusCode(500, new { message = "Error updating asset" });
}
```

## 3. Unauthorized Access Handling

**JWT Authentication Layer**
- `[Authorize]` attributes on all protected endpoints
- Role-based authorization (`[Authorize(Roles = "admin")]`)
- Automatic token validation and expiration handling

**Security-First Approach**
- Consistent error messages prevent information leakage
- No exposure of sensitive data in error responses
- Comprehensive audit logging for security events

**Frontend Protection**
- Route middleware for authentication checks
- Automatic logout on token expiration
- Dynamic UI rendering based on user roles

**Key Implementation**
```csharp
// Security-conscious error handling
if (user == null || !passwordValid) {
    _logger.LogWarning($"Login failed for username: {request.Username}");
    return Unauthorized(new { message = "Invalid credentials" });
}
```

## Error Response Structure

**Standardized Format**
```json
{
  "message": "User-friendly error message",
  "details": "Technical details for debugging",
  "errors": { "fieldName": ["Field-specific errors"] },
  "statusCode": 400
}
```

## Key System Features

**Multi-Layer Protection**
- Client-side validation → Server-side validation → Database constraints

**Graceful Error Recovery**
- Retry mechanisms for transient failures
- User-friendly error messages with recovery options

**Comprehensive Logging**
- All errors logged with full context
- Security events tracked for audit compliance

**User Experience Focus**
- Toast notifications for success/error feedback
- Loading states during operations
- Contextual help for error resolution

**Security Best Practices**
- No sensitive data exposure in errors
- Rate limiting prevents brute force attacks
- Consistent error messaging prevents enumeration
