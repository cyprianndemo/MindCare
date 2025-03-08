using System.ComponentModel.DataAnnotations;

namespace MindCare.ViewModel
{
    public class SystemSettingsViewModel
    {
        [Required]
        [Display(Name = "Application Name")]
        [StringLength(100)]
        public string ApplicationName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Support Email")]
        public string SupportEmail { get; set; }

        [Display(Name = "Maintenance Mode")]
        public bool MaintenanceMode { get; set; }

        [Required]
        [Range(15, 180)]
        [Display(Name = "Session Duration (Minutes)")]
        public int SessionDurationMinutes { get; set; } = 60;

        [Required]
        [Range(1, 20)]
        [Display(Name = "Max Appointments Per Day")]
        public int MaxAppointmentsPerDay { get; set; } = 10;

        [Display(Name = "Allow Self-Registration")]
        public bool AllowSelfRegistration { get; set; } = true;

        [Display(Name = "Enabled Payment Methods")]
        public string EnabledPaymentMethods { get; set; }

        [Required]
        [Display(Name = "Video Call Provider")]
        public string VideoCallProvider { get; set; } = "Zoom";

        [Required]
        [Display(Name = "Default Currency Symbol")]
        [StringLength(5)]
        public string DefaultCurrencySymbol { get; set; } = "$";

        [Required]
        [Display(Name = "System Time Zone")]
        public string SystemTimeZone { get; set; } = "UTC";

        [Display(Name = "Notification Settings")]
        public string NotificationSettings { get; set; }
    }
}