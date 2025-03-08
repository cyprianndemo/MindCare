using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class SystemSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ApplicationName { get; set; } = "MindCare";

        [Required]
        [EmailAddress]
        public string SupportEmail { get; set; } = "support@mindcare.com";

        public bool MaintenanceMode { get; set; } = false;

        [Required]
        [Range(15, 180)]
        public int SessionDurationMinutes { get; set; } = 60;

        [Required]
        [Range(1, 20)]
        public int MaxAppointmentsPerDay { get; set; } = 10;

        public bool AllowSelfRegistration { get; set; } = true;

        public string EnabledPaymentMethods { get; set; } = "CreditCard,PayPal";

        [Required]
        public string VideoCallProvider { get; set; } = "Zoom";

        [Required]
        [StringLength(5)]
        public string DefaultCurrencySymbol { get; set; } = "$";

        [Required]
        public string SystemTimeZone { get; set; } = "UTC";

        public string NotificationSettings { get; set; } = "EnableEmailNotifications,EnableAppointmentReminders";
    }
}