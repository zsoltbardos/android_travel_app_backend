using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelGuideApi.Models
{
    public class UserFavoriteDestination
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int DestinationId { get; set; }
        
        public DateTime AddedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        [ForeignKey("DestinationId")]
        public virtual Destination Destination { get; set; }
    }
} 