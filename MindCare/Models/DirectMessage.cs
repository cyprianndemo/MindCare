using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MindCare.Models
{
    public class DirectMessage
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int ConversationId { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string RecipientId { get; set; }

        [Required]
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }

        // Navigation properties
        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; }

        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; }

        [ForeignKey("RecipientId")]
        public virtual ApplicationUser Recipient { get; set; }
    }

}
