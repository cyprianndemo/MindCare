using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MindCare.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        // List of roles with selection status
        public List<RoleSelection> Roles { get; set; } = new List<RoleSelection>();
    }

    // Helper class for role selection
    public class RoleSelection
    {
        public string RoleName { get; set; }
        public bool Selected { get; set; } // Whether this role is selected for the user
    }
}
