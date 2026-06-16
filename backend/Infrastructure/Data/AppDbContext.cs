using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<TicketStatus> TicketStatuses { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<TicketHoursLog> TicketHoursLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TicketComment>()
                .HasOne(tc => tc.User)
                .WithMany()
                .HasForeignKey(tc => tc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Department>()
                .HasOne(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Department)
                .WithMany()
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.EscalatedByUser)
                .WithMany()
                .HasForeignKey(t => t.EscalatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TicketAttachment>()
                .HasOne(a => a.UploadedByUser)
                .WithMany()
                .HasForeignKey(a => a.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TicketAttachment>()
                .HasOne(a => a.Comment)
                .WithMany()
                .HasForeignKey(a => a.CommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TicketHoursLog>()
                .HasOne(h => h.Ticket)
                .WithMany()
                .HasForeignKey(h => h.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TicketHoursLog>()
                .HasOne(h => h.Agent)
                .WithMany()
                .HasForeignKey(h => h.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TicketHoursLog>()
                .Property(h => h.HoursWorked)
                .HasPrecision(8, 2);
        }
    }
}
