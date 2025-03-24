using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelGuideApi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }
        
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [MaxLength(50)]
        public string FirstName { get; set; }
        
        [MaxLength(50)]
        public string LastName { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<Review> Reviews { get; set; }
        
        // Many-to-Many relationship with Destination
        public virtual ICollection<UserFavoriteDestination> FavoriteDestinations { get; set; }
    }
} 