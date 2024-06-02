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
    public class addmemberprofileModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<addmemberModel> _logger;

        public addmemberprofileModel(IHttpClientFactory httpClientFactory, ILogger<addmemberModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        [BindProperty]
        public MemberProfileInputModel Memberprofile { get; set; }

        public class MemberProfileInputModel
        {
            [Required]
            public string Name { get; set; }

            [Required]
            public string CardId { get; set; }

            [Required]
            public string MemberId { get; set; }

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
                name = Memberprofile.Name,
                card_id = Memberprofile.CardId,
                credit = Memberprofile.Rechargeamount,
                member_id = Memberprofile.MemberId

            };

            var content = new StringContent(JsonSerializer.Serialize(memberData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://127.0.0.1:5000/api/profiles/create", content);

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
