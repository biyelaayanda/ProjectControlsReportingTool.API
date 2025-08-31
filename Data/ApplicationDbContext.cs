using Microsoft.EntityFrameworkCore;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;

namespace ProjectControlsReportingTool.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportTemplate> ReportTemplates { get; set; }
        public DbSet<ReportSignature> ReportSignatures { get; set; }
        public DbSet<ReportAttachment> ReportAttachments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasConversion<int>();
                entity.Property(e => e.Department).HasConversion<int>();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Report entity
            modelBuilder.Entity<Report>(entity =>
            {
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.Department).HasConversion<int>();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("GETUTCDATE()");

                // Configure relationships
                entity.HasOne(r => r.Creator)
                    .WithMany(u => u.CreatedReports)
                    .HasForeignKey(r => r.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.RejectedByUser)
                    .WithMany()
                    .HasForeignKey(r => r.RejectedBy)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(r => r.Template)
                    .WithMany(t => t.ReportsCreatedFromTemplate)
                    .HasForeignKey(r => r.TemplateId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure ReportTemplate entity
            modelBuilder.Entity<ReportTemplate>(entity =>
            {
                entity.Property(e => e.DefaultDepartment).HasConversion<int>();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(rt => rt.Creator)
                    .WithMany()
                    .HasForeignKey(rt => rt.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(rt => rt.LastModifiedByUser)
                    .WithMany()
                    .HasForeignKey(rt => rt.LastModifiedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure ReportSignature entity
            modelBuilder.Entity<ReportSignature>(entity =>
            {
                entity.Property(e => e.SignatureType).HasConversion<int>();
                entity.Property(e => e.SignedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(rs => rs.Report)
                    .WithMany(r => r.Signatures)
                    .HasForeignKey(rs => rs.ReportId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rs => rs.User)
                    .WithMany(u => u.Signatures)
                    .HasForeignKey(rs => rs.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ReportAttachment entity
            modelBuilder.Entity<ReportAttachment>(entity =>
            {
                entity.Property(e => e.UploadedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(ra => ra.Report)
                    .WithMany(r => r.Attachments)
                    .HasForeignKey(ra => ra.ReportId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ra => ra.UploadedByUser)
                    .WithMany()
                    .HasForeignKey(ra => ra.UploadedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure AuditLog entity
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.Property(e => e.Action).HasConversion<int>();
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(al => al.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(al => al.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(al => al.Report)
                    .WithMany(r => r.AuditLogs)
                    .HasForeignKey(al => al.ReportId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // No hardcoded seed data - users will be created through registration
            // This prevents issues with static password hashes and makes the system more flexible
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Report && e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Report report)
                {
                    report.LastModifiedDate = DateTime.UtcNow;
                }
            }
        }
    }
}
