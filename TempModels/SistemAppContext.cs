using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SistemApp.TempModels;

public partial class SistemAppContext : DbContext
{
    public SistemAppContext(DbContextOptions<SistemAppContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Domain> Domains { get; set; }

    public virtual DbSet<EquipmentType> EquipmentTypes { get; set; }

    public virtual DbSet<InventoryAssignmentHistory> InventoryAssignmentHistories { get; set; }

    public virtual DbSet<InventoryItem> InventoryItems { get; set; }

    public virtual DbSet<MailEntry> MailEntries { get; set; }

    public virtual DbSet<MailPasswordHistory> MailPasswordHistories { get; set; }

    public virtual DbSet<PasswordHistory> PasswordHistories { get; set; }

    public virtual DbSet<Personnel> Personnel { get; set; }

    public virtual DbSet<Site> Sites { get; set; }

    public virtual DbSet<SystemHardware> SystemHardwares { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex").IsUnique();
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex").IsUnique();

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Domain>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_Domains_CompanyId");

            entity.HasOne(d => d.Company).WithMany(p => p.Domains).HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<InventoryAssignmentHistory>(entity =>
        {
            entity.ToTable("InventoryAssignmentHistory");

            entity.HasIndex(e => e.InventoryItemId, "IX_InventoryAssignmentHistory_InventoryItemId");

            entity.HasIndex(e => e.PersonnelId, "IX_InventoryAssignmentHistory_PersonnelId");

            entity.HasOne(d => d.InventoryItem).WithMany(p => p.InventoryAssignmentHistories).HasForeignKey(d => d.InventoryItemId);

            entity.HasOne(d => d.Personnel).WithMany(p => p.InventoryAssignmentHistories).HasForeignKey(d => d.PersonnelId);
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasIndex(e => e.EquipmentTypeId, "IX_InventoryItems_EquipmentTypeId");

            entity.HasIndex(e => e.PersonnelId, "IX_InventoryItems_PersonnelId");

            entity.HasIndex(e => e.SerialNumber, "IX_InventoryItems_SerialNumber").IsUnique();

            entity.HasIndex(e => e.SiteId1, "IX_InventoryItems_SiteId1");

            entity.HasOne(d => d.EquipmentType).WithMany(p => p.InventoryItems)
                .HasForeignKey(d => d.EquipmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Personnel).WithMany(p => p.InventoryItems)
                .HasForeignKey(d => d.PersonnelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.SiteId1Navigation).WithMany(p => p.InventoryItems).HasForeignKey(d => d.SiteId1);
        });

        modelBuilder.Entity<MailEntry>(entity =>
        {
            entity.HasIndex(e => e.DomainId, "IX_MailEntries_DomainId");

            entity.HasIndex(e => e.PersonnelId, "IX_MailEntries_PersonnelId");

            entity.HasIndex(e => e.SiteId1, "IX_MailEntries_SiteId1");

            entity.HasOne(d => d.Domain).WithMany(p => p.MailEntries)
                .HasForeignKey(d => d.DomainId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Personnel).WithMany(p => p.MailEntries)
                .HasForeignKey(d => d.PersonnelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.SiteId1Navigation).WithMany(p => p.MailEntries).HasForeignKey(d => d.SiteId1);
        });

        modelBuilder.Entity<MailPasswordHistory>(entity =>
        {
            entity.ToTable("MailPasswordHistory");

            entity.HasIndex(e => e.MailEntryId, "IX_MailPasswordHistory_MailEntryId");

            entity.HasOne(d => d.MailEntry).WithMany(p => p.MailPasswordHistories).HasForeignKey(d => d.MailEntryId);
        });

        modelBuilder.Entity<PasswordHistory>(entity =>
        {
            entity.ToTable("PasswordHistory");

            entity.HasIndex(e => e.SystemHardwareId, "IX_PasswordHistory_SystemHardwareId");

            entity.HasOne(d => d.SystemHardware).WithMany(p => p.PasswordHistories).HasForeignKey(d => d.SystemHardwareId);
        });

        modelBuilder.Entity<Personnel>(entity =>
        {
            entity.HasIndex(e => e.SiteId, "IX_Personnel_SiteId");

            entity.HasOne(d => d.Site).WithMany(p => p.Personnel)
                .HasForeignKey(d => d.SiteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Site>(entity =>
        {
            entity.HasIndex(e => e.CompanyId, "IX_Sites_CompanyId");

            entity.HasOne(d => d.Company).WithMany(p => p.Sites)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SystemHardware>(entity =>
        {
            entity.ToTable("SystemHardware");

            entity.HasIndex(e => e.CompanyId, "IX_SystemHardware_CompanyId");

            entity.HasIndex(e => e.EquipmentTypeId, "IX_SystemHardware_EquipmentTypeId");

            entity.HasIndex(e => e.SiteId, "IX_SystemHardware_SiteId");

            entity.HasOne(d => d.Company).WithMany(p => p.SystemHardwares)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.EquipmentType).WithMany(p => p.SystemHardwares)
                .HasForeignKey(d => d.EquipmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Site).WithMany(p => p.SystemHardwares)
                .HasForeignKey(d => d.SiteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
