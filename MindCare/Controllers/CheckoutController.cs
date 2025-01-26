using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MindCare.Models;
using MindCare.ViewModel;
using Microsoft.Extensions.Configuration;
using Stripe;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using MindCare.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MindCare.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<CheckoutController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public CheckoutController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<CheckoutController> logger,
        IConfiguration configuration,
        IHttpClientFactory clientFactory)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public IActionResult Pay()
        {
            var viewModel = new PaymentViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult TestPayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            // Create receipt data
            var receiptData = new ReceiptViewModel
            {
                ReceiptNumber = GenerateReceiptNumber(),
                TransactionId = Guid.NewGuid().ToString("N"),
                PaymentDate = DateTime.UtcNow,
                Amount = model.Amount,
                PaymentMethod = "Test Payment",
                Status = "Completed",
                CustomerDetails = new CustomerDetails
                {
                    Name = "Test Customer",
                    Email = "test@example.com",
                    PhoneNumber = model.PhoneNumber
                }
            };

            // Store receipt data in TempData for retrieval in receipt view
            TempData["ReceiptData"] = System.Text.Json.JsonSerializer.Serialize(receiptData);

            var response = new
            {
                success = true,
                transactionId = receiptData.TransactionId,
                amount = model.Amount,
                date = receiptData.PaymentDate,
                message = "Test payment processed successfully",
                receiptUrl = Url.Action("Receipt", "Payment", new { id = receiptData.TransactionId })
            };

            return Json(response);
        }
        public IActionResult PaymentSuccess(decimal amount)
        {
            return View(amount);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Receipt(string id)
        {
            try
            {
                // Get the currently logged in user
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Try to get payment details from database
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.TransactionId == id);

                if (payment == null)
                {
                    _logger.LogWarning($"Payment with ID {id} not found");
                    return NotFound("Payment not found");
                }

                // Create receipt view model
                var receiptData = new ReceiptViewModel
                {
                    ReceiptNumber = payment.MpesaReceiptNumber ?? $"RCP-{payment.TransactionId}",
                    TransactionId = payment.TransactionId,
                    PaymentDate = payment.TransactionDate,
                    Amount = payment.Amount,
                    PaymentMethod = "M-Pesa",
                    Status = payment.IsSuccessful ? "Completed" : "Failed",
                    CustomerDetails = new CustomerDetails
                    {
                        Name = $"{currentUser.FirstName} {currentUser.LastName}",
                        Email = currentUser.Email,
                        PhoneNumber = payment.PhoneNumber 
                    }
                };

                return View(receiptData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating receipt");
                return View("Error", new ErrorViewModel
                {
                    Message = "There was an error generating your receipt. Please try again later."
                });
            }
        }
    

    private string GenerateReceiptNumber()
        {
            return "RCP-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }
    
    [HttpPost]
        [ActionName("PayWithMpesa")]
        public async Task<IActionResult> PayWithMpesa(PaymentViewModel model)
        {
            if (model.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Amount must be greater than 0.");
                return View("Pay", model);
            }

            if (model.PhoneNumber.StartsWith("254"))
            {
                var response = await InitiateStkPush(model.PhoneNumber, model.Amount.ToString());
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(PaymentResult), new { paymentId = "MPesaPaymentId" });
                }
                else
                {
                    ModelState.AddModelError("PhoneNumber", "M-Pesa payment initiation failed.");
                    return View("Pay", model);
                }
            }
            else
            {
                ModelState.AddModelError("PhoneNumber", "Invalid M-Pesa phone number.");
                return View("Pay", model);
            }
        }

        [HttpPost]
        [ActionName("PayWithVisa")]
        public async Task<IActionResult> PayWithVisa(PaymentViewModel model)
        {
            if (model.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Amount must be greater than 0.");
                return View("Pay", model);
            }

            var options = new ChargeCreateOptions
            {
                Amount = (long)(model.Amount * 100),
                Currency = "usd",
                Source = model.StripeToken,
                Description = "Rent Payment",
            };

            var service = new ChargeService();
            Charge charge = await service.CreateAsync(options);

            if (charge.Status == "succeeded")
            {
                return RedirectToAction(nameof(PaymentResult), new { paymentId = charge.Id });
            }
            else
            {
                return View("Error", new ErrorViewModel { Message = "Visa payment failed." });
            }
        }

        [HttpGet]
        public IActionResult PaymentResult(string paymentId)
        {
            var paymentResult = new PaymentResultViewModel
            {
                PaymentId = paymentId,
                Status = "Completed"
            };
            return View(paymentResult);
        }

        private async Task<string> GetToken()
        {
            var client = _clientFactory.CreateClient("mpesa");
            var authString = "PDOIhArqnGMNhVbjvNgwAG3RzHKftjxGPB393ZMs5VvVNgQp:gc89HUFtZGdGjH7HMMvQKRGLTKY3ulVx1Np0Mzu0gvV1XEkS9bkNerELmPeiikfn";
            var encodedString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authString));
            var _url = "/oauth/v1/generate?grant_type=client_credentials";
            var request = new HttpRequestMessage(HttpMethod.Get, _url);
            request.Headers.Add("Authorization", $"Basic {encodedString}");

            var response = await client.SendAsync(request);
            var mpesaResponse = await response.Content.ReadAsStringAsync();

            MPesaToken tokenObject = JsonConvert.DeserializeObject<MPesaToken>(mpesaResponse);
            return tokenObject.access_token;
        }

        private async Task<HttpResponseMessage> InitiateStkPush(string phoneNumber, string amount)
        {
            var jsonBody = JsonConvert.SerializeObject(new
            {
                BusinessShortCode = "174379",
                Password = GeneratePassword(),
                Timestamp = DateTime.Now.ToString("yyyyMMddHHmmss"),
                TransactionType = "CustomerPayBillOnline",
                Amount = amount,
                PartyA = phoneNumber,
                PartyB = "174379",
                PhoneNumber = phoneNumber,
                CallBackURL = "https://yourdomain.com/callback",
                AccountReference = "Subscription Payment",
                TransactionDesc = "Payment for " + amount + " subscription"
            });

            var jsonReadBody = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var token = await GetToken();
            var client = _clientFactory.CreateClient("mpesa");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var url = "/mpesa/stkpush/v1/processrequest";
            var response = await client.PostAsync(url, jsonReadBody);

            return response;
        }

        [HttpPost]
        [Route("/callback")]
        [Produces("application/json")]
        public async Task<IActionResult> MpesaCallback([FromBody] MpesaResponse response)
        {
            if (response.Body.stkCallback.ResultCode == 0)
            {
                var metadata = response.Body.stkCallback.CallbackMetadata;
                var amount = metadata.Item.FirstOrDefault(i => i.Name == "Amount")?.Value;
                var phoneNumber = metadata.Item.FirstOrDefault(i => i.Name == "PhoneNumber")?.Value;
                var mpesaReceiptNumber = metadata.Item.FirstOrDefault(i => i.Name == "MpesaReceiptNumber")?.Value;
                var transactionDate = metadata.Item.FirstOrDefault(i => i.Name == "TransactionDate")?.Value;


                var payment = new Payment
                {
                    PhoneNumber = phoneNumber,
                    Amount = Convert.ToDecimal(amount),
                    MpesaReceiptNumber = mpesaReceiptNumber,
                    TransactionDate = DateTime.ParseExact(transactionDate, "yyyyMMddHHmmss", null),
                    MerchantRequestID = response.Body.stkCallback.MerchantRequestID,
                    CheckoutRequestID = response.Body.stkCallback.CheckoutRequestID,
                    IsSuccessful = true
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Transaction successful and saved to the database.");
            }
            else
            {
                _logger.LogError("Transaction failed.");
            }

            return Ok();
        }

        private string GeneratePassword()
        {
            string shortCode = "174379";
            string passkey = "bfb279f9aa9bdbcf158e97dd71a467cd2e0c893059b10f78e6b72ada1ed2c919";
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string plainText = shortCode + passkey + timestamp;
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
