using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ITAssetManagement.API.Data;
using ITAssetManagement.API.Models;

namespace ITAssetManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/dashboard/stats
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStats>> GetStats()
        {
            try
            {
                var totalAssets = await _context.Assets.CountAsync();
                var availableAssets = await _context.Assets.CountAsync(a =>
                    a.Status == "Unassigned" || a.Status == "Available");
                var inMaintenance = await _context.Assets.CountAsync(a =>
                    a.Status == "Needs Repair" || a.Status == "Maintenance");
                var assignedAssets = await _context.Assets.CountAsync(a =>
                    a.Status == "Assigned");

                var stats = new DashboardStats
                {
                    TotalAssets = totalAssets,
                    AvailableAssets = availableAssets,
                    InMaintenance = inMaintenance,
                    AssignedAssets = assignedAssets
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching dashboard stats", error = ex.Message });
            }
        }

        // GET: api/dashboard/recent-activity
        [HttpGet("recent-activity")]
        public async Task<ActionResult<List<ActivityLog>>> GetRecentActivity()
        {
            try
            {
                var recentAssets = await _context.Assets
                    .Where(a => a.UpdatedAt != null || a.CreatedAt >= DateTime.Now.AddDays(-30))
                    .OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt)
                    .Take(10)
                    .Select(a => new ActivityLog
                    {
                        Id = a.Id,
                        Type = a.UpdatedAt != null ? "asset_updated" : "asset_created",
                        Description = a.UpdatedAt != null
                            ? $"Asset '{a.Name}' was updated"
                            : $"Asset '{a.Name}' was created",
                        // Fix: ensure Timestamp is non-null
                        Timestamp = a.UpdatedAt ?? a.CreatedAt ?? DateTime.Now
                    })
                    .ToListAsync();

                return Ok(recentAssets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching recent activity", error = ex.Message });
            }
        }

        // GET: api/dashboard/asset-status-breakdown
        [HttpGet("asset-status-breakdown")]
        public async Task<ActionResult<List<AssetStatusBreakdown>>> GetAssetStatusBreakdown()
        {
            try
            {
                var breakdown = await _context.Assets
                    .GroupBy(a => a.Status)
                    .Select(g => new AssetStatusBreakdown
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Status)
                    .ToListAsync();

                return Ok(breakdown);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching asset status breakdown", error = ex.Message });
            }
        }

        // GET: api/dashboard/category-breakdown
        [HttpGet("category-breakdown")]
        public async Task<ActionResult<List<AssetCategoryBreakdown>>> GetCategoryBreakdown()
        {
            try
            {
                var breakdown = await _context.Assets
                    .GroupBy(a => a.Category)
                    .Select(g => new AssetCategoryBreakdown
                    {
                        Category = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Category)
                    .ToListAsync();

                return Ok(breakdown);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching category breakdown", error = ex.Message });
            }
        }
    }

    // Response models for dashboard
    public class DashboardStats
    {
        public int TotalAssets { get; set; }
        public int AvailableAssets { get; set; }
        public int InMaintenance { get; set; }
        public int AssignedAssets { get; set; }
    }

    public class ActivityLog
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class AssetStatusBreakdown
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class AssetCategoryBreakdown
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
