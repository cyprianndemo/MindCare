using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindCare.Models
{
    // Models/UserActivity.cs
    public class UserActivity
    {
        public int Id { get; set; }
        public string  UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string? Action { get; set; }
        public string? Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
    }
}
