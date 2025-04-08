using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class SupportGroup
    {
        [Key]

        public int GroupId { get; set; }
        public int Id { get; set; }
        public string? ModeratorId { get; set; }
        public virtual ApplicationUser? Moderator { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; }
/*        public virtual ICollection<ApplicationUser> Members { get; set; }
*/
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<GroupMember> Members { get; set; }
        public virtual ICollection<Discussion> Discussions { get; set; }
    }

    // Group member model
    public class GroupMember
    {
        [Key]
        public int MemberId { get; set; }

        [Required]
        public int GroupId { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime JoinedAt { get; set; }

        // Navigation properties
        [ForeignKey("GroupId")]
        public virtual SupportGroup Group { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }

    // Discussion model
    public class Discussion
    {
        [Key]
        public int DiscussionId { get; set; }

        [Required]
        public int GroupId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("GroupId")]
        public virtual SupportGroup Group { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Response> Responses { get; set; }
    }

    // Response model
    public class Response
    {
        [Key]
        public int ResponseId { get; set; }

        [Required]
        public int DiscussionId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("DiscussionId")]
        public virtual Discussion Discussion { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }

    // Mental health profile model
    public class MentalHealthProfile
    {
        [Key]
        public int ProfileId { get; set; }

        [Required]
        public string UserId { get; set; }

        public string Biography { get; set; }

        public string Conditions { get; set; }

        public int JourneyProgress { get; set; } // 0-100 scale

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }

    // Mood log model
    public class MoodLog
    {
        [Key]
        public int MoodLogId { get; set; }

        [Required]
        public string UserId { get; set; }

        public int MoodRating { get; set; } // 1-10 scale

        public string MoodDescription { get; set; }

        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }

    // Conversation model for direct messages
    public class Conversation
    {
        [Key]
        public int ConversationId { get; set; }

        [Required]
        public string User1Id { get; set; }

        [Required]
        public string User2Id { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastActivityAt { get; set; }


        // Navigation properties
        [ForeignKey("User1Id")]
        public virtual ApplicationUser User1 { get; set; }

        [ForeignKey("User2Id")]
        public virtual ApplicationUser User2 { get; set; }

        public virtual ICollection<DirectMessage> Messages { get; set; }
    }

    
}
