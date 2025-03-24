using System;
using System.Collections.Generic;

namespace TravelGuideApi.DTOs
{
    public class DestinationDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public float AverageRating { get; set; }
        public List<string> PointsOfInterest { get; set; }
        public List<string> ImageUrls { get; set; }
        public string CoverImageUrl { get; set; }
        public string Region { get; set; }
        public string Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class DestinationCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public List<string> PointsOfInterest { get; set; }
        public List<string> ImageUrls { get; set; }
        public string CoverImageUrl { get; set; }
        public string Region { get; set; }
        public string Category { get; set; }
    }

    public class DestinationUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public List<string> PointsOfInterest { get; set; }
        public List<string> ImageUrls { get; set; }
        public string CoverImageUrl { get; set; }
        public string Region { get; set; }
        public string Category { get; set; }
    }
} 