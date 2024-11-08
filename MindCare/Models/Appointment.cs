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

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Time { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [ForeignKey("Student")]
        public string StudentId { get; set; }
        public virtual Student? Student { get; set; }

        [ForeignKey("Therapist")]
        public string TherapistId { get; set; }
        //public virtual Therapist? Therapist { get; set; }
        public ApplicationUser? Therapist { get; set; }


        [ForeignKey("Psychiatrist")]
        public string PsychiatristId { get; set; }
        public virtual Psychiatrist? Psychiatrist { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        // Helper methods for timezone handling
        public void SetTimesInUtc(DateTime localStartTime, DateTime localEndTime)
        {
            StartTime = DateTime.SpecifyKind(localStartTime.ToUniversalTime(), DateTimeKind.Utc);
            EndTime = DateTime.SpecifyKind(localEndTime.ToUniversalTime(), DateTimeKind.Utc);
        }

        public DateTime GetLocalStartTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(StartTime, TimeZoneInfo.Local);
        }

        public DateTime GetLocalEndTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(EndTime, TimeZoneInfo.Local);
        }
    }
}