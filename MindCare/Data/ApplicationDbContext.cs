using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MindCare.Models;
using System.Security.Cryptography.X509Certificates;

namespace MindCare.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
           
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Therapist)
                .WithMany()
                .HasForeignKey(a => a.TherapistId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Psychiatrist)
                .WithMany()
                .HasForeignKey(a => a.PsychiatristId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Checkout> Checkouts { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<MentalHealthResource> MentalHealthResources { get; set;}
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MoodEntry> MoodEntries { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}
