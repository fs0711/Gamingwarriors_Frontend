using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using System.Text;

namespace POS.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class addcardModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);


        private readonly ILogger<addcardModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public addcardModel(ILogger<addcardModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        [BindProperty]
        public CardInputModel Card { get; set; }

        public class CardInputModel
        {
            [Required]
            public string CardNumber { get; set; }

            [Required]
            public string Branch { get; set; }

        }
        public string SuccessMessage { get; set; }
        public IActionResult OnGet()
        {
            var accessToken = HttpContext.Session.GetString("SessionToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToPage("/signin");
            }
            SuccessMessage = TempData["SuccessMessage"] as string;
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

            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToPage("/signin");
            }

            client.DefaultRequestHeaders.Add("x-session-key", accessToken);

            var cardData = new
            {
                card_uid = Card.CardNumber,
                branch = Card.Branch
            };

            var content = new StringContent(JsonSerializer.Serialize(cardData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://127.0.0.1:5000/api/rfid/create", content);

            if (response.IsSuccessStatusCode)
            {
                //var responseContent = await response.Content.ReadAsStringAsync();
                //HttpContext.Session.SetString("FullResponse", responseContent);
                //return RedirectToPage("/response");
                TempData["SuccessMessage"] = "Data saved successfully!";
                return RedirectToPage("/addcard");
            }
            else
            {
                _logger.LogError("Error posting card data: {StatusCode}", response.StatusCode);
                ModelState.AddModelError(string.Empty, "There was an error saving the card.");
                return Page();
            }
        }
    }

}
