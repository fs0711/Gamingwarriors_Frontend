using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using static POS.Pages.SigninModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace POS.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class posModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<posModel> _logger;

        public posModel(IHttpClientFactory httpClientFactory, ILogger<posModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public PosInputModel POS { get; set; }
        public SelectList rfcardList { get; set; }


        public class PosInputModel
        {

            [Required]
            public int Amount { get; set; }

            [Required]
            public string Purpose { get; set; }


            [Required]
            public string Card_id { get; set; }


        }

        public async Task<IActionResult> OnGetAsync()
        {
            var accessToken = HttpContext.Session.GetString("SessionToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToPage("/signin");
            }

            var client = _httpClientFactory.CreateClient();


            client.DefaultRequestHeaders.Add("x-session-key", accessToken);

            var responserfcard = await client.GetAsync("http://127.0.0.1:5000/api/rfid/list_rfcards");

            if (responserfcard.IsSuccessStatusCode)
            {
                var json = await responserfcard.Content.ReadAsStringAsync();


                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var responseDatarfcard = doc.RootElement.GetProperty("response_data").EnumerateArray();

                    var rfcards = responseDatarfcard.Select(rfcards => new
                    {
                        Id = rfcards.GetProperty("card_id").GetString(),
                        card_id = rfcards.GetProperty("id").GetString()
                    }).ToList();

                    rfcardList = new SelectList(rfcards, "card_id", "Id");
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            var client = _httpClientFactory.CreateClient();

            var accessToken = HttpContext.Session.GetString("SessionToken");

            client.DefaultRequestHeaders.Add("x-session-key", accessToken);

            var posData = new
            {
                amount = POS.Amount,
                purpose = POS.Purpose,
                card_id = POS.Card_id,

            };

            var content = new StringContent(JsonSerializer.Serialize(posData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://127.0.0.1:5000/api/accounts/credit", content);

            if (response.IsSuccessStatusCode)
            {
                //var responseContent = await response.Content.ReadAsStringAsync();
                //HttpContext.Session.SetString("FullResponse", responseContent);
                //return RedirectToPage("/response");
                return Page();
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
