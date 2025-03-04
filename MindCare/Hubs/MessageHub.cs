using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

public class MessageHub : Hub
{
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }

    // This method isn't actually used in your current implementation
    // You're directly calling _hubContext.Clients.Group() from your controller
    public async Task SendMessage(string conversationId, string messageId, string senderId, string senderName,
        string senderMoodIcon, string content, DateTime createdAt)
    {
        await Clients.Group(conversationId).SendAsync("ReceiveMessage", messageId, senderId,
            senderName, senderMoodIcon, content, createdAt);
    }
}