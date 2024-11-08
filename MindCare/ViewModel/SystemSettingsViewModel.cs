using System.ComponentModel.DataAnnotations;

namespace MindCare.ViewModels
{
    public class SystemSettingsViewModel
    {
        [Display(Name = "Enable Maintenance Mode")]
        public bool MaintenanceMode { get; set; }

        [Display(Name = "Default Role for New Users")]
        public string DefaultRole { get; set; }

        // Add any additional settings as needed
    }
}
