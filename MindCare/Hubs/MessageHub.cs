using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace MindCare.Hubs
{
    public class MessageHub : Hub
    {
        // Join a specific conversation group
        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }

        // Leave a specific conversation group
        public async Task LeaveConversation(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        }

        // Method that can be called directly from clients
        public async Task SendMessage(string conversationId, string messageId, string senderId,
                                    string senderName, string senderMoodIcon, string content, DateTime createdAt)
        {
            await Clients.Group(conversationId).SendAsync("ReceiveMessage",
                messageId, senderId, senderName, senderMoodIcon, content, createdAt);
        }
    }
}