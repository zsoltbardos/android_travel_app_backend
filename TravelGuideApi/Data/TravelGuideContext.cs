using Microsoft.EntityFrameworkCore;
using TravelGuideApi.Models;

namespace TravelGuideApi.Data
{
    public class TravelGuideContext : DbContext
    {
        public TravelGuideContext(DbContextOptions<TravelGuideContext> options)
            : base(options)
        {
        }

        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserFavoriteDestination> UserFavoriteDestinations { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Destination)
                .WithMany(d => d.Reviews)
                .HasForeignKey(r => r.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserFavoriteDestination>()
                .HasOne(ufd => ufd.User)
                .WithMany(u => u.FavoriteDestinations)
                .HasForeignKey(ufd => ufd.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFavoriteDestination>()
                .HasOne(ufd => ufd.Destination)
                .WithMany(d => d.FavoritedByUsers)
                .HasForeignKey(ufd => ufd.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 