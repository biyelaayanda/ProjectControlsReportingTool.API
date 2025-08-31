using Microsoft.EntityFrameworkCore;
using ProjectControlsReportingTool.API.Models.Entities;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Data.Entities;

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
        
        // Notification entities
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<NotificationPreference> NotificationPreferences { get; set; }
        public DbSet<NotificationHistory> NotificationHistories { get; set; }
        public DbSet<EmailQueue> EmailQueues { get; set; }
        public DbSet<NotificationSubscription> NotificationSubscriptions { get; set; }
        
        // User notification preferences
        public DbSet<UserNotificationPreference> UserNotificationPreferences { get; set; }
        
        // Email template management
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        
        // Push notification management
        public DbSet<PushNotificationSubscription> PushNotificationSubscriptions { get; set; }
        
        // SMS management
        public DbSet<SmsMessage> SmsMessages { get; set; }
        public DbSet<SmsTemplate> SmsTemplates { get; set; }
        public DbSet<SmsStatistic> SmsStatistics { get; set; }
        
        // Teams integration management
        public DbSet<TeamsWebhookConfig> TeamsWebhookConfigs { get; set; }
        public DbSet<TeamsMessage> TeamsMessages { get; set; }
        public DbSet<TeamsNotificationTemplate> TeamsNotificationTemplates { get; set; }
        public DbSet<TeamsIntegrationStat> TeamsIntegrationStats { get; set; }
        public DbSet<TeamsDeliveryFailure> TeamsDeliveryFailures { get; set; }

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

            // Configure Notification entity
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.Type).HasConversion<int>();
                entity.Property(e => e.Priority).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(n => n.Recipient)
                    .WithMany()
                    .HasForeignKey(n => n.RecipientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.Sender)
                    .WithMany()
                    .HasForeignKey(n => n.SenderId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(n => n.RelatedReport)
                    .WithMany()
                    .HasForeignKey(n => n.RelatedReportId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(n => new { n.RecipientId, n.ReadDate });
                entity.HasIndex(n => new { n.Type, n.CreatedDate });
                entity.HasIndex(n => n.Status);
            });

            // Configure NotificationTemplate entity
            modelBuilder.Entity<NotificationTemplate>(entity =>
            {
                entity.Property(e => e.Type).HasConversion<int>();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => new { e.Type, e.IsActive });
            });

            // Configure NotificationPreference entity
            modelBuilder.Entity<NotificationPreference>(entity =>
            {
                entity.Property(e => e.NotificationType).HasConversion<int>();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(np => np.User)
                    .WithMany()
                    .HasForeignKey(np => np.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(np => new { np.UserId, np.NotificationType }).IsUnique();
            });

            // Configure NotificationHistory entity
            modelBuilder.Entity<NotificationHistory>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(nh => nh.Notification)
                    .WithMany(n => n.History)
                    .HasForeignKey(nh => nh.NotificationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(nh => new { nh.NotificationId, nh.CreatedDate });
            });

            // Configure EmailQueue entity
            modelBuilder.Entity<EmailQueue>(entity =>
            {
                entity.Property(e => e.Priority).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => new { e.Status, e.ScheduledDate });
                entity.HasIndex(e => e.Priority);
            });

            // Configure NotificationSubscription entity
            modelBuilder.Entity<NotificationSubscription>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.WebhookUrl).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure UserNotificationPreference entity
            modelBuilder.Entity<UserNotificationPreference>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Create unique constraint for UserId + NotificationType
                entity.HasIndex(e => new { e.UserId, e.NotificationType }).IsUnique();
                
                // Create indexes for common queries
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.NotificationType);
                entity.HasIndex(e => e.EmailEnabled);
                entity.HasIndex(e => e.RealTimeEnabled);
                
                // Configure foreign key relationship
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure EmailTemplate entity
            modelBuilder.Entity<EmailTemplate>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Create unique constraint for template name
                entity.HasIndex(e => e.Name).IsUnique();
                
                // Create indexes for common queries
                entity.HasIndex(e => e.TemplateType);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsSystemTemplate);
                entity.HasIndex(e => e.Version);
                entity.HasIndex(e => e.UsageCount);
                entity.HasIndex(e => e.LastUsed);
                entity.HasIndex(e => e.CreatedAt);
                
                // Configure foreign key relationships
                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.LastUpdater)
                    .WithMany()
                    .HasForeignKey(e => e.UpdatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure PushNotificationSubscription entity
            modelBuilder.Entity<PushNotificationSubscription>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Create unique constraint for endpoint to prevent duplicates
                entity.HasIndex(e => e.Endpoint).IsUnique();
                
                // Create indexes for common queries
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.DeviceType);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.HasPermission);
                entity.HasIndex(e => e.LastUsed);
                entity.HasIndex(e => e.ExpiresAt);
                entity.HasIndex(e => new { e.UserId, e.IsActive });
                entity.HasIndex(e => new { e.DeviceType, e.IsActive });
                
                // Configure foreign key relationship
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SMS entities
            modelBuilder.Entity<SmsMessage>(entity =>
            {
                entity.HasIndex(e => e.Recipient);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.SentAt);
                entity.HasIndex(e => e.ExternalMessageId);
                entity.HasIndex(e => e.BatchId);
                entity.HasIndex(e => new { e.UserId, e.SentAt });
                entity.HasIndex(e => new { e.Status, e.IsUrgent });
                entity.HasIndex(e => new { e.Provider, e.SentAt });
                
                // Configure foreign key relationships
                entity.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(s => s.RelatedReport)
                    .WithMany()
                    .HasForeignKey(s => s.RelatedReportId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(s => s.Template)
                    .WithMany(t => t.SmsMessages)
                    .HasForeignKey(s => s.TemplateId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<SmsTemplate>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsSystemTemplate);
                entity.HasIndex(e => new { e.Category, e.IsActive });
                
                // Configure foreign key relationships
                entity.HasOne(t => t.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(t => t.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(t => t.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(t => t.UpdatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SmsStatistic>(entity =>
            {
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.Provider);
                entity.HasIndex(e => new { e.Date, e.Provider }).IsUnique();
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
