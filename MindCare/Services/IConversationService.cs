namespace MindCare.Services
{
    public interface IConversationService
    {
        Task LogCallAttempt(string callerId, string recipientId, string conversationId, string callType);
        Task LogCallAccepted(string callerId, string recipientId, string conversationId, string callType);
        Task LogCallRejected(string callerId, string recipientId, string reason);
        Task LogCallEnded(string userId, string peerId, string reason);
    }
}
