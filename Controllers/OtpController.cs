using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using KDKMenShop.Service;
using Microsoft.AspNetCore.Mvc;

namespace KDKMenShop.Controllers
{
    public class OtpController : Controller
    {
        private readonly ISmsService _smsService;

        public OtpController(ISmsService smsService)
        {
            _smsService = smsService;
        }
        [HttpGet]
        public IActionResult SendOtp()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SendOtp(string phoneNumber)
        {
            // Generate random OTP (One Time Password)
            Random rand = new Random();
            int otp = rand.Next(1000, 9999); // Generate a 4-digit OTP

            // Compose the message
            string message = $"Your OTP for verification is: {otp}";

            try
            {
                // Send OTP via SMS
                await _smsService.SendSmsAsync(phoneNumber, message);

                // Here you can store the OTP in the database or session for verification later

                return Ok("OTP sent successfully!");
            }
            catch (Exception ex)
            {
                // Handle errors appropriately
                return BadRequest($"Failed to send OTP: {ex.Message}");
            }
            
        }
        
       
    }
}
