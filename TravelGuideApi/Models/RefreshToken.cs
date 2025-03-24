using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelGuideApi.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Token { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public DateTime Expires { get; set; }
        
        public bool IsRevoked { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
} 