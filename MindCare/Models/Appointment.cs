using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindCare.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Please select an appointment time")]
        public string Time { get; set; }

        public DateTime? CancellationTime { get; set; }
        public string? CancelledById { get; set; }

        [Required]
        public DateTime LastModified { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [Required(ErrorMessage = "Student ID is required")]
        [ForeignKey("Student")]
        public string StudentId { get; set; }

        public virtual Student? Student { get; set; }
       
        [ForeignKey("Therapist")]
        public string? TherapistId { get; set; }

        public ApplicationUser? Therapist { get; set; }

        [ForeignKey("Psychiatrist")]
        public string? PsychiatristId { get; set; }

        public ApplicationUser? Psychiatrist { get; set; }
        public string? UpdatedBy { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
        // In MindCare.Models.Appointment
        public string? StudentFirstName { get; set; }
        public string? StudentLastName { get; set; }
        // Helper methods for timezone handling
        public void SetTimesInUtc(DateTime localStartTime, DateTime localEndTime)
        {
            StartTime = localStartTime.ToUniversalTime();
            EndTime = localEndTime.ToUniversalTime();
        }

        public void SetAppointmentTimes(DateTime date, string time)
        {
            // Ensure the date is treated as local time
            var localDate = DateTime.SpecifyKind(date, DateTimeKind.Local);
            var localStartTime = localDate.Date.Add(TimeSpan.Parse(time));
            var localEndTime = localStartTime.AddHours(1);

            // Convert to UTC for storage
            StartTime = localStartTime.ToUniversalTime();
            EndTime = localEndTime.ToUniversalTime();
        }

        public DateTime GetLocalStartTime()
        {
            return StartTime.ToLocalTime();
        }

        public DateTime GetLocalEndTime()
        {
            return EndTime.ToLocalTime();
        }

        public void EnsureUtcTimes()
        {
            // Convert all DateTime properties to UTC if they're not already
            if (CancellationTime.HasValue)
            {
                CancellationTime = DateTime.SpecifyKind(CancellationTime.Value, DateTimeKind.Utc);
            }

            LastModified = DateTime.SpecifyKind(LastModified, DateTimeKind.Utc);
            CreatedAt = DateTime.SpecifyKind(CreatedAt, DateTimeKind.Utc);
            StartTime = DateTime.SpecifyKind(StartTime, DateTimeKind.Utc);
            EndTime = DateTime.SpecifyKind(EndTime, DateTimeKind.Utc);
            Date = DateTime.SpecifyKind(Date, DateTimeKind.Utc);
        }
    }
}