using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using static POS.Pages.SigninModel;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace POS.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class addgameModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<addgameModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public addgameModel(ILogger<addgameModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public GameInputModel Game { get; set; }

        public class GameInputModel
        {
            [Required]
            public string Name { get; set; }

            [Required]
            public string Type { get; set; }

            [Required]
            public int GameLevel { get; set; }

            [Required]
            [Range(0, double.MaxValue, ErrorMessage = "Please enter a valid cost")]
            public decimal Cost { get; set; }

            [Required]
            public string UnitStatus { get; set; }
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

            var client = _httpClientFactory.CreateClient();

            var accessToken = HttpContext.Session.GetString("SessionToken");

            client.DefaultRequestHeaders.Add("x-session-key", accessToken);


            var gameData = new
            {
                name =Game.Name,
                type = Game.Type,
                game_level = Game.GameLevel,
                unit_status = Game.UnitStatus,
                cost = Game.Cost
            };


            var gameJson = JsonSerializer.Serialize(Game);

            var content = new StringContent(JsonSerializer.Serialize(gameData), Encoding.UTF8, "application/json");


            var response = await client.PostAsync("http://127.0.0.1:5000/api/gameunit/create", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<ResponseModel>(responseContent);
                string at = responseObject.response_data.access_token;
                HttpContext.Session.SetString("GameAccessToken", at);
                return RedirectToPage("/addgame");
            }
            else
            {
                _logger.LogError("Error posting game data: {StatusCode}", response.StatusCode);
                ModelState.AddModelError(string.Empty, "There was an error saving the game.");
                return Page();
            }
        }


    }

}
