using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using static POS.Pages.SigninModel;

namespace POS.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class addmemberModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<addmemberModel> _logger;

        public addmemberModel(IHttpClientFactory httpClientFactory, ILogger<addmemberModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        [BindProperty]
        public MemberInputModel Member { get; set; }

        public class MemberInputModel
        {
            [Required]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Level { get; set; }

            [Required]
            [Phone]
            public string Mobile { get; set; }

            [Required]
            public string NIC { get; set; }

            [Required]
            public string CardId { get; set; }

            [Required]
            public string City { get; set; }

            [Required]
            public int Rechargeamount { get; set; }

        }

        public IActionResult OnGet()
        {
            var accessToken = HttpContext.Session.GetString("SessionToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToPage("/signin");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = _httpClientFactory.CreateClient();

            var accessToken = HttpContext.Session.GetString("SessionToken");

            client.DefaultRequestHeaders.Add("x-session-key", accessToken);

            var memberData = new
            {
                name = Member.Name,
                email_address = Member.Email,
                membership_level = Member.Level,
                phone_number = Member.Mobile,
                nic = Member.NIC,
                card_id = Member.CardId,
                city = Member.City,
                credit = Member.Rechargeamount
            };

            var content = new StringContent(JsonSerializer.Serialize(memberData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://127.0.0.1:5000/api/members/create", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                HttpContext.Session.SetString("FullResponse", responseContent);
                return RedirectToPage("/response");
            }
            else
            {
                _logger.LogError("Error posting member data: {StatusCode}", response.StatusCode);
                ModelState.AddModelError(string.Empty, "There was an error saving the member.");
                return Page();
            }
        }
    }

}
