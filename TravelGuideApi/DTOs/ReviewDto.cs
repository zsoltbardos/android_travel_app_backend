using System;

namespace TravelGuideApi.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int DestinationId { get; set; }
        public int? UserId { get; set; }
        public string ReviewerName { get; set; }
        public string ReviewText { get; set; }
        public float Rating { get; set; }
        public DateTime ReviewDate { get; set; }
        public string DestinationName { get; set; }
    }

    public class ReviewCreateDto
    {
        public int DestinationId { get; set; }
        public string ReviewerName { get; set; } // Used for anonymous reviews
        public string ReviewText { get; set; }
        public float Rating { get; set; }
    }

    public class ReviewUpdateDto
    {
        public string ReviewText { get; set; }
        public float Rating { get; set; }
    }
} 