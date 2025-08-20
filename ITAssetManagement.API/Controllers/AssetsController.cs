using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ITAssetManagement.API.Data;
using ITAssetManagement.API.Models;
using System.Security.Claims;
using System.Linq.Expressions;

namespace ITAssetManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssetsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AssetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/assets
        [HttpGet]
        public async Task<ActionResult<AssetListResponse>> GetAssets([FromQuery] AssetSearchRequest request)
        {
            try
            {
                var query = _context.Assets.AsQueryable();

                // Apply search filters
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(a => 
                        (a.Name != null && a.Name.ToLower().Contains(searchTerm)) ||
                        (a.AssetTag != null && a.AssetTag.ToLower().Contains(searchTerm)) ||
                        (a.Brand != null && a.Brand.ToLower().Contains(searchTerm)) ||
                        (a.Model != null && a.Model.ToLower().Contains(searchTerm)) ||
                        (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(searchTerm)) ||
                        (a.AssignedTo != null && a.AssignedTo.ToLower().Contains(searchTerm)));
                }

                if (!string.IsNullOrEmpty(request.Category))
                {
                    query = query.Where(a => a.Category == request.Category);
                }

                if (!string.IsNullOrEmpty(request.Status))
                {
                    query = query.Where(a => a.Status == request.Status);
                }

                if (!string.IsNullOrEmpty(request.AssignedTo))
                {
                    if (request.AssignedTo.ToLower() == "unassigned")
                    {
                        query = query.Where(a => string.IsNullOrEmpty(a.AssignedTo));
                    }
                    else
                    {
                        query = query.Where(a => a.AssignedTo != null && a.AssignedTo.Contains(request.AssignedTo));
                    }
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    query = ApplySorting(query, request.SortBy, request.SortOrder);
                }
                else
                {
                    // Default sorting - handle null CreatedAt values
                    query = query.OrderByDescending(a => a.CreatedAt ?? DateTime.MinValue);
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                var assets = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return Ok(new AssetListResponse
                {
                    Assets = assets,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching assets: {ex}");
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
                return StatusCode(500, new { 
                    message = "Error fetching assets", 
                    details = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        private IQueryable<Asset> ApplySorting(IQueryable<Asset> query, string sortBy, string sortOrder)
        {
            var isAscending = sortOrder?.ToLower() != "desc";
            
            return sortBy?.ToLower() switch
            {
                "name" => isAscending ? query.OrderBy(a => a.Name ?? "") : query.OrderByDescending(a => a.Name ?? ""),
                "category" => isAscending ? query.OrderBy(a => a.Category ?? "") : query.OrderByDescending(a => a.Category ?? ""),
                "status" => isAscending ? query.OrderBy(a => a.Status ?? "") : query.OrderByDescending(a => a.Status ?? ""),
                "assignedto" => isAscending ? query.OrderBy(a => a.AssignedTo ?? "") : query.OrderByDescending(a => a.AssignedTo ?? ""),
                "createdat" => isAscending ? query.OrderBy(a => a.CreatedAt ?? DateTime.MinValue) : query.OrderByDescending(a => a.CreatedAt ?? DateTime.MinValue),
                _ => query.OrderByDescending(a => a.CreatedAt ?? DateTime.MinValue)
            };
        }

        // GET: api/assets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Asset>> GetAsset(int id)
        {
            try
            {
                var asset = await _context.Assets.FindAsync(id);

                if (asset == null)
                {
                    return NotFound();
                }

                return asset;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching asset {id}: {ex}");
                return StatusCode(500, new { message = "Error fetching asset", details = ex.Message });
            }
        }

        // POST: api/assets
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Asset>> CreateAsset(AssetCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                
                return BadRequest(new { message = "Validation failed", errors });
            }

            try
            {
                // Check if asset tag already exists
                if (await _context.Assets.AnyAsync(a => a.AssetTag == request.AssetTag))
                {
                    return BadRequest(new { message = "Asset tag already exists" });
                }

                var currentUser = User.Identity?.Name ?? "unknown";

                var asset = new Asset
                {
                    Name = request.Name?.Trim() ?? "",
                    AssetTag = request.AssetTag?.Trim() ?? "",
                    Category = request.Category ?? "",
                    Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim(),
                    Model = string.IsNullOrWhiteSpace(request.Model) ? null : request.Model.Trim(),
                    SerialNumber = string.IsNullOrWhiteSpace(request.SerialNumber) ? null : request.SerialNumber.Trim(),
                    Status = request.Status ?? "Available",
                    AssignedTo = string.IsNullOrWhiteSpace(request.AssignedTo) ? null : request.AssignedTo.Trim(),
                    Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
                    PurchasePrice = request.PurchasePrice,
                    PurchaseDate = request.PurchaseDate,
                    WarrantyExpiry = request.WarrantyExpiry,
                    Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                    CreatedBy = currentUser,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Assets.Add(asset);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, asset);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"DbUpdateException: {ex}");
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
                
                return StatusCode(500, new { 
                    message = "Database error occurred", 
                    details = ex.InnerException?.Message ?? ex.Message 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/assets/check-tag/{assetTag}
        [HttpGet("check-tag/{assetTag}")]
        public async Task<ActionResult<object>> CheckAssetTag(string assetTag)
        {
            try
            {
                var exists = await _context.Assets.AnyAsync(a => a.AssetTag == assetTag);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking asset tag: {ex}");
                return StatusCode(500, new { message = "Error checking asset tag", details = ex.Message });
            }
        }

        // PUT: api/assets/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateAsset(int id, AssetUpdateRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                var asset = await _context.Assets.FindAsync(id);
                if (asset == null)
                {
                    return NotFound();
                }

                // Check if asset tag already exists for another asset
                if (await _context.Assets.AnyAsync(a => a.AssetTag == request.AssetTag && a.Id != id))
                {
                    return BadRequest(new { message = "Asset tag already exists" });
                }

                var currentUser = User.Identity?.Name ?? "unknown";

                // Update properties
                asset.Name = request.Name?.Trim() ?? "";
                asset.AssetTag = request.AssetTag?.Trim() ?? "";
                asset.Category = request.Category ?? "";
                asset.Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim();
                asset.Model = string.IsNullOrWhiteSpace(request.Model) ? null : request.Model.Trim();
                asset.SerialNumber = string.IsNullOrWhiteSpace(request.SerialNumber) ? null : request.SerialNumber.Trim();
                asset.Status = request.Status ?? "Available";
                asset.AssignedTo = string.IsNullOrWhiteSpace(request.AssignedTo) ? null : request.AssignedTo.Trim();
                asset.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
                asset.PurchasePrice = request.PurchasePrice;
                asset.PurchaseDate = request.PurchaseDate;
                asset.WarrantyExpiry = request.WarrantyExpiry;
                asset.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
                asset.UpdatedBy = currentUser;
                asset.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await AssetExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating asset: {ex}");
                return StatusCode(500, new { message = "Error updating asset", details = ex.Message });
            }
        }

        // PUT: api/assets/5/assign
        [HttpPut("{id}/assign")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AssignAsset(int id, AssetAssignRequest request)
        {
            try
            {
                var asset = await _context.Assets.FindAsync(id);
                if (asset == null)
                {
                    return NotFound();
                }

                if (asset.Status == "Retired")
                {
                    return BadRequest(new { message = "Cannot assign retired asset" });
                }

                var currentUser = User.Identity?.Name ?? "unknown";

                asset.AssignedTo = string.IsNullOrWhiteSpace(request.AssignedTo) ? null : request.AssignedTo.Trim();
                asset.Status = string.IsNullOrWhiteSpace(request.AssignedTo) ? "Available" : "Assigned";
                asset.UpdatedBy = currentUser;
                asset.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(request.Notes))
                {
                    asset.Notes = string.IsNullOrWhiteSpace(asset.Notes) 
                        ? request.Notes.Trim() 
                        : $"{asset.Notes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {request.Notes.Trim()}";
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error assigning asset: {ex}");
                return StatusCode(500, new { message = "Error assigning asset", details = ex.Message });
            }
        }

        // DELETE: api/assets/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            try
            {
                var asset = await _context.Assets.FindAsync(id);
                if (asset == null)
                {
                    return NotFound();
                }

                _context.Assets.Remove(asset);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting asset: {ex}");
                return StatusCode(500, new { message = "Error deleting asset", details = ex.Message });
            }
        }

        // DELETE: api/assets/bulk-delete
        [HttpDelete("bulk-delete")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> BulkDeleteAssets(BulkDeleteRequest request)
        {
            try
            {
                var assets = await _context.Assets
                    .Where(a => request.AssetIds.Contains(a.Id))
                    .ToListAsync();

                if (!assets.Any())
                {
                    return NotFound("No assets found to delete");
                }

                _context.Assets.RemoveRange(assets);
                await _context.SaveChangesAsync();

                return Ok(new { deletedCount = assets.Count });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error bulk deleting assets: {ex}");
                return StatusCode(500, new { message = "Error deleting assets", details = ex.Message });
            }
        }

        // GET: api/assets/categories
        [HttpGet("categories")]
        public async Task<ActionResult<List<string>>> GetCategories()
        {
            try
            {
                var categories = await _context.Assets
                    .Where(a => !string.IsNullOrEmpty(a.Category))
                    .Select(a => a.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                // Add default categories if none exist
                if (!categories.Any())
                {
                    categories = new List<string> 
                    { 
                        "Laptop", "Desktop", "Monitor", "Printer", "Server", 
                        "Network Equipment", "Mobile Device", "Tablet", "Accessories", "Software", "Other" 
                    };
                }

                return Ok(categories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching categories: {ex}");
                return StatusCode(500, new { message = "Error fetching categories", details = ex.Message });
            }
        }

        // GET: api/assets/statuses
        [HttpGet("statuses")]
        public ActionResult<List<string>> GetStatuses()
        {
            var statuses = new List<string> { "Available", "Assigned", "Maintenance", "Retired" };
            return Ok(statuses);
        }

        // GET: api/assets/export
        [HttpGet("export")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ExportAssets([FromQuery] AssetSearchRequest request)
        {
            try
            {
                var query = _context.Assets.AsQueryable();

                // Apply same filters as GetAssets but without pagination
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(a => 
                        a.Name.ToLower().Contains(searchTerm) ||
                        a.AssetTag.ToLower().Contains(searchTerm) ||
                        (a.Brand != null && a.Brand.ToLower().Contains(searchTerm)) ||
                        (a.Model != null && a.Model.ToLower().Contains(searchTerm)) ||
                        (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(searchTerm)) ||
                        (a.AssignedTo != null && a.AssignedTo.ToLower().Contains(searchTerm)));
                }

                if (!string.IsNullOrEmpty(request.Category))
                {
                    query = query.Where(a => a.Category == request.Category);
                }

                if (!string.IsNullOrEmpty(request.Status))
                {
                    query = query.Where(a => a.Status == request.Status);
                }

                if (!string.IsNullOrEmpty(request.AssignedTo))
                {
                    if (request.AssignedTo.ToLower() == "unassigned")
                    {
                        query = query.Where(a => string.IsNullOrEmpty(a.AssignedTo));
                    }
                    else
                    {
                        query = query.Where(a => a.AssignedTo != null && a.AssignedTo.Contains(request.AssignedTo));
                    }
                }

                var assets = await query.OrderBy(a => a.Name).ToListAsync();

                // Generate CSV
                var csv = "Name,Asset Tag,Category,Brand,Model,Serial Number,Status,Assigned To,Location,Purchase Price,Purchase Date,Warranty Expiry,Notes,Created At\n";
                
                foreach (var asset in assets)
                {
                    csv += $"\"{asset.Name}\",\"{asset.AssetTag}\",\"{asset.Category}\",\"{asset.Brand ?? ""}\",\"{asset.Model ?? ""}\",\"{asset.SerialNumber ?? ""}\",\"{asset.Status}\",\"{asset.AssignedTo ?? ""}\",\"{asset.Location ?? ""}\",\"{asset.PurchasePrice?.ToString("F2") ?? ""}\",\"{asset.PurchaseDate?.ToString("yyyy-MM-dd") ?? ""}\",\"{asset.WarrantyExpiry?.ToString("yyyy-MM-dd") ?? ""}\",\"{asset.Notes?.Replace("\"", "\"\"") ?? ""}\",\"{asset.CreatedAt:yyyy-MM-dd HH:mm:ss}\"\n";
                }

                return Content(csv, "text/csv");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting assets: {ex}");
                return StatusCode(500, new { message = "Error exporting assets", details = ex.Message });
            }
        }

        private async Task<bool> AssetExists(int id)
        {
            return await _context.Assets.AnyAsync(e => e.Id == id);
        }
    }

    // Additional DTOs
    public class AssetAssignRequest
    {
        public string AssignedTo { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class BulkDeleteRequest
    {
        public List<int> AssetIds { get; set; } = new();
    }
}