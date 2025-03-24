using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelGuideApi.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int DestinationId { get; set; }
        
        public int? UserId { get; set; }
        
        [MaxLength(100)]
        public string ReviewerName { get; set; }
        
        [Required]
        public string ReviewText { get; set; }
        
        [Required]
        [Range(1, 5)]
        public float Rating { get; set; }
        
        public DateTime ReviewDate { get; set; }
        
        // Navigation properties
        [ForeignKey("DestinationId")]
        public virtual Destination Destination { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
} 