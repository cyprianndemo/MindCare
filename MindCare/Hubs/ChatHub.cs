using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MindCare.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.User(user).SendAsync("ReceiveMessage", message);
        }
    }
}
