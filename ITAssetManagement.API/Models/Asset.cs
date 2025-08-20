using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAssetManagement.API.Models
{
    public class Asset
    {
        [Key]
        [Column("AssetId")]  // Map DB column AssetId → property Id
        public int Id { get; set; }
        
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(50)]
        public string? AssetTag { get; set; } // Unique identifier
        
        [StringLength(50)]
        public string? Category { get; set; } // Laptop, Desktop, Monitor, etc.
        
        [StringLength(50)]
        public string? Brand { get; set; }
        
        [StringLength(50)]
        public string? Model { get; set; }
        
        [StringLength(100)]
        public string? SerialNumber { get; set; }
        
        [StringLength(20)]
        public string? Status { get; set; } // Available, Assigned, Maintenance, Retired
        
        [StringLength(100)]
        public string? AssignedTo { get; set; } // Employee name
        
        [StringLength(100)]
        public string? Location { get; set; }
        
        public decimal? PurchasePrice { get; set; }
        
        public DateTime? PurchaseDate { get; set; }
        
        public DateTime? WarrantyExpiry { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime? CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        [StringLength(50)]
        public string? CreatedBy { get; set; }
        
        [StringLength(50)]
        public string? UpdatedBy { get; set; }
    }

    public class AssetCreateRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string AssetTag { get; set; } = string.Empty;
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string Status { get; set; } = "Available";
        public string? AssignedTo { get; set; }
        public string? Location { get; set; }
        public decimal? PurchasePrice { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public string? Notes { get; set; }
    }

    public class AssetUpdateRequest : AssetCreateRequest
    {
        public int Id { get; set; }
    }

    public class AssetSearchRequest
    {
        public string? SearchTerm { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public string? AssignedTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string SortOrder { get; set; } = "asc";
    }

    public class AssetListResponse
    {
        public List<Asset> Assets { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}