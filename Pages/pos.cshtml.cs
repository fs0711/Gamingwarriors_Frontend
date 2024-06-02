using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace POS.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class posModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<posModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public posModel(ILogger<posModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        [BindProperty]
        public RechargeInputModel Recharge { get; set; }

        public string ErrorMessage { get; set; }

        public class RechargeInputModel
        {
            [Required]
            public string CardTid { get; set; }

            [Required]
            [Range(0.01, double.MaxValue, ErrorMessage = "Please enter a valid amount")]
            public decimal RechargeAmount { get; set; }
        }

        public IActionResult OnGet()
        {
            var accessToken = HttpContext.Session.GetString("SessionToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToPage("/signin");
            }

            // Handle GET request if needed
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var rechargeData = new
            {
                card_tid = Recharge.CardTid,
                recharge_amount = Recharge.RechargeAmount
            };
            var accessToken = HttpContext.Session.GetString("SessionToken");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-session-key", accessToken);

            var content = new StringContent(JsonSerializer.Serialize(rechargeData), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                HttpContext.Session.SetString("FullResponse", responseContent);

                return RedirectToPage("/");
            }
            else
            {
                ErrorMessage = "An error occurred while processing the recharge.";
                return Page();
            }
        }
    }

}
