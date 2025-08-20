using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ITAssetManagement.API.Data;
using ITAssetManagement.API.Models;
using BCrypt.Net;
using System.Text;

namespace ITAssetManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<UserListResponse>> GetUsers([FromQuery] UserSearchRequest request)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                // Apply search filters
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(u => 
                        u.FirstName.ToLower().Contains(searchTerm) ||
                        u.LastName.ToLower().Contains(searchTerm) ||
                        u.Username.ToLower().Contains(searchTerm) ||
                        u.Email.ToLower().Contains(searchTerm) ||
                        (u.Department != null && u.Department.ToLower().Contains(searchTerm)));
                }

                if (!string.IsNullOrEmpty(request.Role))
                {
                    query = query.Where(u => u.Role == request.Role);
                }

                if (!string.IsNullOrEmpty(request.Department))
                {
                    query = query.Where(u => u.Department == request.Department);
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    query = ApplyUserSorting(query, request.SortBy, request.SortOrder);
                }
                else
                {
                    query = query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName);
                }

                // Filter active users
                query = query.Where(u => u.IsActive);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                var users = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(u => new UserResponse
                    {
                        Id = u.Id,
                        Username = u.Username,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Role = u.Role,
                        Department = u.Department,
                        Notes = u.Notes,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt,
                        AssignedAssets = _context.Assets.Count(a => a.AssignedTo == u.Username && a.Status == "Assigned")
                    })
                    .ToListAsync();

                return Ok(new UserListResponse 
                { 
                    Users = users,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex}");
                return StatusCode(500, new { message = "Error fetching users", details = ex.Message });
            }
        }

        private IQueryable<User> ApplyUserSorting(IQueryable<User> query, string sortBy, string sortOrder)
        {
            var isAscending = sortOrder?.ToLower() != "desc";
            
            return sortBy?.ToLower() switch
            {
                "firstname" => isAscending ? query.OrderBy(u => u.FirstName) : query.OrderByDescending(u => u.FirstName),
                "lastname" => isAscending ? query.OrderBy(u => u.LastName) : query.OrderByDescending(u => u.LastName),
                "username" => isAscending ? query.OrderBy(u => u.Username) : query.OrderByDescending(u => u.Username),
                "email" => isAscending ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                "role" => isAscending ? query.OrderBy(u => u.Role) : query.OrderByDescending(u => u.Role),
                "department" => isAscending ? query.OrderBy(u => u.Department ?? "") : query.OrderByDescending(u => u.Department ?? ""),
                "createdat" => isAscending ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
                _ => query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            };
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetailResponse>> GetUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == id && u.IsActive)
                    .Select(u => new UserDetailResponse
                    {
                        Id = u.Id,
                        Username = u.Username,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Role = u.Role,
                        Department = u.Department,
                        Notes = u.Notes,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt,
                        AssignedAssets = _context.Assets
                            .Where(a => a.AssignedTo == u.Username && a.Status == "Assigned")
                            .Select(a => new UserAssetResponse
                            {
                                Id = a.Id,
                                Name = a.Name,
                                AssetTag = a.AssetTag,
                                Category = a.Category,
                                Brand = a.Brand,
                                Model = a.Model,
                                SerialNumber = a.SerialNumber,
                                Status = a.Status,
                                Location = a.Location
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user {id}: {ex}");
                return StatusCode(500, new { message = "Error fetching user", details = ex.Message });
            }
        }

        // GET: api/users/departments
        [HttpGet("departments")]
        public async Task<ActionResult<List<string>>> GetDepartments()
        {
            try
            {
                var departments = await _context.Users
                    .Where(u => !string.IsNullOrEmpty(u.Department) && u.IsActive)
                    .Select(u => u.Department)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToListAsync();

                return Ok(departments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching departments: {ex}");
                return StatusCode(500, new { message = "Error fetching departments", details = ex.Message });
            }
        }

        // GET: api/users/export
        [HttpGet("export")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ExportUsers([FromQuery] UserSearchRequest request)
        {
            try
            {
                var query = _context.Users.Where(u => u.IsActive).AsQueryable();

                // Apply same filters as GetUsers but without pagination
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(u => 
                        u.FirstName.ToLower().Contains(searchTerm) ||
                        u.LastName.ToLower().Contains(searchTerm) ||
                        u.Username.ToLower().Contains(searchTerm) ||
                        u.Email.ToLower().Contains(searchTerm) ||
                        (u.Department != null && u.Department.ToLower().Contains(searchTerm)));
                }

                if (!string.IsNullOrEmpty(request.Role))
                {
                    query = query.Where(u => u.Role == request.Role);
                }

                if (!string.IsNullOrEmpty(request.Department))
                {
                    query = query.Where(u => u.Department == request.Department);
                }

                var users = await query
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToListAsync();

                // Generate CSV
                var csv = new StringBuilder();
                csv.AppendLine("First Name,Last Name,Username,Email,Role,Department,Assigned Assets,Created At,Notes");
                
                foreach (var user in users)
                {
                    var assignedAssetCount = await _context.Assets
                        .CountAsync(a => a.AssignedTo == user.Username && a.Status == "Assigned");
                    
                    csv.AppendLine($"\"{user.FirstName}\",\"{user.LastName}\",\"{user.Username}\",\"{user.Email}\",\"{user.Role}\",\"{user.Department ?? ""}\",\"{assignedAssetCount}\",\"{user.CreatedAt:yyyy-MM-dd HH:mm:ss}\",\"{user.Notes?.Replace("\"", "\"\"") ?? ""}\"");
                }

                return Content(csv.ToString(), "text/csv");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting users: {ex}");
                return StatusCode(500, new { message = "Error exporting users", details = ex.Message });
            }
        }

        // POST: api/users
        [HttpPost]
        [Authorize(Roles = "admin")] // Only admins can register new users
        public async Task<ActionResult<UserResponse>> CreateUser(CreateUserRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.Username) || 
                    string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.FirstName) ||
                    string.IsNullOrWhiteSpace(request.LastName) ||
                    string.IsNullOrWhiteSpace(request.Role))
                {
                    return BadRequest(new { message = "All required fields must be provided" });
                }

                // Check if username already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);
                
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Username or email already exists" });
                }

                // Validate role
                var validRoles = new[] { "admin", "manager", "user" };
                if (!validRoles.Contains(request.Role.ToLower()))
                {
                    return BadRequest(new { message = "Invalid role. Must be admin, manager, or user" });
                }

                // Create new user
                var user = new User
                {
                    Username = request.Username,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Role = request.Role.ToLower(),
                    Department = request.Department,
                    Notes = request.Notes,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Return user response (without password)
                var userResponse = new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    Department = user.Department,
                    Notes = user.Notes,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    AssignedAssets = 0 // New user has no assets assigned yet
                };

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex}");
                return StatusCode(500, new { message = "Error creating user", details = ex.Message });
            }
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null || !user.IsActive)
                {
                    return NotFound();
                }

                // Update user properties
                if (!string.IsNullOrWhiteSpace(request.FirstName))
                    user.FirstName = request.FirstName;
                
                if (!string.IsNullOrWhiteSpace(request.LastName))
                    user.LastName = request.LastName;
                
                if (!string.IsNullOrWhiteSpace(request.Email))
                    user.Email = request.Email;
                
                if (!string.IsNullOrWhiteSpace(request.Role))
                {
                    var validRoles = new[] { "admin", "manager", "user" };
                    if (validRoles.Contains(request.Role.ToLower()))
                    {
                        user.Role = request.Role.ToLower();
                    }
                }

                if (request.Department != null)
                    user.Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department;

                if (request.Notes != null)
                    user.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex}");
                return StatusCode(500, new { message = "Error updating user", details = ex.Message });
            }
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Check if user has assigned assets
                var hasAssignedAssets = await _context.Assets
                    .AnyAsync(a => a.AssignedTo == user.Username && a.Status == "Assigned");

                if (hasAssignedAssets)
                {
                    return BadRequest(new { message = "Cannot delete user with assigned assets. Please reassign assets first." });
                }

                // Soft delete - just set IsActive to false
                user.IsActive = false;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user: {ex}");
                return StatusCode(500, new { message = "Error deleting user", details = ex.Message });
            }
        }

        // DELETE: api/users/bulk-delete
        [HttpDelete("bulk-delete")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> BulkDeleteUsers(BulkDeleteUsersRequest request)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => request.UserIds.Contains(u.Id) && u.IsActive)
                    .ToListAsync();

                if (!users.Any())
                {
                    return NotFound("No users found to delete");
                }

                // Check if any users have assigned assets
                var usersWithAssets = new List<string>();
                foreach (var user in users)
                {
                    var hasAssignedAssets = await _context.Assets
                        .AnyAsync(a => a.AssignedTo == user.Username && a.Status == "Assigned");
                    
                    if (hasAssignedAssets)
                    {
                        usersWithAssets.Add($"{user.FirstName} {user.LastName}");
                    }
                }

                if (usersWithAssets.Any())
                {
                    return BadRequest(new { 
                        message = "Cannot delete users with assigned assets. Please reassign assets first.",
                        usersWithAssets = usersWithAssets
                    });
                }

                // Soft delete users
                foreach (var user in users)
                {
                    user.IsActive = false;
                }

                await _context.SaveChangesAsync();

                return Ok(new { deletedCount = users.Count });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error bulk deleting users: {ex}");
                return StatusCode(500, new { message = "Error deleting users", details = ex.Message });
            }
        }
    }

    // Response DTOs
    public class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AssignedAssets { get; set; }
    }

    public class UserDetailResponse : UserResponse
    {
        public new List<UserAssetResponse> AssignedAssets { get; set; } = new();
    }

    public class UserAssetResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AssetTag { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Location { get; set; }
    }

    public class UserListResponse
    {
        public List<UserResponse> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    // Request DTOs
    public class UserSearchRequest
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public string? Department { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; } = "asc";
    }

    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Department { get; set; }
        public string? Notes { get; set; }
    }

    public class BulkDeleteUsersRequest
    {
        public List<int> UserIds { get; set; } = new();
    }
}