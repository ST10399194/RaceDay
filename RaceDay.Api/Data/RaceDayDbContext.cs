using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

        // DbSets mapped directly to your custom ERD classes
        public DbSet<User> Users { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. User -> Participant (One-to-One)
            modelBuilder.Entity<User>()
          .HasOne(u => u.Participant)
          .WithOne(p => p.User)
          .HasForeignKey<Participant>(p => p.UserID)
          .OnDelete(DeleteBehavior.Cascade);

            // 2. User -> Organiser (One-to-One)
            modelBuilder.Entity<User>()
          .HasOne(u => u.Organiser)
          .WithOne(o => o.User)
          .HasForeignKey<Organiser>(o => o.UserID)
          .OnDelete(DeleteBehavior.Cascade);

            // 3. Organiser -> Events (One-to-Many)
            modelBuilder.Entity<Organiser>()
          .HasMany(o => o.Events)
          .WithOne(e => e.Organiser)
          .HasForeignKey(e => e.OrganiserID)
          .OnDelete(DeleteBehavior.Restrict);

            // 4. Category -> Events (One-to-Many)
            modelBuilder.Entity<Category>()
          .HasMany(c => c.Events)
          .WithOne(e => e.Category)
          .HasForeignKey(e => e.CategoryID)
          .OnDelete(DeleteBehavior.Restrict);

            // 5. Participant -> Enrollments (One-to-Many)
            modelBuilder.Entity<Participant>()
          .HasMany(p => p.Enrollments)
          .WithOne(e => e.Participant)
          .HasForeignKey(e => e.ParticipantID)
          .OnDelete(DeleteBehavior.Cascade);

            // 6. Event -> Enrollments (One-to-Many)
            modelBuilder.Entity<Event>()
          .HasMany(e => e.Enrollments)
          .WithOne(en => en.Event)
          .HasForeignKey(en => en.EventID)
          .OnDelete(DeleteBehavior.Cascade);

            // 7. Enrollment -> Result (One-to-Zero-or-One)
            modelBuilder.Entity<Enrollment>()
          .HasOne(e => e.Result)
          .WithOne(r => r.Enrollment)
          .HasForeignKey<Result>(r => r.EnrollmentID)
          .OnDelete(DeleteBehavior.Cascade);

            // Enforce Unique Emails
            modelBuilder.Entity<User>()
          .HasIndex(u => u.Email)
          .IsUnique();
        }
    }
}
