using Microsoft.EntityFrameworkCore;
using TravelGuideApi.Data;

namespace TravelGuideApi
{
    public static class DatabaseInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<TravelGuideContext>();
                    // Apply any pending migrations
                    context.Database.Migrate();
                    
                    // Seed the database if needed
                    SeedDatabase(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while initializing the database.");
                    throw;
                }
            }
        }

        private static void SeedDatabase(TravelGuideContext context)
        {
            // Check if database already has data
            if (context.Destinations.Any())
            {
                return; // Database has been seeded
            }

            // Add seed data
            var destinations = new[]
            {
                new TravelGuideApi.Models.Destination
                {
                    Name = "Tokyo",
                    Description = "Tokyo is Japan's capital and the world's most populous metropolis.",
                    Location = "Japan",
                    CoverImageUrl = "https://example.com/tokyo.jpg",
                    ImageUrls = "[\"https://example.com/tokyo1.jpg\", \"https://example.com/tokyo2.jpg\"]",
                    PointsOfInterest = "[\"Tokyo Tower\", \"Shinjuku Gyoen\", \"Meiji Shrine\"]",
                    Region = "Kanto",
                    Category = "City",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new TravelGuideApi.Models.Destination
                {
                    Name = "Paris",
                    Description = "Paris, France's capital, is a major European city and a global center for art, fashion, gastronomy and culture.",
                    Location = "France",
                    CoverImageUrl = "https://example.com/paris.jpg",
                    ImageUrls = "[\"https://example.com/paris1.jpg\", \"https://example.com/paris2.jpg\"]",
                    PointsOfInterest = "[\"Eiffel Tower\", \"Louvre Museum\", \"Notre-Dame Cathedral\"]",
                    Region = "Île-de-France",
                    Category = "City",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new TravelGuideApi.Models.Destination
                {
                    Name = "New York City",
                    Description = "New York City comprises 5 boroughs sitting where the Hudson River meets the Atlantic Ocean.",
                    Location = "USA",
                    CoverImageUrl = "https://example.com/nyc.jpg",
                    ImageUrls = "[\"https://example.com/nyc1.jpg\", \"https://example.com/nyc2.jpg\"]",
                    PointsOfInterest = "[\"Statue of Liberty\", \"Central Park\", \"Empire State Building\"]",
                    Region = "New York State",
                    Category = "City",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.Destinations.AddRange(destinations);
            context.SaveChanges();
        }
    }
} 