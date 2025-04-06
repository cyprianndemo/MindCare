using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MindCare.Hubs;
using MindCare.Models;
using MindCare.Services;
using System;
using System.Threading.Tasks;

namespace MindCare.Controllers
{
    public class ChatController : Controller
    {
        private readonly ChatbotService _chatbotService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(ChatbotService chatbotService, IHubContext<ChatHub> hubContext)
        {
            _chatbotService = chatbotService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (string.IsNullOrEmpty(message.Content))
                return BadRequest("Message content cannot be empty.");

            try
            {
                // Process the message with the chatbot that now uses Claude AI
                var response = await _chatbotService.ProcessMessage(message.Content, message.SenderId);

                // Create bot response message
                var botResponse = new Message
                {
                    Content = response,
                    SenderId = "bot",
                    ReceiverId = message.SenderId,
                    Timestamp = DateTime.UtcNow,
                    IsFromBot = true
                };

                // Send bot response to user via SignalR
                await _hubContext.Clients.User(message.SenderId).SendAsync("ReceiveMessage", botResponse);

                return Ok(new { message = response }); // Return the bot's response
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                return StatusCode(500, new { message = "Sorry, I encountered an error processing your message." });
            }
        }
    }
}