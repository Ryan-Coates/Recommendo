using Microsoft.EntityFrameworkCore;
using Recommendo.Api.Models;

namespace Recommendo.Api.Data;

public class RecommendoContext : DbContext
{
    public RecommendoContext(DbContextOptions<RecommendoContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<InviteLink> InviteLinks { get; set; }
    public DbSet<Recommendation> Recommendations { get; set; }
    public DbSet<RecommendationNote> RecommendationNotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
        });

        // Friendship configuration
        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.FriendshipsInitiated)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Friend)
                .WithMany(u => u.FriendshipsReceived)
                .HasForeignKey(e => e.FriendId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.UserId, e.FriendId }).IsUnique();
        });

        // InviteLink configuration
        modelBuilder.Entity<InviteLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.InviteLinks)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Recommendation configuration
        modelBuilder.Entity<Recommendation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            
            entity.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.RecommendationsCreated)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.RecommendedToUser)
                .WithMany(u => u.RecommendationsReceived)
                .HasForeignKey(e => e.RecommendedToUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RecommendationNote configuration
        modelBuilder.Entity<RecommendationNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Note).IsRequired().HasMaxLength(1000);
            
            entity.HasOne(e => e.Recommendation)
                .WithMany(r => r.Notes)
                .HasForeignKey(e => e.RecommendationId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
