using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelGuideApi.Models
{
    public class Destination
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Location { get; set; }
        
        public float AverageRating { get; set; }
        
        public string PointsOfInterest { get; set; } // Stored as JSON
        
        public string ImageUrls { get; set; } // Stored as JSON
        
        public string CoverImageUrl { get; set; }
        
        [MaxLength(50)]
        public string Region { get; set; }
        
        [MaxLength(50)]
        public string Category { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<Review> Reviews { get; set; }
        
        // Many-to-Many relationship with User
        public virtual ICollection<UserFavoriteDestination> FavoritedByUsers { get; set; }
    }
} 