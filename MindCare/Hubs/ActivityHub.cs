using Microsoft.AspNetCore.SignalR;
using MindCare.Models;

namespace MindCare.Hubs
{
    public class ActivityHub : Hub
    {
        public async Task SendActivity(UserActivity activity)
        {
            await Clients.All.SendAsync("ReceiveActivity", activity);
        }
    }
}
