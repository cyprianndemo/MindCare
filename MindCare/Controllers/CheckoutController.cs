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
        public IActionResult Pay(/*int appointmentId*/)
        {
            /*var viewModel = new PaymentViewModel();
            var appointment = _context.Appointments.FirstOrDefault(a => a.AppointmentId == appointmentId);
            if (appointment == null)
            {
                return NotFound();
            }

            ViewBag.AppointmentId = appointmentId;
            ViewBag.Amount = 1500;*/

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TestPayment([FromForm] TestPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                // Get current user
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, errors = new[] { "User not authenticated" } });
                }

                // Generate a unique transaction ID
                string transactionId = Guid.NewGuid().ToString("N");

                // Create and save payment record
                var payment = new Payment
                {
                    TransactionId = transactionId,
                    Amount = model.Amount,
                    PaymentDate = DateTime.UtcNow,
                    Status = PaymentStatus.Completed,
                    Method = MindCare.Models.PaymentMethod.Test,
                    TransactionDate = DateTime.UtcNow,
                    PhoneNumber = model.PhoneNumber ?? "N/A",
                    TransactionCode = $"TEST-{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                    IsSuccessful = true,
                    // Store the user ID directly instead of looking up a Student
                    UserId = currentUser.Id // Assuming Payment has a UserId field
                };

                // Add and save to database
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Create receipt data
                var receiptData = new ReceiptViewModel
                {
                    ReceiptNumber = $"RCP-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}",
                    TransactionId = transactionId,
                    PaymentDate = payment.PaymentDate,
                    Amount = model.Amount,
                    PaymentMethod = "Test Payment",
                    Status = "Completed",
                    CustomerDetails = new CustomerDetails
                    {
                        Name = $"{currentUser.FirstName} {currentUser.LastName}",
                        Email = currentUser.Email,
                        PhoneNumber = model.PhoneNumber ?? "N/A"
                    }
                };

                // Store receipt data in TempData for retrieval in receipt view
                TempData["ReceiptData"] = System.Text.Json.JsonSerializer.Serialize(receiptData);

                _logger.LogInformation($"Test payment successful. Transaction ID: {transactionId}");

                var response = new
                {
                    success = true,
                    transactionId = transactionId,
                    amount = model.Amount,
                    date = payment.PaymentDate,
                    message = "Test payment processed successfully",
                    receiptUrl = Url.Action("Receipt", "Checkout", new { id = transactionId })
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing test payment: " + ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: " + ex.InnerException.Message);
                }
                return Json(new { success = false, errors = new[] { "An error occurred while processing the payment." } });
            }
        }

        // Rest of the controller methods remain the same...

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

                // Determine payment method name based on the Method enum
                string paymentMethodName;
                switch (payment.Method)
                {
                    case MindCare.Models.PaymentMethod.MPesa:
                        paymentMethodName = "M-Pesa";
                        break;
                    case MindCare.Models.PaymentMethod.Stripe:
                        paymentMethodName = "Visa";
                        break;
                    case MindCare.Models.PaymentMethod.Test:
                        paymentMethodName = "Test Payment";
                        break;
                    default:
                        paymentMethodName = "Other";
                        break;
                }

                // Create receipt view model
                var receiptData = new ReceiptViewModel
                {
                    ReceiptNumber = payment.MpesaReceiptNumber ?? $"RCP-{payment.TransactionId.Substring(0, 8)}",
                    TransactionId = payment.TransactionId,
                    PaymentDate = payment.PaymentDate,
                    Amount = payment.Amount,
                    PaymentMethod = paymentMethodName,
                    Status = payment.IsSuccessful ? "Completed" : "Failed",
                    CustomerDetails = new CustomerDetails
                    {
                        Name = $"{currentUser.FirstName} {currentUser.LastName}",
                        Email = currentUser.Email,
                        PhoneNumber = payment.PhoneNumber ?? "N/A"
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
            if (model.Amount <= 0 || string.IsNullOrEmpty(model.PhoneNumber))
            {
                ModelState.AddModelError("Error", "Invalid amount or phone number.");
                return View("Pay", model);
            }

            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var response = await InitiateStkPush(model.PhoneNumber, model.Amount.ToString());

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"M-Pesa API response: {responseContent}");

                // Parse the response more safely
                dynamic mpesaResponseDynamic = JsonConvert.DeserializeObject<dynamic>(responseContent);

                // Generate a unique transaction ID
                string transactionId = Guid.NewGuid().ToString("N");

                // Safely get the CheckoutRequestID or use a default empty string
                string checkoutRequestId = "";
                try
                {
                    checkoutRequestId = mpesaResponseDynamic.CheckoutRequestID?.ToString() ?? "";
                    // If your response structure is different, adjust the property path
                    // For example: checkoutRequestId = mpesaResponseDynamic.Body.stkCallback.CheckoutRequestID?.ToString() ?? "";
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error parsing CheckoutRequestID: {ex.Message}");
                }

                // Store transaction details in the database
                var payment = new Payment
                {
                    TransactionId = transactionId,
                    PhoneNumber = model.PhoneNumber,
                    Amount = model.Amount,
                    PaymentDate = DateTime.UtcNow,
                    TransactionDate = DateTime.UtcNow,
                    CheckoutRequestID = checkoutRequestId,
                    Status = PaymentStatus.Pending,
                    Method = MindCare.Models.PaymentMethod.MPesa,
                    UserId = currentUser.Id,
                    IsSuccessful = false // Will be updated when callback is received
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Redirect to waiting page with a timeout for 45 seconds
                return RedirectToAction("WaitForMpesaPayment", new { paymentId = payment.PaymentId });
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"M-Pesa API error: {responseContent}");
                return RedirectToAction("Fail", new { reason = "api_error" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> WaitForMpesaPayment(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return NotFound();
            }

            // Return the payment object as the model instead of just using ViewBag
            return View(payment);
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
        [Route("/mpesa/callback")]
        public async Task<IActionResult> MpesaCallback([FromBody] MpesaResponse response)
        {
            var metadata = response.Body.stkCallback.CallbackMetadata;
            var transactionCode = metadata.Item.FirstOrDefault(i => i.Name == "MpesaReceiptNumber")?.Value;
            var status = response.Body.stkCallback.ResultCode == 0 ? "Successful" : "Failed";

            var payment = _context.Payments
                .FirstOrDefault(p => p.CheckoutRequestID == response.Body.stkCallback.CheckoutRequestID);

            if (payment != null)
            {
                payment.Status = PaymentStatus.Completed;
                payment.MpesaReceiptNumber = transactionCode;
                await _context.SaveChangesAsync();
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
        [HttpGet]
        public async Task<IActionResult> Fail(string transactionId, string reason)
        {
            // Update payment status if needed
            if (!string.IsNullOrEmpty(transactionId))
            {
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.TransactionId == transactionId && p.Status == PaymentStatus.Pending);

                if (payment != null)
                {
                    payment.Status = PaymentStatus.Failed;

                    if (reason == "timeout")
                    {
                        payment.FailureReason = "Payment request timed out";
                    }
                    else
                    {
                        payment.FailureReason = "Payment failed or was cancelled";
                    }

                    await _context.SaveChangesAsync();
                }
            }

            ViewBag.TransactionId = transactionId;
            ViewBag.Reason = reason;

            return View();
        }
    }
}