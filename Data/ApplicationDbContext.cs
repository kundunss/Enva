
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemApp.Models;

namespace SistemApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<Personnel> Personnel { get; set; }
        public DbSet<EquipmentType> EquipmentTypes { get; set; }
        public DbSet<Domain> Domains { get; set; }
        public DbSet<SystemHardware> SystemHardware { get; set; }
        public DbSet<PasswordHistory> PasswordHistory { get; set; }
        public DbSet<MailEntry> MailEntries { get; set; }
        public DbSet<MailPasswordHistory> MailPasswordHistory { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<InventoryAssignmentHistory> InventoryAssignmentHistory { get; set; }
        public DbSet<ServiceEntry> ServiceEntries { get; set; }
        public DbSet<PayEntry> PayEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ServiceEntry-Company ilişkisinde Cascade silme
            modelBuilder.Entity<ServiceEntry>()
                .HasOne(s => s.Company)
                .WithMany(c => c.ServiceEntries)
                .HasForeignKey(s => s.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure cascade delete behavior
            modelBuilder.Entity<Company>()
                .HasMany(c => c.Sites)
                .WithOne(s => s.Company)
                .HasForeignKey(s => s.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Company>()
                .HasMany(c => c.SystemHardware)
                .WithOne(h => h.Company)
                .HasForeignKey(h => h.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Site>()
                .HasMany(s => s.Personnel)
                .WithOne(p => p.Site)
                .HasForeignKey(p => p.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Site>()
                .HasMany(s => s.SystemHardware)
                .WithOne(h => h.Site)
                .HasForeignKey(h => h.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EquipmentType>()
                .HasMany(e => e.SystemHardware)
                .WithOne(h => h.EquipmentType)
                .HasForeignKey(h => h.EquipmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EquipmentType>()
                .HasMany(e => e.InventoryItems)
                .WithOne(i => i.EquipmentType)
                .HasForeignKey(i => i.EquipmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Personnel>()
                .HasMany(p => p.MailEntries)
                .WithOne(m => m.Personnel)
                .HasForeignKey(m => m.PersonnelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Personnel>()
                .HasMany(p => p.AssignedInventoryItems)
                .WithOne(i => i.Personnel)
                .HasForeignKey(i => i.PersonnelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Domain>()
                .HasMany(d => d.MailEntries)
                .WithOne(m => m.Domain)
                .HasForeignKey(m => m.DomainId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure unique constraint for InventoryItem SerialNumber
            modelBuilder.Entity<InventoryItem>()
                .HasIndex(i => i.SerialNumber)
                .IsUnique();
        }
    }
}