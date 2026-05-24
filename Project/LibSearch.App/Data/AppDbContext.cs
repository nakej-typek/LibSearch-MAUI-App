using LibSearch.App.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibSearch.App.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<TextDocument> Documents => Set<TextDocument>();
    public DbSet<SearchHistoryItem> SearchHistory => Set<SearchHistoryItem>();
    public DbSet<SavedPassage> SavedPassages => Set<SavedPassage>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<SavedPassageTag> SavedPassageTags => Set<SavedPassageTag>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<TextDocument>()
            .HasOne(d => d.Owner)
            .WithMany(u => u.Documents)
            .HasForeignKey(d => d.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SearchHistoryItem>()
            .HasOne(h => h.Owner)
            .WithMany(u => u.SearchHistory)
            .HasForeignKey(h => h.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SearchHistoryItem>()
            .HasOne(h => h.Document)
            .WithMany(d => d.SearchHistory)
            .HasForeignKey(h => h.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SavedPassage>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.SavedPassages)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SavedPassage>()
            .HasOne(p => p.Document)
            .WithMany(d => d.SavedPassages)
            .HasForeignKey(p => p.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tag>()
            .HasOne(t => t.Owner)
            .WithMany(u => u.Tags)
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tag>()
            .HasIndex(t => new { t.OwnerId, t.Name })
            .IsUnique();

        modelBuilder.Entity<SavedPassageTag>()
            .HasKey(pt => new { pt.SavedPassageId, pt.TagId });

        modelBuilder.Entity<SavedPassageTag>()
            .HasOne(pt => pt.SavedPassage)
            .WithMany(p => p.Tags)
            .HasForeignKey(pt => pt.SavedPassageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SavedPassageTag>()
            .HasOne(pt => pt.Tag)
            .WithMany(t => t.SavedPassages)
            .HasForeignKey(pt => pt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
