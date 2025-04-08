using System.ComponentModel.DataAnnotations;

namespace MindCare.ViewModel
{
    public class SupportGroupViewModel
    {
        public int GroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MemberCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // View model for group details page
    public class GroupDetailsViewModel
    {
        public int GroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsMember { get; set; }
        public int MemberCount { get; set; }
        public List<DiscussionViewModel> Discussions { get; set; }
    }

    // View model for discussions list
    public class DiscussionViewModel
    {
        public int DiscussionId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
        public int ResponseCount { get; set; }
        public DateTime LatestResponseAt { get; set; }
    }

    // View model for creating a new discussion
    public class CreateDiscussionViewModel
    {
        public int GroupId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }
    }

    // View model for discussion details page
    public class DiscussionDetailsViewModel
    {
        public int DiscussionId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
        public string UserMoodIcon { get; set; }
        public List<ResponseViewModel> Responses { get; set; }
        public CreateResponseViewModel NewResponse { get; set; }
    }

    // View model for responses
    public class ResponseViewModel
    {
        public int ResponseId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
        public string UserMoodIcon { get; set; }
        public bool IsAuthor { get; set; }
    }

    // View model for creating a new response
    public class CreateResponseViewModel
    {
        public int DiscussionId { get; set; }

        [Required(ErrorMessage = "Response content is required")]
        public string Content { get; set; }
    }

    // View model for peer suggestion/finding
    public class PeerViewModel
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string CurrentMood { get; set; }
        public string MoodIcon { get; set; }
        public string Biography { get; set; }
        public string Conditions { get; set; }
        public int JourneyProgress { get; set; }
        public DateTime? LastActive { get; set; }
    }
    public class UpdateMoodViewModel
    {
        [Required(ErrorMessage = "Please describe your current mood")]
        [StringLength(100, ErrorMessage = "Mood description cannot be longer than 100 characters")]
        public string CurrentMood { get; set; }

        [Required(ErrorMessage = "Please select a mood icon")]
        public string MoodIcon { get; set; }

        [Range(1, 10, ErrorMessage = "Please rate your mood from 1 to 10")]
        public int MoodRating { get; set; }

        public string Notes { get; set; }
    }

    // View model for conversation list
    

    // View model for conversation details
    public class ConversationDetailsViewModel
    {
        public int ConversationId { get; set; }
        public string OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string MoodIcon { get; set; }
        public bool IsOnline { get; set; }
        public List<MessageViewModel> Messages { get; set; }
        public CreateMessageViewModel NewMessage { get; set; }
    }

    // View model for messages
    public class MessageViewModel
    {
        public int MessageId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SenderName { get; set; }
        public string SenderMoodIcon { get; set; }
        public bool IsFromCurrentUser { get; set; }
    }

    // View model for creating a new message
    public class CreateMessageViewModel
    {
        [Required(ErrorMessage = "Conversation ID is required.")]
        public int ConversationId { get; set; }

        [Required(ErrorMessage = "Recipient ID is required.")]
        public string RecipientId { get; set; }

        [Required(ErrorMessage = "Message content is required.")]
        [StringLength(500, ErrorMessage = "Message cannot be longer than 500 characters.")]
        public string Content { get; set; }
    }
    // View model for messages page
    public class MessagesViewModel
    {
        public List<ConversationViewModel> Conversations { get; set; }
        public ConversationDetailsViewModel ActiveConversation { get; set; }
    }

    // View model for notifications
    public class NotificationViewModel
    {
        public int NotificationId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; }
        public string Link { get; set; }
    }

}
