using AssetManagement.Infrastructure.Identity;
using AssetManagement.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Building> Buildings { get; set; }
    public DbSet<Floor> Floors { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<TicketHistory> TicketHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename Identity tables (remove AspNet prefix)
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

        // Asset - unique index on QRCode
        builder.Entity<Asset>(entity =>
        {
            entity.HasIndex(a => a.QRCode).IsUnique();
            entity.HasOne(a => a.Building)
                  .WithMany(b => b.Assets)
                  .HasForeignKey(a => a.BuildingId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Floor)
                  .WithMany(f => f.Assets)
                  .HasForeignKey(a => a.FloorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Floor - building relationship
        builder.Entity<Floor>(entity =>
        {
            entity.HasOne(f => f.Building)
                  .WithMany(b => b.Floors)
                  .HasForeignKey(f => f.BuildingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Ticket - indexes and relationships
        builder.Entity<Ticket>(entity =>
        {
            entity.HasIndex(t => t.TicketNumber).IsUnique();
            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.CreatedByUserId);
            entity.HasIndex(t => t.DueAt);
            entity.Property(t => t.ActualCost).HasPrecision(10, 2);

            entity.HasOne(t => t.Asset)
                  .WithMany(a => a.Tickets)
                  .HasForeignKey(t => t.AssetId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.AssignedVendor)
                  .WithMany(v => v.Tickets)
                  .HasForeignKey(t => t.AssignedVendorId)
                  .OnDelete(DeleteBehavior.SetNull);

            // No EF nav for user FKs (string IDs pointing to Identity table)
        });

        // Attachment - ticket relationship
        builder.Entity<Attachment>(entity =>
        {
            entity.HasOne(a => a.Ticket)
                  .WithMany(t => t.Attachments)
                  .HasForeignKey(a => a.TicketId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TicketHistory - ticket relationship
        builder.Entity<TicketHistory>(entity =>
        {
            entity.HasIndex(h => h.TicketId);
            entity.HasOne(h => h.Ticket)
                  .WithMany(t => t.History)
                  .HasForeignKey(h => h.TicketId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
