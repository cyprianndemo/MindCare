using MindCare.Models;

namespace MindCare.ViewModel
{
    public class ConversationViewModel
    {
        public ApplicationUser CurrentUser { get; set; }
        public ApplicationUser OtherUser { get; set; }
        public List<DirectMessage> Messages { get; set; }
       
            public int ConversationId { get; set; }
            public string OtherUserId { get; set; }
            public string OtherUserName { get; set; }
            public string MoodIcon { get; set; }
            public string LastMessagePreview { get; set; }
            public DateTime? LastMessageTime { get; set; }
            public int UnreadCount { get; set; }
            public bool IsOnline { get; set; }
            public string IsOffline { get; set; }
        
    }
    
}
