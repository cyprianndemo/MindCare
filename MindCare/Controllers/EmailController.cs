using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MindCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EmailController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("Send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailModel model)
        {
            try
            {
                // Get SMTP settings from configuration
                var emailSettings = _configuration.GetSection("EmailSettings");
                var host = emailSettings["Host"];
                var port = int.Parse(emailSettings["Port"]);
                var username = emailSettings["Username"];
                var password = emailSettings["Password"];
                var fromAddress = emailSettings["FromAddress"];

                // Create mail message
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromAddress),
                    Subject = model.Subject,
                    Body = model.Body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(model.To);

                // Create SMTP client
                using (var client = new SmtpClient(host, port))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(username, password);
                    client.EnableSsl = true;

                    // Send email
                    await client.SendMailAsync(mailMessage);
                }

                return Ok(new { success = true, message = "Email sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Failed to send email: {ex.Message}" });
            }
        }
    }

    public class EmailModel
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}