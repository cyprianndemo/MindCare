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
            
            modelBuilder.Entity<DirectMessage>()
               .HasOne(d => d.Sender)
               .WithMany()
               .HasForeignKey(d => d.SenderId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DirectMessage>()
                .HasOne(d => d.Recipient)
                .WithMany()
                .HasForeignKey(d => d.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<PrescriptionMedication>()
                .HasKey(pm => new { pm.PrescriptionId, pm.MedicationId });

            modelBuilder.Entity<PrescriptionMedication>()
                .HasOne(pm => pm.Prescription)
                .WithMany(p => p.PrescriptionMedications)
                .HasForeignKey(pm => pm.PrescriptionId);

            modelBuilder.Entity<PrescriptionMedication>()
                .HasOne(pm => pm.Medication)
                .WithMany(m => m.PrescriptionMedications)
                .HasForeignKey(pm => pm.MedicationId);

            modelBuilder.Entity<MoodEntry>()
               .HasIndex(m => new { m.UserId, m.EntryDate });
           modelBuilder.Entity<MoodEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Mood).IsRequired();
                entity.Property(e => e.Intensity).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.EntryDate).IsRequired();

                // Configure the relationship with ApplicationUser
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Checkout> Checkouts { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }

        public DbSet<MentalHealthResource> MentalHealthResources { get; set;}
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MoodEntry> MoodEntries { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DirectMessage> DirectMessages { get; set; }
        public DbSet<ReportMetrics> Metrics { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<ConversationLog> ConversationLogs { get; set; }
        public DbSet<SupportGroup> SupportGroups { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Discussion> Discussions { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<MentalHealthProfile> MentalHealthProfiles { get; set; }
        public DbSet<MoodLog> MoodLogs { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<UserProgress> UserProgress { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }

        public DbSet<MentalHealthExercise> MentalHealthExercises { get; set; }
        public DbSet<ProfessionalResource> ProfessionalResources { get; set; }
        public DbSet<MentalHealthAssessment> MentalHealthAssessments { get; set; }
        
    }
}
